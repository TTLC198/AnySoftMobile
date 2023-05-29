using System.Collections.Generic;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftMobile.ViewModels;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MultipleProductsView : BaseMultipleProductsView
{
    public MultipleProductsView(MultipleProductsViewModel multipleProductsViewModel)
    {
        InitializeComponent();
    }
}

public abstract class BaseMultipleProductsView : BaseView<MultipleProductsViewModel>
{
}