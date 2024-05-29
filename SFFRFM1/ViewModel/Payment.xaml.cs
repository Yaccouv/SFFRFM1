using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using MySqlConnector;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using SFFRFM1.Model;
using System.Linq;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Payment : ContentPage
    {
        Entry cardNumberEntry;
        Entry expirationMonthEntry;
        Entry expirationYearEntry;
        Entry numberOfBagsEntry;
        Button confirmPaymentButton;
        private string userEmail;

        public Payment(string userEmail)
        {
            InitializeComponent();
            InitializeUI();
            this.userEmail = userEmail;
        }

        private async void InitializeUI()
        {
            int farmerID = await GetFarmerID();
            string couponCode = await GetCouponCode(farmerID);

            // Title
            var titleLabel = new Label
            {
                Text = "Make Your Coupon Payment",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 2
                FontAttributes = FontAttributes.Bold, // Set font to bold
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, 20, 0, 20)
            
            };
            var couponLabel = new Label
            {
                Text = "Your Coupon Code is: " + couponCode,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Create entry for card number
            cardNumberEntry = new Entry
                {
                    Placeholder = "Card Number",
                    Keyboard = Keyboard.Numeric,
                Margin = new Thickness(20, 0, 20, 10)
            };

                cardNumberEntry.TextChanged += (sender, e) =>
                {
                    var text = e.NewTextValue;
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Remove any non-numeric characters
                        text = new string(text.Where(char.IsDigit).ToArray());

                        // Insert spaces after every 4 characters
                        if (text.Length > 4)
                            text = text.Insert(4, " ");
                        if (text.Length > 9)
                            text = text.Insert(9, " ");
                        if (text.Length > 14)
                            text = text.Insert(14, " ");

                        // Limit the input to 19 characters
                        if (text.Length > 19)
                            text = text.Substring(0, 19);

                        // Update the entry text
                        cardNumberEntry.Text = text;
                    }
                };

                // Create entry for expiration month
                expirationMonthEntry = new Entry
                {
                    Placeholder = "MM",
                    Keyboard = Keyboard.Numeric,
                    Margin = new Thickness(20, 0, 20, 10),
                    MaxLength = 2
                };

                // Create entry for expiration year
                expirationYearEntry = new Entry
                {
                    Placeholder = "YY",
                    Keyboard = Keyboard.Numeric,
                    Margin = new Thickness(20, 0, 20, 10),
                    MaxLength = 2
                };

                // Create entry for number of bags
                numberOfBagsEntry = new Entry
                {
                    Placeholder = "Number of Bags",
                    Keyboard = Keyboard.Numeric,
                    Margin = new Thickness(20, 0, 20, 10)
                };

                // Create a stack layout to organize the entries vertically
                StackLayout entryLayout = new StackLayout
                {
                    Children =
                    {
                        titleLabel,
                        couponLabel,
                        cardNumberEntry,
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                expirationMonthEntry,
                                new BoxView { BackgroundColor = Color.Black, WidthRequest = 2, VerticalOptions = LayoutOptions.FillAndExpand },
                                expirationYearEntry
                            }
                        },
                        numberOfBagsEntry
                    },
                                Spacing = 10 // Add spacing between entries
                };

                // Create a frame to contain the card information and bags entry
                Frame cardInfoFrame = new Frame
                {
                    Content = entryLayout,
                    HasShadow = true, // Add shadow to the frame
                    CornerRadius = 20,
                    Padding = new Thickness(20),
                    BackgroundColor = Color.FromHex("#f0f5f0"),
                    Margin = new Thickness(20)
                };

                // Add the card information frame to the content of the page
                Content = cardInfoFrame;

                // Initialize button for confirming payment
                confirmPaymentButton = new Button
                {
                    Text = "Confirm Payment",
                    BackgroundColor = Color.FromHex("025409"),
                    TextColor = Color.White,
                    Margin = new Thickness(20, 10, 20, 0)
                };
                confirmPaymentButton.Clicked += ConfirmPaymentButton_Clicked;

                // Add the confirm payment button to the layout
                Content = new StackLayout
                {
                    Children = { cardInfoFrame, confirmPaymentButton }
                };
            }
        

            private async void ConfirmPaymentButton_Clicked(object sender, EventArgs e)
        {
            await HandlePayment();
        }

        private async Task HandlePayment()
        {
            try
            {
                // Set your secret key. Remember to switch to your live secret key in production.
                StripeConfiguration.ApiKey = "sk_test_51OtrDhEt3C29p5IeDisMmc4qaAAP1UDxCcK3Lddnrog3XCA7v2kQEoYEYH1rNU7laCSDAnPgzlLe6w8DI6X8Ch3c002PMd0LE1";

                // Fetch coupon details from your data source
                double couponAmount = await GetDistinctAmountFromCoupon();

                // Get the total amount in MWK from coupon
                double totalPriceMWK = Convert.ToDouble(couponAmount);

                // Retrieve the number of bags from user input
                int numberOfBags = Convert.ToInt32(numberOfBagsEntry.Text); // Assuming user enters valid numeric input

                // Calculate the final amount by multiplying the number of bags with the coupon amount
                double finalAmount = totalPriceMWK * numberOfBags;

                // Create a payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = Convert.ToInt64(finalAmount * 100), // Amount is in cents
                    Currency = "mwk", // Malawi Kwacha
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                Console.WriteLine($"Client secret: {paymentIntent.ClientSecret}");

                var paymentMethodId = "pm_card_visa"; // Replace with the desired payment method ID

                // Confirm the payment intent with the payment method
                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };
                var confirmService = new PaymentIntentService();
                var confirmedIntent = await confirmService.ConfirmAsync(paymentIntent.Id, confirmOptions);

                // Handle the payment response
                if (confirmedIntent.Status == "succeeded")
                {
                    int farmerID = await GetFarmerID();
                    var paymentClass = new PaymentClass
                    {
                        Amount = finalAmount,
                        FarmerID = farmerID,
                        NumberOfBags = numberOfBags,
                    };


                    // Insert the payment into the database

                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();
                        string query = "INSERT INTO payment (Amount, FarmerID, NumberOfBags) VALUES (@Amount, @FarmerID, @NumberOfBags)";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Amount", paymentClass.Amount);
                        command.Parameters.AddWithValue("@FarmerID", paymentClass.FarmerID);
                        command.Parameters.AddWithValue("@NumberOfBags", paymentClass.NumberOfBags);
                        command.ExecuteNonQuery();
                    }
                    await DisplayAlert("Success", "Payment successful!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Payment failed.", "OK");
                    // Handle failed payment (e.g., display error message to user)
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Method to fetch distinct amount from coupon table
        private async Task<double> GetDistinctAmountFromCoupon()
        {
           
            string query = "SELECT DISTINCT Amount FROM Coupon";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetDouble(0);
                        }
                    }
                }
            }

            throw new Exception("No distinct amount found in Coupon table.");
        }

        private async Task<int> GetFarmerID()
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

            throw new Exception("No FarmerID found for the provided email.");
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
     
    }
}
