using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.Order;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.Utils;
using AnySoftMobile.Services;
using AnySoftMobile.Utils;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class LibraryViewModel : BaseViewModel
{
    private readonly IJobDialogService _dialogService;
    
    private ObservableCollection<ProductResponseDto> _products = new ();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set => Set(ref _products, value);
    }

    public ICommand OnProductViewEntered { get; set; }

    public LibraryViewModel(IJobDialogService dialogService)
    {
        _dialogService = dialogService;
        OnProductViewEntered = new Command(OpenSingleProductPage);
    }
    
    private async void OpenSingleProductPage(object id)
    {
        await Navigation.PushAsync(ViewNames.SingleProductView, id);
    }
    
    public override async void OnViewPushed(object navigationParameter = null!)
    {
        await UpdateOrders();
    }

    public override async void OnViewAppearing(object sender, EventArgs args)
    {
        await UpdateOrders();
    }
    
    private async Task UpdateOrders()
    {
        if (!VersionManager.Instance.IsAuthorized)
        {
            await Navigation.PushAsync(ViewNames.LoginView);
            return;
        }
        try
        {
            var getOrdersRequest = await WebApiService.GetCall("api/orders",  VersionManager.Instance.ApplicationUser.JwtToken!);
            if (getOrdersRequest.IsSuccessStatusCode)
            {
                var orders = new List<OrderResponseDto>();
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync();
                    var receivedOrders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    orders = receivedOrders.ToList();
                }
                var products = new List<ProductResponseDto>();
                foreach (var productId in orders
                             .Where(o => o.Status == "Paid")
                             .SelectMany(o => o.PurchasedProductsIds))
                {
                    var getProductRequest = await WebApiService.GetCall($"api/products/{productId}", VersionManager.Instance.ApplicationUser.JwtToken!);
                    if (getProductRequest.IsSuccessStatusCode)
                    {
                        using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                        var responseStream =
                            await getProductRequest.Content.ReadAsStreamAsync();
                        var product = await JsonSerializer.DeserializeAsync<ProductResponseDto>(
                            responseStream,
                            CustomJsonSerializerOptions.Options,
                            cancellationToken: cancellationTokenSource.Token);
                        if (product is null)
                            continue;
                        products.Add(
                            new ProductResponseDto()
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Cost = product.Cost,
                                Discount = product.Discount,
                                Rating = product.Rating,
                                Seller = product.Seller,
                                Description = product.Description?.Substring(0, 45) + "...",
                                Images = product.Images,
                                Genres = product.Genres,
                                Properties = product.Properties
                            });
                    }
                }
                Products = new ObservableCollection<ProductResponseDto>(
                    products
                );
            }
            else
            {
                var msg = await getOrdersRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getOrdersRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
}