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
            Xamarin.Forms.DataGrid.DataGridComponent.Init(); 

            navigationService.SetRootView(ViewNames.MainView);
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
}