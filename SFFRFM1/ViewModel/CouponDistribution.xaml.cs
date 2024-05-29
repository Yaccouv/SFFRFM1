using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MySqlConnector;
using SFFRFM1.Model;
using System.Runtime.InteropServices.ComTypes;
using Xamarin.Essentials;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CouponDistribution : ContentPage
    {
        private Entry txtQuantity;
        private Entry txtAmount;
        private Picker regionPicker;
        private Button btnDisribute;

        private string[] regionOptions = { "Blantyre", "Zomba", "Lilongwe", "Mzuzu" };
        

        public CouponDistribution()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Title
            var titleLabel = new Label
            {
                Text = "Distribute Coupons to Regions",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 2
                FontAttributes = FontAttributes.Bold, // Set font to bold
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, 20, 0, 20)
            };


            // Create textboxes
            txtQuantity = new Entry { Placeholder = "Quantity", Margin = new Thickness(20, 0, 20, 10) };
            txtAmount = new Entry { Placeholder = "Coupon Amount", Margin = new Thickness(20, 0, 20, 10) };

            // Create region picker
            regionPicker = new Picker
            {
                Title = "Select Region",
                Margin = new Thickness(20, 0, 20, 10)
            };
            foreach (var region in regionOptions)
            {
                regionPicker.Items.Add(region);
            }

            // Create register button
            btnDisribute = new Button
            {
                Text = "SUBMIT",
                BackgroundColor = Color.FromHex("025409"),
                TextColor= Color.White,
                Margin = new Thickness(20, 10, 20, 0)
            };
            btnDisribute.Clicked += BtnDistribute_Clicked;

            // Create a curved frame to contain the form elements
            var curvedFrame = new Frame
            {
                CornerRadius = 20,
                Padding = new Thickness(20),
                BackgroundColor = Color.FromHex("#f0f5f0"),
                Margin = new Thickness(20)
            };
            curvedFrame.Content = new StackLayout
            {
                Children =
                {
                    
                    titleLabel,
                    regionPicker,
                    txtQuantity,
                    txtAmount,
                    btnDisribute
                }
            };

            // Set the content of the page to the curved frame
            Content = curvedFrame;
        }

        private void BtnDistribute_Clicked(object sender, EventArgs e)
        {
            Distribute();
        }

        private void Distribute()
        {
            if (!string.IsNullOrWhiteSpace(txtQuantity.Text) &&
                regionPicker.SelectedItem != null)
            {

                var coupon = new Coupon
                {
                    Quantity = Convert.ToInt32(txtQuantity.Text),
                    Region = regionPicker.SelectedItem.ToString(),
                    DistributeYear = DateTime.UtcNow.Year,
                    Amount = Convert.ToDouble(txtAmount.Text),
                };

                try
                {
                    // Insert the user into the database

                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();
                        string query = "INSERT INTO Coupon (Region, Quantity, DistributeYear, Amount) VALUES (@Region, @Quantity, @DistributeYear, @Amount)";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Quantity", coupon.Quantity);
                        command.Parameters.AddWithValue("@Region", coupon.Region);
                        command.Parameters.AddWithValue("@DistributeYear", coupon.DistributeYear);
                        command.Parameters.AddWithValue("@Amount", coupon.Amount);
                        command.ExecuteNonQuery();
                    }

                    // Display success message
                    DisplayAlert("Success", "Coupons successfully distributed.", "OK");

                    // Clear input fields
                    txtQuantity.Text = string.Empty;
                    txtAmount.Text = string.Empty;
                    regionPicker.SelectedIndex = -1; // Reset picker selection
                }
                catch (Exception ex)
                {
                    // Handle database error
                    DisplayAlert("Error", $"Failed to register user: {ex.Message}", "OK");
                }
            }
            else
            {
                DisplayAlert("Error", "Please fill in all fields", "OK");
            }
        }

    }
}
