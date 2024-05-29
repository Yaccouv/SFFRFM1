using System;
using System.Threading.Tasks;
using MySqlConnector;
using SFFRFM1.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRScanner : ContentPage
    {
        private string userEmail;
        private int farmerID;
        private StackLayout qrCodeContainer;

        public QRScanner(string userEmail)
        {
            InitializeComponent();
            this.userEmail = userEmail;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create a StackLayout to contain the QR code and scan button
            qrCodeContainer = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            // Create a button to scan QR code
            Button scanButton = new Button
            {
                Text = "Scan QR Code",
                BackgroundColor = Color.FromHex("025409"),
                TextColor = Color.White,
                Margin = new Thickness(20, 10, 20, 0)
            };
            scanButton.Clicked += ScanButton_Clicked;

            // Add the scan button to the layout
            qrCodeContainer.Children.Add(scanButton);

            // Set the content of the page to the StackLayout
            Content = qrCodeContainer;
        }

        private async void ScanButton_Clicked(object sender, EventArgs e)
        {
            // Initialize the QR code scanner page
            var scanPage = new ZXingScannerPage();

            // Subscribe to the scan result event
            scanPage.OnScanResult += (result) =>
            {
                // Stop scanning
                scanPage.IsScanning = false;


                // Pop the scanner page from the navigation stack
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();

                    // Process the scanned QR code
                    await ProcessScannedQRCode(result.Text);

                });
            };

            // Navigate to the scanner page
            await Navigation.PushAsync(scanPage);
        }

        private async Task ProcessScannedQRCode(string qrCodeValue)
        {
            try
            {
                // Check if the scanned QR code exists in the couponApproval table and retrieve the FarmerID
                var (exists, farmerID) = await CheckCouponCodeExists(qrCodeValue);

                if (exists)
                {
                    // Query the database to fetch Farmer's details using the retrieved FarmerID
                    string farmerDetailsQuery = "SELECT couponApproval.CouponCode, couponApproval.Type, farmer.FullName, farmer.NationalID, farmer.Region, payment.NumberOfBags FROM farmer JOIN couponApproval ON farmer.FarmerID = couponApproval.FarmerID JOIN payment ON farmer.FarmerID = payment.FarmerID WHERE farmer.FarmerID = @FarmerID";

                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();

                        using (MySqlCommand command = new MySqlCommand(farmerDetailsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@FarmerID", farmerID);

                            using (MySqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    // Retrieve Farmer's details
                                    string couponCode = reader.GetString("CouponCode");
                                    string fullName = reader.GetString("FullName");
                                    string nationalID = reader.GetString("NationalID");
                                    string region = reader.GetString("Region");
                                    int numberOfBags = reader.GetInt32("NumberOfBags");
                                    int type = reader.GetInt32("Type");

                                    // Display Farmer's details
                                    if (type == 1)
                                    {
                                        string message = $"Coupon Code: {couponCode}\nFull Name: {fullName}\nNational ID: {nationalID}\nRegion: {region}\nNumber of Bags: {numberOfBags}";
                                        await DisplayAlert("Success", message, "OK");

                                        // Update the Type column in couponApproval table to 0
                                        string updateQuery = "UPDATE couponApproval SET Type = 0 WHERE FarmerID = @FarmerID";

                                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                                        {
                                            updateCommand.Parameters.AddWithValue("@Type", 0);
                                            await updateCommand.ExecuteNonQueryAsync();
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Error", "Coupon Already Used.", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Error", "No Farmer details found.", "OK");
                                }
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Coupon code not found in the couponApproval table.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }



        private async Task<(bool exists, int farmerID)> CheckCouponCodeExists(string couponCode)
        {
            string query = "SELECT FarmerID FROM couponApproval WHERE CouponCode = @CouponCode";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CouponCode", couponCode);
                    object result = await command.ExecuteScalarAsync();

                    if (result != null)
                    {
                        int farmerID = Convert.ToInt32(result);
                        return (true, farmerID);
                    }
                    else
                    {
                        return (false, -1); // Return -1 as FarmerID if the coupon code doesn't exist
                    }
                }
            }
        }



    }
}
