using MySqlConnector;
using SFFRFM1.Model;
using SFFRFM1.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SFFRFM1
{
    public partial class MasterPageFarmer : MasterDetailPage
    {
        private Farmer _farmer;
        string userEmail;
        List<MenuItemFarmer> menu;



        public MasterPageFarmer(string userEmail)
        {
            InitializeComponent();
            this.userEmail = userEmail;
            InitializePage();
        }

        private async void InitializePage()
        {
            try
            {
               
                NavigationPage.SetHasNavigationBar(this, false);
                menu = new List<MenuItemFarmer>();

                menu.Add(new MenuItemFarmer { OptionName = "Home", IconSource = ImageSource.FromFile("home1.jpeg") });
                menu.Add(new MenuItemFarmer { OptionName = "Buy", IconSource = ImageSource.FromFile("home1.jpeg") });
                menu.Add(new MenuItemFarmer { OptionName = "QR-Code", IconSource = ImageSource.FromFile("inc3.jpeg") });

                int farmerID = await GetFarmerID(userEmail);
                bool couponCodeExists = await CouponCodeExists(farmerID);

                if (couponCodeExists)
                {
                    menu.Add(new MenuItemFarmer { OptionName = "Notification(+1)", IconSource = ImageSource.FromFile("report2.png") });
                }
                else
                {
                    menu.Add(new MenuItemFarmer { OptionName = "Notification", IconSource = ImageSource.FromFile("report2.png") });
                }
                menu.Add(new MenuItemFarmer { OptionName = "Help", IconSource = ImageSource.FromFile("report2.png") });
                menu.Add(new MenuItemFarmer { OptionName = "Logout", IconSource = ImageSource.FromFile("report2.png") });

                navigationList.ItemsSource = menu;
                Detail = new NavigationPage(new DashboardFarmer());
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        public void SetFarmerUsername(string email)
        {
            userEmail = email;
            lblUsername.Text = "Hello " + email;
        }


        private async void Item_Tapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = e.Item as MenuItemFarmer;

                foreach (var menuItemFarmer in menu)
                {
                    menuItemFarmer.IsActive = (menuItemFarmer.OptionName == item.OptionName);
                }

                navigationList.ItemsSource = null;
                navigationList.ItemsSource = menu;

                foreach (var cell in navigationList.TemplatedItems)
                {
                    var viewCell = (ViewCell)cell;
                    var contentView = (ContentView)viewCell.View;

                    if (contentView.BindingContext == item)
                    {
                        contentView.BackgroundColor = Color.FromHex("#E5F0E5");
                    }
                    else
                    {
                        contentView.BackgroundColor = Color.White;
                    }
                }

                
                switch (item.OptionName)
                {
                    case "Home":
                        Detail = new NavigationPage(new DashboardFarmer());
                        IsPresented = false;
                        break;
                    case "Buy":
                        Detail = new NavigationPage(new Payment(userEmail));
                        IsPresented = false;
                        break;
                    case "QR-Code":
                        Detail = new NavigationPage(new QRcode(userEmail));
                        IsPresented = false;
                        break;
                    case "Notification":
                        Detail = new NavigationPage(new Notification(userEmail));
                        IsPresented = false;
                        break;
                    case "Notification(+1)":
                        Detail = new NavigationPage(new Notification(userEmail));
                        IsPresented = false;
                        break;
                    case "Help":
                        // Handle the "Reports" action here or remove this case if not needed.
                        IsPresented = false;
                        break;
                    case "Logout":
                        bool result = await DisplayAlert("Logout", "Are you sure you want to log out and exit?", "OK", "Cancel");

                        if (result)
                        {
                            await Detail.Navigation.PushAsync(new Login());
                            IsPresented = false;
                        }
                        break;
                    default:
                        // Handle other actions or defaults
                        IsPresented = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        private async Task<bool> CouponCodeExists(int farmerID)
        {
            string query = "SELECT COUNT(*) FROM CouponApproval WHERE FarmerID = @FarmerID";

            using (MySqlConnection connection = new MySqlConnection(DatabaseSettings.ConnectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FarmerID", farmerID);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
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

       
    }
}

 