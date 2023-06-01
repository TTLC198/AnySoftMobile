using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnySoftBackend.Library.DataTransferObjects.Product;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.UsersControls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProductsCarousel : CarouselView
{
    public static readonly BindableProperty ProductsProperty =
        BindableProperty.Create( nameof(Products), 
            typeof(ObservableCollection<ProductResponseDto>),
            typeof(ProductsCarousel), 
            new ObservableCollection<ProductResponseDto>());

    public ObservableCollection<ProductResponseDto> Products {
        get => (ObservableCollection<ProductResponseDto>)GetValue(ProductsProperty);
        set => SetValue(ProductsProperty, value );
    }
    
    public ProductsCarousel()
    {
        InitializeComponent();
    }
}