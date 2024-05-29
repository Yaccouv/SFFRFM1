using System;
using System.Threading.Tasks;
using MySqlConnector;
using SFFRFM1.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Common;
using ZXing.Net.Mobile.Forms;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRcode : ContentPage
    {
        private string userEmail;
        private int farmerID;
        private StackLayout qrCodeContainer;

        public QRcode(string userEmail)
        {
            this.userEmail = userEmail;
            InitializeComponent();
            InitializeUI();
        }

        private async void InitializeUI()
        {
            // Title label
            var titleLabel = new Label
            {
                Text = "Coupon QR-Code",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 5
                FontAttributes = FontAttributes.Bold, // Set font to bold
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, 20, 0, 20)
            };

            // Create a StackLayout to contain the QR code
            qrCodeContainer = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            InitializeQRCode();

            // Create a curved frame to contain the title label and QR code container
            var curvedFrame = new Frame
            {
                HasShadow = true, // Add shadow to the frame
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
                    qrCodeContainer
                }
            };

            // Set the content of the page to the curved frame
            Content = curvedFrame;
        }

        private async Task<int> GetFarmerID()
        {
            string query = "SELECT FarmerID FROM farmer WHERE Email = @Email";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", userEmail);

                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            throw new Exception("No FarmerID found for the provided email.");
        }

        private async Task<string> GetCouponCode(int farmerID)
        {
            string query = "SELECT CouponCode FROM couponApproval WHERE FarmerID = @FarmerID";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FarmerID", farmerID);

                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                }
            }

            throw new Exception("No CouponCode found for the provided FarmerID.");
        }

        private async void InitializeQRCode()
        {
            try
            {
                // Get FarmerID
                farmerID = await GetFarmerID();

                // Check if FarmerID exists in payment table
                bool farmerExistsInPaymentTable = await CheckFarmerIDInPaymentTable(farmerID);

                if (farmerExistsInPaymentTable)
                {
                    // Check if FarmerID exists in couponApproval table
                    bool farmerExistsInCouponApprovalTable = await CheckFarmerIDInCouponApprovalTable(farmerID);

                    if (farmerExistsInCouponApprovalTable)
                    {
                        // Get CouponCode
                        string couponCode = await GetCouponCode(farmerID);

                        // Create QR Code with CouponCode
                        ZXingBarcodeImageView QRCodeView = new ZXingBarcodeImageView
                        {
                            IsVisible = true,
                            HeightRequest = 300,
                            WidthRequest = 300,
                            BarcodeValue = couponCode
                        };

                        QRCodeView.BarcodeOptions = new EncodingOptions
                        {
                            Width = 300,
                            Height = 300
                        };

                        StackLayout stackLayout = new StackLayout();
                        stackLayout.Children.Add(QRCodeView);

                        qrCodeContainer.Children.Add(stackLayout); // Add QR Code to layout
                    }
                    else
                    {
                        await DisplayAlert("Error", "Farmer ID does not exist in couponApproval table.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Farmer ID does not exist in payment table.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<bool> CheckFarmerIDInPaymentTable(int farmerID)
        {
            string query = "SELECT COUNT(*) FROM payment WHERE FarmerID = @FarmerID";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FarmerID", farmerID);
                    long count = (long)await command.ExecuteScalarAsync();

                    return count > 0;
                }
            }
        }

        private async Task<bool> CheckFarmerIDInCouponApprovalTable(int farmerID)
        {
            string query = "SELECT COUNT(*) FROM couponApproval WHERE FarmerID = @FarmerID";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FarmerID", farmerID);
                    long count = (long)await command.ExecuteScalarAsync();

                    return count > 0;
                }
            }
        }
    }
}
