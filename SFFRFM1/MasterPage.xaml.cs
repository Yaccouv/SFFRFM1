using SFFRFM1.Model;
using SFFRFM1.ViewModel;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SFFRFM1
{
    public partial class MasterPage : MasterDetailPage
    {

        private Farmer _farmer;
        string userEmail;
        List<MenuItem> menu;

        public MasterPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            menu = new List<MenuItem>();

            menu.Add(new MenuItem { OptionName = "Home", IconSource = ImageSource.FromFile("home1.jpeg") });
            menu.Add(new MenuItem { OptionName = "Add Chief", IconSource = ImageSource.FromFile("home1.jpeg") });
            menu.Add(new MenuItem { OptionName = "Distribute Coupons", IconSource = ImageSource.FromFile("inc3.jpeg") });
            menu.Add(new MenuItem { OptionName = "Approve Coupons", IconSource = ImageSource.FromFile("inc3.jpeg") });
            menu.Add(new MenuItem { OptionName = "Scan", IconSource = ImageSource.FromFile("hat1.jpeg") });
            menu.Add(new MenuItem { OptionName = "Reports", IconSource = ImageSource.FromFile("report2.png") });
            menu.Add(new MenuItem { OptionName = "Help", IconSource = ImageSource.FromFile("report2.png") });
            menu.Add(new MenuItem { OptionName = "Logout", IconSource = ImageSource.FromFile("report2.png") });


            navigationList.ItemsSource = menu;
            Detail = new NavigationPage(new Dashboard());
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
                var item = e.Item as MenuItem;

                foreach (var menuItem in menu)
                {
                    menuItem.IsActive = (menuItem.OptionName == item.OptionName);
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
                        Detail = new NavigationPage(new Dashboard());
                        IsPresented = false;
                        break;
                    case "Add Chief":
                        Detail = new NavigationPage(new RegisterChief());
                        IsPresented = false;
                        break;
                  
                    case "Distribute Coupons":
                        // Handle the "Reports" action here or remove this case if not needed.
                        Detail = new NavigationPage(new CouponDistribution());
                        IsPresented = false;
                        break;
                    case "Approve Coupons":
                        Detail = new NavigationPage(new CouponApproval());
                        IsPresented = false;
                        break;
                    case "Scan":
                        // Handle the "Reports" action here or remove this case if not needed.
                        Detail = new NavigationPage(new QRScanner(userEmail));
                        IsPresented = false;
                        break;
                    case "Reports":
                        // Handle the "Reports" action here or remove this case if not needed.
                        Detail = new NavigationPage(new CouponChart());
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
    }
}
