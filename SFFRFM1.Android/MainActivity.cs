using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using ZXing.Mobile;
using Xamarin.Forms.Platform.Android;
using Xamarin.Essentials;

namespace SFFRFM1.Droid
{
    [Activity(Label = "SFFRFM1", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            MobileBarcodeScanner.Initialize(Application);
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            LoadApplication(new App());
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            global::ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
