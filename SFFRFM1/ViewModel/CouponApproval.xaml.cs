using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using MySqlConnector;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SFFRFM1.Model;

namespace SFFRFM1.ViewModel
{
    public partial class CouponApproval : ContentPage
    {
        private List<Coupon> coupons;
        private int RegistrationYear = DateTime.UtcNow.Year;
        int quantity;
        int disID;

        // List to store selected farmers
        private List<Farmer> selectedFarmers = new List<Farmer>();

        public CouponApproval()
        {
            InitializeComponent();
            LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                {
                    await connection.OpenAsync();

                    coupons = await LoadCoupons(connection);

                    // Group farmers by region
                    var groupedFarmers = (await LoadFarmers(connection))
                        .GroupBy(farmer => farmer.Region)
                        .ToDictionary(group => group.Key, group => group.ToList());

                    StackLayout stackLayout = new StackLayout();

                    foreach (var region in groupedFarmers.Keys)
                    {
                        TableView tableView = new TableView();
                        TableRoot tableRoot = new TableRoot();

                        // Add region name as section header
                        TableSection regionSection = new TableSection("Region: "+region+"  Coupons Distributed: "+quantity+"");

                        foreach (var farmer in groupedFarmers[region])
                        {
                            // Create a Switch for each farmer
                            Switch farmerSwitch = new Switch
                            {
                                BindingContext = farmer,
                                HorizontalOptions = LayoutOptions.End
                            };
                            farmerSwitch.Toggled += (sender, e) =>
                            {
                                var selectedFarmer = (Farmer)((Switch)sender).BindingContext;
                                if (e.Value)
                                {
                                    selectedFarmers.Add(selectedFarmer); // Add farmer to selectedFarmers list
                                }
                                else
                                {
                                    selectedFarmers.Remove(selectedFarmer); // Remove farmer from selectedFarmers list
                                }
                            };

                            regionSection.Add(new TextCell
                            {
                                Text = $"Fullname: {farmer.Fullname} - National ID: {farmer.NationalID} - Status: {farmer.Status}",
                                Detail = $"Email: {farmer.Email}",
                                TextColor = Color.Black,
                                IsEnabled = false
                            });

                            regionSection.Add(new ViewCell { View = farmerSwitch });
                        }

                        tableRoot.Add(regionSection);
                        tableView.Root = tableRoot;

                        stackLayout.Children.Add(tableView);
                    }

                    Button approveButton = new Button
                    {
                        Text = "Distribute Coupons",
                        BackgroundColor = Color.FromHex("025409"),
                        TextColor = Color.White,
                        Margin = new Thickness(20, 10, 20, 0)
                    };
                    approveButton.Clicked += async (sender, e) =>
                    {
                        if (selectedFarmers.Count == 0)
                        {
                            await DisplayAlert("Error", "Select at least one farmer to approve coupons.", "OK");
                            return;
                        }

                        try
                        {
                            using (MySqlConnection approveConnection = new MySqlConnection(DatabaseSettings.ConnectionString))
                            {
                                await approveConnection.OpenAsync();
                                await ApproveData(approveConnection);
                            }
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Error", $"An error occurred while approving data: {ex.Message}", "OK");
                        }
                    };

                    stackLayout.Children.Add(approveButton);

                    Content = stackLayout;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task ApproveData(MySqlConnection connection)
        {
            try
            {
                List<string> couponCodes = GenerateCouponCodes(selectedFarmers);

                foreach (var farmer in selectedFarmers)
                {
                    int type = 1;
                    string couponCode = couponCodes.FirstOrDefault(c => c.StartsWith($"{farmer.Region}{farmer.NationalID}"));
                    if (!string.IsNullOrEmpty(couponCode))
                    {
                        if (quantity >= 0)
                        {
                            MySqlCommand insertCommand = new MySqlCommand("INSERT INTO CouponApproval (FarmerID, CouponCode, Type) VALUES (@FarmerID, @CouponCode, @Type)", connection);
                            insertCommand.Parameters.AddWithValue("@FarmerID", farmer.FarmerID);
                            insertCommand.Parameters.AddWithValue("@CouponCode", couponCode);
                            insertCommand.Parameters.AddWithValue("@Type", type);
                            await insertCommand.ExecuteNonQueryAsync();


                            // Update farmer type
                            MySqlCommand updateCommand = new MySqlCommand("UPDATE farmer SET Type = 0 WHERE FarmerID = @FarmerID", connection);
                            updateCommand.Parameters.AddWithValue("@FarmerID", farmer.FarmerID);
                            await updateCommand.ExecuteNonQueryAsync();

                            int q = quantity - 1;
                            MySqlCommand updateCommand1 = new MySqlCommand("UPDATE coupon SET Quantity = "+q+" WHERE DistributionID = "+disID+"", connection);
                            updateCommand1.Parameters.AddWithValue("@DistributionID", disID);
                            await updateCommand1.ExecuteNonQueryAsync();



                            // Send email with coupon code to farmer
                            await SendCouponCodeEmail(farmer.Email, farmer.Fullname, couponCode);
                        }
                        else
                        {
                            await DisplayAlert("Success", "No coupons remaining.", "OK");

                        }
                    }
                }

                await DisplayAlert("Success", "Coupon codes sent to respective farmers.", "OK");

                // Reload data after approval
                await LoadData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while approving data: {ex.Message}", "OK");
            }
        }


        private async Task<List<Farmer>> LoadFarmers(MySqlConnection connection)
        {
            List<Farmer> farmers = new List<Farmer>();
            string query = $"SELECT FarmerID, Fullname, Email, Status, Region, NationalID FROM farmer WHERE RegistrationYear = {RegistrationYear} and Type = 1 ORDER BY Region";
            MySqlCommand command = new MySqlCommand(query, connection);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Farmer farmer = new Farmer
                    {
                        FarmerID = reader.GetInt32("FarmerID"),
                        Email = reader.GetString("Email"),
                        Fullname = reader.GetString("Fullname"),
                        Region = reader.GetString("Region"),
                        NationalID = reader.GetString("NationalID"),
                        Status = reader.GetString("Status")
                    };
                    farmers.Add(farmer);
                }
            }
            return farmers;
        }

        private async Task<List<Coupon>> LoadCoupons(MySqlConnection connection)
        {
            List<Coupon> coupons = new List<Coupon>();
            string query = $"SELECT DistributionID, Region, Quantity FROM coupon WHERE DistributeYear = {RegistrationYear} ORDER BY Region";
            MySqlCommand command = new MySqlCommand(query, connection);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string region = reader.GetString("Region");
                    quantity = reader.GetInt32("Quantity");
                    disID = reader.GetInt32("Quantity");
                    coupons.Add(new Coupon { Region = region, Quantity = quantity });
                }
            }
            return coupons;
        }

        private List<string> GenerateCouponCodes(List<Farmer> farmers)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            List<string> couponCodes = new List<string>();

            foreach (var farmer in farmers)
            {
                string couponCode = $"{farmer.Region}{farmer.NationalID}"; // Combine Region and NationalID

                // Append random characters from the 'chars' constant to make the coupon code unique
                couponCode += new string(Enumerable.Repeat(chars, 8)
                                                .Select(s => s[random.Next(s.Length)])
                                                .ToArray());

                couponCodes.Add(couponCode);
            }

            return couponCodes;
        }

        private async Task SendCouponCodeEmail(string recipientEmail, string recipientName, string couponCode)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("mungoshiyacc4@gmail.com");

                if (!string.IsNullOrEmpty(recipientEmail))
                {
                    mail.To.Add(recipientEmail);
                }

                mail.Subject = "Your Coupon Code";
                mail.Body = $"Dear {recipientName},\n\nCoupons have been distributed to you and \nYour Coupon Code is: {couponCode}\n\nBest regards,\nThe SFFRFM Team";

                SmtpServer.Port = 587;
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new NetworkCredential("mungoshiyacc4@gmail.com", "dvdq cqhs ixvv qrso");

                await SmtpServer.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., display an error message or log it.
            }
        }
    }
}
