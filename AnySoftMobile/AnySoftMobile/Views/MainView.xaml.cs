using AnySoftMobile.ViewModels;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : BaseMainView
    {
        public MainView()
        {
            InitializeComponent();
        }
    }

    public abstract class BaseMainView : BaseView<MainViewModel> { }
}