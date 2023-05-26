using System;
using AnySoftDesktop.Models;
using AnySoftMobile.Utils;
using AnySoftMobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace AnySoftMobile
{
    public partial class App : Application
    {
        public App(INavigationService navigationService)
        {
            XF.Material.Forms.Material.Init(this);
            InitializeComponent();
            XF.Material.Forms.Material.Use("Material.Style");

            navigationService.SetRootView(ViewNames.DashboardView);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
    
    public partial class App
    {
        public static string CdnUrl { get; } = "https://157e-91-245-37-31.ngrok-free.app/";
        
        public static string ApiUrl { get; } = "https://157e-91-245-37-31.ngrok-free.app/";

        public static ApplicationUser ApplicationUser { get; set; } = new ApplicationUser();
    }
}