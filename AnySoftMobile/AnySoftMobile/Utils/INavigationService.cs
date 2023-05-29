using System.Threading.Tasks;
using AnySoftMobile.ViewModels;
using AnySoftMobile.Views;
using Xamarin.Forms;

namespace AnySoftMobile.Utils
{
    public interface INavigationService
    {
        void SetRootView(string rootViewName, object parameter = null!);

        Task PushAsync(string viewName, object parameter = null!);

        Task PopAsync();

        Task PushModalAsync(string viewName, object parameter = null!);

        Task PopModalAsync();
    }
}