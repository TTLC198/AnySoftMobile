using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftDesktop.Utils;
using AnySoftMobile.Services;
using AnySoftMobile.Utils.Dialogs;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IJobDialogService _dialogService;

    private ObservableCollection<ProductResponseDto> _products;

    public ObservableCollection<ProductResponseDto> Products
    {
        get => _products;
        set => Set(ref _products, value);
    }

    public DashboardViewModel(IJobDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public override async void OnViewAppearing(object sender, EventArgs args)
    {
        await UpdateProducts();
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
                            Description = p.Description?.Substring(0, 65) + "...",
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