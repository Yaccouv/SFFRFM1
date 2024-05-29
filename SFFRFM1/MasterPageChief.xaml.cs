using SFFRFM1.Model;
using SFFRFM1.ViewModel;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SFFRFM1
{
    public partial class MasterPageChief : MasterDetailPage
    {

        List<MenuItemChief> menu;

        public MasterPageChief()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            menu = new List<MenuItemChief>();

            menu.Add(new MenuItemChief { OptionName = "Home", IconSource = ImageSource.FromFile("home1.jpeg") });
            menu.Add(new MenuItemChief { OptionName = "Add Farmer", IconSource = ImageSource.FromFile("inc3.jpeg") });
            menu.Add(new MenuItemChief { OptionName = "Help", IconSource = ImageSource.FromFile("report2.png") });
            menu.Add(new MenuItemChief { OptionName = "Logout", IconSource = ImageSource.FromFile("report2.png") });


            navigationList.ItemsSource = menu;
            Detail = new NavigationPage(new DashboardChief());
        }



        private async void Item_Tapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = e.Item as MenuItemChief;

                foreach (var menuItemChief in menu)
                {
                    menuItemChief.IsActive = (menuItemChief.OptionName == item.OptionName);
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
                        Detail = new NavigationPage(new DashboardChief());
                        IsPresented = false;
                        break;
                    case "Add Farmer":
                        Detail = new NavigationPage(new AddFarmer());
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
    }
}
