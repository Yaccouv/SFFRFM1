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
    public partial class AddFarmer : ContentPage
    {
        private Entry txtFullname;
        private Entry txtEmail;
        private Entry txtNationalID;
        private Picker regionPicker;
        private Picker StatusPicker;
        private Picker GenderPicker;
        private Button btnRegister;

        private string[] regionOptions = { "Blantyre", "Zomba", "Lilongwe", "Mzuzu" };
        private string[] StatusOptions = { "Own", "Rented" };
        private string[] GenderOptions = { "Male", "Female" };
        private bool emailSent = false;

        int type = 1;

        public AddFarmer()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Title
            var titleLabel = new Label
            {
                Text = "Register Farmer",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) + 5, // Increase the font size by 2
                FontAttributes = FontAttributes.Bold, // Set font to bold
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(0, 20, 0, 20)
            };

            // Create textboxes
            txtFullname = new Entry { Placeholder = "Fullname", Margin = new Thickness(20, 0, 20, 10) };
            txtNationalID = new Entry { Placeholder = "National ID", Margin = new Thickness(20, 0, 20, 10) };
            txtEmail = new Entry { Placeholder = "Email", Margin = new Thickness(20, 0, 20, 10) };

            // Create region picker
            GenderPicker = new Picker
            {
                Title = "Select Gender",
                Margin = new Thickness(20, 0, 20, 10)
            };
            regionPicker = new Picker
            {
                Title = "Select Region",
                Margin = new Thickness(20, 0, 20, 10)
            };
            StatusPicker = new Picker
            {
                Title = "Select Land Status",
                Margin = new Thickness(20, 0, 20, 10)
            };
            foreach (var region in regionOptions)
            {
                regionPicker.Items.Add(region);
            }

            foreach (var gender in GenderOptions)
            {
                GenderPicker.Items.Add(gender);
            }
            foreach (var status in StatusOptions)
            {
                StatusPicker.Items.Add(status);
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
                    txtNationalID,
                    txtEmail,
                    regionPicker,
                    StatusPicker,
                    GenderPicker,
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
                !string.IsNullOrWhiteSpace(txtNationalID.Text) &&
                !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                regionPicker.SelectedItem != null)
            {
                // Generate a random password
                string password = GenerateRandomPassword();

                var farmer = new Farmer
                {
                    Fullname = txtFullname.Text,
                    NationalID = txtNationalID.Text,
                    Email = txtEmail.Text,
                    Region = regionPicker.SelectedItem.ToString(),
                    Gender = GenderPicker.SelectedItem.ToString(),
                    RegistrationYear = DateTime.UtcNow.Year,
                    Status = StatusPicker.SelectedItem.ToString(),
                    Password = password, // Save the generated password
                    Type = type
                };

                try
                {
                    // Insert the user into the database

                    using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
                    {
                        connection.Open();
                        string query = "INSERT INTO farmer (Fullname, NationalID, Email, Region, Gender, Status, Password, Type, RegistrationYear) VALUES (@Fullname, @NationalID, @Email, @Region, @Gender, @Status, @Password, @Type, @RegistrationYear)";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Fullname", farmer.Fullname);
                        command.Parameters.AddWithValue("@NationalID", farmer.NationalID);
                        command.Parameters.AddWithValue("@Email", farmer.Email);
                        command.Parameters.AddWithValue("@Region", farmer.Region);
                        command.Parameters.AddWithValue("@Gender", farmer.Gender);
                        command.Parameters.AddWithValue("@Status", farmer.Status);
                        command.Parameters.AddWithValue("@Password", farmer.Password);
                        command.Parameters.AddWithValue("@RegistrationYear", farmer.RegistrationYear);
                        command.Parameters.AddWithValue("@type", farmer.Type);
                        command.ExecuteNonQuery();
                    }

                    // Send email with password
                    SendPasswordEmail(farmer.Email, password);

                    // Display success message
                    DisplayAlert("Success", "User registered successfully. Check your email for the password.", "OK");

                    // Clear input fields
                    txtFullname.Text = string.Empty;
                    txtNationalID.Text = string.Empty;
                    txtEmail.Text = string.Empty;
                    regionPicker.SelectedIndex = -1; // Reset picker selection
                    GenderPicker.SelectedIndex = -1;
                    StatusPicker.SelectedIndex = -1;
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
