using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Mail;
using MySqlConnector;
using SFFRFM1.Model;

namespace SFFRFM1.ViewModel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        private Entry txtEmail;
        private Entry txtPassword;
        private Button btnLogin;

        public Login()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Image
            var logoImage = new Image
            {
                Source = "back1.jpg", // Replace "back1.jpg" with the path to your image file
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0)
            };

            // Title
            var titleLabel = new Label
            {
                Text = "Welcome",
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(20, 20, 0, 10),
                HorizontalTextAlignment = TextAlignment.Start,
                FontFamily = "metropolis",
                TextColor = Color.FromHex("#4A4B4d")
            };

            var titleLabel1 = new Label
            {
                Text = "Login into Your Account",
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(20, 0, 0, 0),
                HorizontalTextAlignment = TextAlignment.Start,
                FontFamily = "metropolis",
                TextColor = Color.FromHex("#4A4B4d")
            };

            // Username Entry
            var usernameFrame = new Frame
            {
                HasShadow = true,
                CornerRadius = 30,
                BorderColor = Color.WhiteSmoke,
                BackgroundColor = Color.White,
                WidthRequest = 350,
                HeightRequest = 20, // Adjust height as needed
                Margin = new Thickness(20, 40, 20, 15)
            };

            var usernameStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            var userIcon = new Image
            {
                Source = "user1.png",
                WidthRequest = 30,
                HorizontalOptions = LayoutOptions.Start
            };

            txtEmail = new Entry
            {
                Placeholder = "Email                                ",
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = 15,
                PlaceholderColor = Color.FromHex("#4A4B4d"),
                TextColor = Color.Black,
                Margin = new Thickness(15, -30, 30, -30)
            };

            var eyeButton = new ImageButton
            {
                Source = "eyeclose.png",
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(110, 0, 0, 0),
                HeightRequest = 100
            };
            eyeButton.Clicked += HidePass_Clicked;

            usernameStackLayout.Children.Add(userIcon);
            usernameStackLayout.Children.Add(txtEmail);

            usernameFrame.Content = usernameStackLayout;

            // Password Entry
            var passwordFrame = new Frame
            {
                HasShadow = true,
                CornerRadius = 30,
                BorderColor = Color.WhiteSmoke,
                BackgroundColor = Color.White,
                WidthRequest = 350,
                HeightRequest = 20, // Adjust height as needed
                Margin = new Thickness(20, 0, 20, 10)
            };

            var passwordStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            var passwordIcon = new Image
            {
                Source = "pass.png", // Change to the path of your password icon
                WidthRequest = 30,
                HorizontalOptions = LayoutOptions.Start
            };

            txtPassword = new Entry
            {
                Placeholder = "Password                                ",
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = 15,
                PlaceholderColor = Color.FromHex("#4A4B4d"),
                TextColor = Color.Black,
                IsPassword = true,
                Margin = new Thickness(15, -30, 30, -30)
            };

            passwordStackLayout.Children.Add(passwordIcon);
            passwordStackLayout.Children.Add(txtPassword);
            passwordStackLayout.Children.Add(eyeButton);

            passwordFrame.Content = passwordStackLayout;

            // Forgot Password Label
            var forgotPasswordLabel = new Label
            {
                Text = "Forgot Password?",
                HorizontalTextAlignment = TextAlignment.End,
                FontSize = 18,
                FontFamily = "metropolis",
                TextColor = Color.Gray,
                Margin = new Thickness(0, 5, 20, 0)
            };

            // Create login button
            btnLogin = new Button
            {
                Text = "Login",
                BackgroundColor = Color.FromHex("#025409"),
                TextColor = Color.White,
                CornerRadius = 20, // Make it rounded
                Margin = new Thickness(20, 10, 20, 0)
            };
            btnLogin.Clicked += BtnLogin_Clicked;

            // Create a stack layout to contain all the elements
            var stackLayout = new StackLayout
            {
                Children =
            {
                logoImage,
                titleLabel,
                titleLabel1,
                usernameFrame,
                passwordFrame,
                forgotPasswordLabel,
                btnLogin
            }
                    };

            // Set the content of the page to the stack layout
            Content = stackLayout;
        }



        private void BtnLogin_Clicked(object sender, EventArgs e)
        {
            LoginUser();
        }

        private void LoginUser()
        {
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                try
                {
                    // Check credentials from the database
                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();

                        // Check if the user exists in the farmer table
                        string query = "SELECT COUNT(*) FROM farmer WHERE Email = @Email AND Password = @Password";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Email", txtEmail.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text);
                        long farmerCount = (long)command.ExecuteScalar();

                        // Check if the user exists in the chief table
                        query = "SELECT COUNT(*) FROM chief WHERE Email = @Email AND Password = @Password";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Email", txtEmail.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text);
                        long chiefCount = (long)command.ExecuteScalar();

                        // Check if the user exists in the sffrfm table
                        query = "SELECT COUNT(*) FROM sffrfmtable WHERE Email = @Email AND Password = @Password";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Email", txtEmail.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text);
                        long sffrfmCount = (long)command.ExecuteScalar();

                        if (farmerCount > 0)
                        {
                            // Create a new MasterPage instance
                            var masterPageFarmer = new MasterPageFarmer(txtEmail.Text);

                            // Set the Farmer data on the MasterPage
                            masterPageFarmer.SetFarmerUsername(txtEmail.Text);

                            // Navigate to the MasterPage
                            Navigation.PushAsync(masterPageFarmer);
                        }
                        else if (chiefCount > 0)
                        {
                            // User is a chief, navigate to chief page
                           Navigation.PushAsync(new MasterPageChief());
                        }
                        else if (sffrfmCount > 0)
                        {
                            // Create a new MasterPage instance
                            var masterPage = new MasterPage();

                            // Set the Farmer data on the MasterPage
                            masterPage.SetFarmerUsername(txtEmail.Text);

                            // Navigate to the MasterPage
                            Navigation.PushAsync(masterPage);
                        }
                        else
                        {
                            // Invalid credentials
                            DisplayAlert("Error", "Invalid email or password", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle database error
                    DisplayAlert("Error", $"Failed to login: {ex.Message}", "OK");
                }
            }
            else
            {
                DisplayAlert("Error", "Please fill in all fields", "OK");
            }
        }


        private void HidePass_Clicked(object sender, EventArgs e)
        {
            var imageButton = sender as ImageButton;
            if (txtPassword.IsPassword)
            {
                imageButton.Source = ImageSource.FromFile("eyeopen.png");
                txtPassword.IsPassword = false;
            }
            else
            {
                imageButton.Source = ImageSource.FromFile("eyeclose.png");
                txtPassword.IsPassword = true;
            }
        }

    }
}
