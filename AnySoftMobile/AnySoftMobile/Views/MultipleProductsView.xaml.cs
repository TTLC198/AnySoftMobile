using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnySoftMobile.ViewModels;
using AnySoftMobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MultipleProductsView : BaseMultipleProductsView
{
    public MultipleProductsView(DashboardViewModel dashboardViewModel)
    {
        InitializeComponent();
    }
}

public abstract class BaseMultipleProductsView : BaseView<MultipleProductsViewModel>
{
}