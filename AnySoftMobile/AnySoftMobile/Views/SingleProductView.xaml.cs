using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnySoftMobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SingleProductView : BaseSingleProductView
{
    public SingleProductView(SingleProductViewModel singleProductViewModel)
    {
        InitializeComponent();
    }
}

public abstract class BaseSingleProductView : BaseView<SingleProductViewModel>
{
}