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
public partial class DashboardView : BaseDashboardView
{
    public DashboardView(DashboardViewModel dashboardViewModel)
    {
        InitializeComponent();
    }

    public DashboardView()
    {
        InitializeComponent();
    }
}

public abstract class BaseDashboardView : BaseView<DashboardViewModel>
{
}