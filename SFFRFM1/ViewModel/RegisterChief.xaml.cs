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
    public partial class RegisterChief : ContentPage
    {
        private Entry txtFullname;
        private Entry txtPhone;
        private Entry txtEmail;
        private Picker regionPicker;
        private Button btnRegister;

        private string[] regionOptions = { "Blantyre", "Zomba", "Lilongwe", "Mzuzu" };
        private bool emailSent = false;

         int type = 2;

        public RegisterChief()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Title
            var titleLabel = new Label
            {
                Text = "Register Chief",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 2
                FontAttributes = FontAttributes.Bold, // Set font to bold
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, 20, 0, 20)
            };

            // Create textboxes
            txtFullname = new Entry { Placeholder = "Fullname", Margin = new Thickness(20, 0, 20, 10) };
            txtPhone = new Entry { Placeholder = "Phone", Margin = new Thickness(20, 0, 20, 10) };
            txtEmail = new Entry { Placeholder = "Email",  Margin = new Thickness(20, 0, 20, 10) };

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
            btnRegister = new Button
            {
                Text = "SUBMIT",
                BackgroundColor = Color.FromHex("025409"),
                TextColor = Color.White,
                Margin = new Thickness(20, 10, 20, 0)
            };
            btnRegister.Clicked += BtnRegister_Clicked;

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
                    txtFullname,
                    txtPhone,
                    txtEmail,
                    regionPicker,
                    btnRegister
                }
            };

            // Set the content of the page to the curved frame
            Content = curvedFrame;
        }

        private void BtnRegister_Clicked(object sender, EventArgs e)
        {
            RegisterUser();
        }

        private void RegisterUser()
        {
            if (!string.IsNullOrWhiteSpace(txtFullname.Text) &&
                !string.IsNullOrWhiteSpace(txtPhone.Text) &&
                !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                regionPicker.SelectedItem != null)
            {
                // Generate a random password
                string password = GenerateRandomPassword();

                var chief = new Chief
                {
                    Fullname = txtFullname.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,
                    Region = regionPicker.SelectedItem.ToString(),
                    Password = password, // Save the generated password
                    Type = type
                };

                try
                {
                    // Insert the user into the database

                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();
                        string query = "INSERT INTO chief (Fullname, Phone, Email, Region, Password, Type) VALUES (@Fullname, @Phone, @Email, @Region, @Password, @Type)";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Fullname", chief.Fullname);
                        command.Parameters.AddWithValue("@Phone", chief.Phone);
                        command.Parameters.AddWithValue("@Email", chief.Email);
                        command.Parameters.AddWithValue("@Region", chief.Region);
                        command.Parameters.AddWithValue("@Password", chief.Password);
                        command.Parameters.AddWithValue("@type", chief.Type);
                        command.ExecuteNonQuery();
                    }

                    // Send email with password
                    SendPasswordEmail(chief.Email, password);

                    // Display success message
                    DisplayAlert("Success", "User registered successfully. Check your email for the password.", "OK");

                    // Clear input fields
                    txtFullname.Text = string.Empty;
                    txtPhone.Text = string.Empty;
                    txtEmail.Text = string.Empty;
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

        private string GenerateRandomPassword()
        {
            // Generate a random password of 8 characters
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SendPasswordEmail(string recipientEmail, string password)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("mungoshiyacc4@gmail.com");

                if (!string.IsNullOrEmpty(txtEmail.Text))
                {
                    mail.To.Add(txtEmail.Text); 
                }

                mail.Subject = "Your SFFRFM chief Password";
                mail.Body = $"Your Password is: {password}";

                SmtpServer.Port = 587;
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new NetworkCredential("mungoshiyacc4@gmail.com", "wzjh qyhj bkgf xkja");

                SmtpServer.Send(mail);


            }
            catch (Exception ex)
            {
                //Handle the exception, e.g., display an error message or log it.
                //DisplayAlert("Failed", ex.Message, "OK");
            }
            emailSent = true;
        }
            
    }
}
