using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.Utils;
using AnySoftMobile.Models;
using AnySoftMobile.Services;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IJobDialogService _dialogService;

    private ObservableCollection<ProductResponseDto> _products = new();

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set => Set(ref _products, value);
    }
    
    public string? SearchString { get; set; }
    
    public ICommand OnSearchBarTextEnteredCommand { get; set; }
    public ICommand OnProductViewEnteredCommand { get; set; }
    
    public ICommand OnLoginViewEnteredCommand { get; set; }
    
    public ICommand OnProfileViewEnteredCommand { get; set; }

    public DashboardViewModel(IJobDialogService dialogService)
    {
        _dialogService = dialogService;
        OnSearchBarTextEnteredCommand = new Command(OpenSearchPage);
        OnProductViewEnteredCommand = new Command(OpenSingleProductPage);
        OnLoginViewEnteredCommand = new Command(OpenLoginPage);
        OnProfileViewEnteredCommand = new Command(OpenProfilePage);
    }

    public override async void OnViewAppearing(object sender, EventArgs args)
    {
        await UpdateProducts();
    }

    private async void OpenSingleProductPage(object id)
    {
        await Navigation.PushAsync(ViewNames.SingleProductView, id);
    }
    
    private async void OpenLoginPage()
    {
        await Navigation.PushAsync(ViewNames.LoginView);
    }
    
    private async void OpenProfilePage()
    {
        await Navigation.PushAsync(ViewNames.ProfileView);
    }

    private async void OpenSearchPage(object o)
    {
        try
        {
            var productRequestDto = new ProductRequestDto
            {
                Name = string.IsNullOrEmpty(SearchString)
                    ? null
                    : SearchString,
                /*Genres = SelectedGenre is {Id: > 0}
                    ? new List<int>() {SelectedGenre.Id}
                    : null,
                Properties = SelectedProperty is {Id: > 0}
                    ? new List<int>() {SelectedProperty.Id}
                    : null,*/
                //TODO
            };
            var productRequestQueryJson =
                HttpUtility.UrlEncode(JsonSerializer.Serialize(productRequestDto, CustomJsonSerializerOptions.Options));
            var getProductsRequest = await WebApiService.GetCall($"api/products?Query={productRequestQueryJson}");
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getProductsRequest.Content.ReadAsStreamAsync();
                var productsFiltered = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                if (productsFiltered is not null)
                    await Navigation.PushAsync(ViewNames.MultipleProductsView, new SearchModel()
                    {
                        SearchString = SearchString ?? "",
                        Products = productsFiltered
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
                            })
                    });

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

    private async Task UpdateProducts()
    {
        try
        {
            var getProductsRequest = await WebApiService.GetCall("api/products");
            if (getProductsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream =
                        await getProductsRequest.Content.ReadAsStreamAsync();
                    var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(
                        responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
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
}