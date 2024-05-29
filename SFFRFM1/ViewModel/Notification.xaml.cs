using System;
using System.Threading.Tasks;
using MySqlConnector;
using SFFRFM1.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Notification : ContentPage
    {
        private string userEmail;

        public Notification(string userEmail)
        {
            InitializeComponent();
            this.userEmail = userEmail;
            LoadCouponCode();
        }

        private async Task<string> GetCouponCode(int farmerID)
        {
            string query = "SELECT CouponCode FROM CouponApproval WHERE FarmerID = @FarmerID";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                connection.Open();

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

            throw new Exception("No coupon code found.");
        }

        private async Task<int> GetFarmerID(string userEmail)
        {
            string query = "SELECT FarmerID FROM farmer WHERE Email = @Email";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                 connection.Open();

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

            throw new Exception("No farmer ID found for the provided email.");
        }

        private async void LoadCouponCode()
        {
            try
            {
                int farmerID = await GetFarmerID(userEmail);
                string couponCode = await GetCouponCode(farmerID);

                // Title
                var titleLabel = new Label
                {
                    Text = "Coupon Code",
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 2
                    FontAttributes = FontAttributes.Bold, // Set font to bold
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Margin = new Thickness(0, 20, 0, 20)
                };

                // Display coupon code
                var couponLabel = new Label
                {
                    Text = "Your Coupon Code is: " + couponCode,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

               
                // StackLayout to hold coupon label and button
                var stackLayout = new StackLayout
                {
                    Children = { titleLabel, couponLabel},
                    Padding = new Thickness(20),
                    Spacing = 20
                };

                // Create a curved frame
                Frame frame = new Frame
                {
                    Content = stackLayout,
                    HasShadow = true, // Add shadow to the frame
                    CornerRadius = 20,
                    Padding = new Thickness(20),
                    BackgroundColor = Color.FromHex("#f0f5f0"),
                    Margin = new Thickness(20)
                };

                Content = frame;
            }
            catch (Exception ex)
            {
                // Handle the exception
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


       

    }
}
