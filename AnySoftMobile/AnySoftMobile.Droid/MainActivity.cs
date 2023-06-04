using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AnySoftMobile.Droid.Core;
using XF.Material.Droid;

namespace AnySoftMobile.Droid
{
    [Activity(Label = "AnySoftMobile", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Material.Init(this, savedInstanceState);

            var appContainer = new PlatformContainer();
            appContainer.Setup();

            var app = CommonServiceLocator.ServiceLocator.Current.GetInstance<App>();

            LoadApplication(app);
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        public override void OnBackPressed()
        {
            Material.HandleBackButton(base.OnBackPressed);
        }
    }
}