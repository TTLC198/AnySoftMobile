using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftMobile.Models;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;

namespace AnySoftMobile.ViewModels;

public class MultipleProductsViewModel : BaseViewModel
{
    private readonly IJobDialogService _dialogService;
    
    private ObservableCollection<ProductResponseDto> _products = new ();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set => Set(ref _products, value);
    }

    private string? _searchString;
    
    public string? SearchString { 
        get => _searchString;
        set => Set(ref _searchString, value); 
    }
    
    public ICommand OnProductViewEntered { get; set; }

    public MultipleProductsViewModel(IJobDialogService dialogService)
    {
        _dialogService = dialogService;
        OnProductViewEntered = new Command(OpenSingleProductPage);
    }

    private async void OpenSingleProductPage(object id)
    {
        await Navigation.PushAsync(ViewNames.SingleProductView, id);
    }
    
    public override void OnViewPushed(object navigationParameter = null!)
    {
        if (navigationParameter is not SearchModel searchModel) return;
        SearchString = searchModel.SearchString;
        Products = new ObservableCollection<ProductResponseDto>(searchModel.Products);
    }
}