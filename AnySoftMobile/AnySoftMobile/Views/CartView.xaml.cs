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
public partial class CartView : BaseCartView
{
    public CartView(CartViewModel cartViewModel)
    {
        InitializeComponent();
    }

    public CartView()
    {
        InitializeComponent();
    }
}

public abstract class BaseCartView : BaseView<CartViewModel>
{
}