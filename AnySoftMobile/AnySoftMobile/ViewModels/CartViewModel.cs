using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.Order;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.Utils;
using AnySoftMobile.Models;
using AnySoftMobile.Services;
using AnySoftMobile.Utils;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class CartViewModel : BaseViewModel
{
    private readonly IJobDialogService _dialogService;

    private ObservableCollection<ProductResponseDto> _products = new();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set => Set(ref _products, value);
    }

    public ICommand OnProductViewEntered { get; set; }
    
    public ICommand OnCheckoutButtonClicked { get; set; }

    public CartViewModel(IJobDialogService dialogService)
    {
        _dialogService = dialogService;
        OnProductViewEntered = new Command(OpenSingleProductPage);
        OnCheckoutButtonClicked = new Command(OnOrderRequest);
    }

    private async void OpenSingleProductPage(object id)
    {
        await Navigation.PushAsync(ViewNames.SingleProductView, id);
    }

    public override async void OnViewPushed(object navigationParameter = null!)
    {
        await UpdateProducts();
    }

    public override async void OnViewAppearing(object sender, EventArgs args)
    {
        await UpdateProducts();
    }

    private async Task UpdateProducts()
    {
        if (!VersionManager.Instance.IsAuthorized)
        {
            await Navigation.PushAsync(ViewNames.LoginView);
            return;
        }
        try
        {
            var getProductsRequest =
                await WebApiService.GetCall($"api/cart", VersionManager.Instance.ApplicationUser.JwtToken!);
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync();
                var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                if (products is not null)
                    Products = new ObservableCollection<ProductResponseDto>(products
                        .Select(p => new ProductResponseDto()
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Cost = p.Cost,
                            Discount = p.Discount,
                            Rating = p.Rating,
                            Seller = p.Seller,
                            Description = p.Description?.Substring(0, 45) + "...",
                            Images = p.Images,
                            Genres = p.Genres,
                            Properties = p.Properties
                        }));
            }
            else
            {
                var msg = await getProductsRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{getProductsRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }

    private async void OnOrderRequest()
    {
        try
        {
            var cartOrderRequest =
                await WebApiService.PostCall("api/cart/order", null!,
                    VersionManager.Instance.ApplicationUser.JwtToken!);
            if (cartOrderRequest.IsSuccessStatusCode)
            {
                var createdOrder = new OrderResponseDto();
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream =
                        await cartOrderRequest.Content.ReadAsStreamAsync();
                    createdOrder = await JsonSerializer.DeserializeAsync<OrderResponseDto>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (createdOrder is null)
                        throw new InvalidOperationException("Order is null");
                }

                var getPaymentsRequest =
                    await WebApiService.GetCall("api/payment", VersionManager.Instance.ApplicationUser.JwtToken!);
                if (getPaymentsRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await getPaymentsRequest.Content.ReadAsStreamAsync();
                    var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (payments is null)
                        throw new InvalidOperationException("You have not added a means of payment");

                    var selectedPaymentMethod = await MaterialDialog.Instance.SelectChoiceAsync(
                        title: "Select a payment method",
                        choices: payments
                            .Select(p => p.Number)
                            .ToArray());
                    if (selectedPaymentMethod < 0)
                        throw new InvalidOperationException("Payment method not selected");

                    var orderPurchase = new OrderPurchaseDto()
                    {
                        OrderId = createdOrder.Id,
                        PaymentId = payments.ToArray()[selectedPaymentMethod].Id ?? -1
                    };

                    var orderConfirmationRequest = await WebApiService.PostCall("api/orders/buy", orderPurchase,
                        VersionManager.Instance.ApplicationUser.JwtToken!);
                    if (!orderConfirmationRequest.IsSuccessStatusCode) return;

                    await UpdateProducts();
                }
                else
                {
                    var msg = await getPaymentsRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{getPaymentsRequest.ReasonPhrase}\n{msg}");
                }
            }
            else
            {
                var msg = await cartOrderRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{cartOrderRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
}