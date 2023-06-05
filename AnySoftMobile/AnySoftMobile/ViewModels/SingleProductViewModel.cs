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
using AnySoftBackend.Library.DataTransferObjects.Review;
using AnySoftBackend.Library.DataTransferObjects.ShoppingCart;
using AnySoftBackend.Library.DataTransferObjects.User;
using AnySoftDesktop.Utils;
using AnySoftMobile.Models;
using AnySoftMobile.Services;
using AnySoftMobile.Utils;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels
{
    public class SingleProductViewModel : BaseViewModel
    {
        private ProductResponseDto? _product = new();

        public ProductResponseDto? Product
        {
            get => _product;
            set => Set(ref _product, value);
        }

        private bool _isInCart;

        public bool IsInCart
        {
            get => _isInCart;
            set => Set(ref _isInCart, value);
        }

        private bool _isBought;

        public bool IsBought
        {
            get => _isBought;
            set => Set(ref _isBought, value);
        }

        private ObservableCollection<CustomReview> _reviews;

        public ObservableCollection<CustomReview> Reviews
        {
            get => _reviews;
            set => Set(ref _reviews, value);
        }

        public ReviewCreateDto? NewReview { get; set; } = new();

        private bool _isReviewAdded;

        public bool IsReviewAdded
        {
            get => _isReviewAdded;
            set => Set(ref _isReviewAdded, value);
        }

        private bool _isOwnReviewExists;

        public bool IsOwnReviewExists
        {
            get => _isOwnReviewExists;
            set => Set(ref _isOwnReviewExists, value);
        }

        public ICommand OnAddToCartButtonClicked { get; set; }
        
        public ICommand OnAddReviewButtonClicked { get; set; }
        
        public ICommand OnEditReviewButtonClicked { get; set; }
        
        public ICommand OnRemoveReviewButtonClicked { get; set; }

        public SingleProductViewModel()
        {
            OnAddToCartButtonClicked = new Command(OnCartButtonClick);
            OnAddReviewButtonClicked = new Command(OnReviewAddButtonClick);
            OnEditReviewButtonClicked = new Command(OnReviewEditButtonClick);
            OnRemoveReviewButtonClicked = new Command(OnReviewRemoveButtonClick);
        }

        public override async void OnViewAppearing(object sender, EventArgs args)
        {
        }

        public override async void OnViewPushed(object navigationParameter = null!)
        {
            if (navigationParameter is not int productId) return;
            await UpdateProduct(productId);
        }

        private async Task UpdateProduct(int id)
        {
            try
            {
                var getProductsRequest = await WebApiService.GetCall($"api/products/{id}");
                if (getProductsRequest.IsSuccessStatusCode)
                {
                    var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                    using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                    {
                        var responseStream =
                            await getProductsRequest.Content.ReadAsStreamAsync();
                        var product = await JsonSerializer.DeserializeAsync<ProductResponseDto>(responseStream,
                            CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        if (product is null)
                            throw new InvalidOperationException("Product is null");
                        Reviews = new ObservableCollection<CustomReview>(product.Reviews
                                .Select(r => new CustomReview()
                                {
                                    Id = r.Id,
                                    Grade = r.Grade,
                                    ProductId = r.ProductId,
                                    Text = r.Text,
                                    Ts = r.Ts,
                                    IsModified = false,
                                    IsOwn = (r.User ?? new UserResponseDto()).Id ==
                                                 VersionManager.Instance.ApplicationUser?.Id,
                                    User = r.User
                                }));
                        IsOwnReviewExists = Reviews?.Any(r => r.IsOwn) ?? false;
                        Product = product;
                    }

                    var getOrdersRequest = await WebApiService.GetCall("api/orders",
                        VersionManager.Instance.ApplicationUser?.JwtToken!);
                    if (getOrdersRequest.IsSuccessStatusCode)
                    {
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                        {
                            var responseStream =
                                await getOrdersRequest.Content.ReadAsStreamAsync();
                            var orders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(
                                responseStream,
                                CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                            if (orders
                                .Where(o => o.Status == "Paid")
                                .SelectMany(o => o.PurchasedProductsIds)
                                .Any(p => p == Product.Id))
                                IsBought = true;
                        }
                    }

                    var getProductsInCartRequest =
                        await WebApiService.GetCall($"api/cart", VersionManager.Instance.ApplicationUser?.JwtToken!);
                    if (getProductsInCartRequest.IsSuccessStatusCode)
                    {
                        using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                        var responseStream =
                            await getProductsInCartRequest.Content.ReadAsStreamAsync();
                        var products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductResponseDto>>(
                            responseStream,
                            CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        if (products
                            .Any(p => p.Id == Product.Id))
                            IsInCart = true;
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

        public async void OnCartButtonClick(object idObject)
        {
            try
            {
                if (idObject is not int id)
                    return;
                if (IsBought)
                    return;
                if (IsInCart)
                    await OnRemoveFromCartButtonClick(id);
                else
                    await OnAddToCartButtonClick(id);
            }
            catch (Exception exception)
            {
                await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
            }
        }

        private async Task OnAddToCartButtonClick(int id)
        {
            var jsonObject = new
            {
                productIds = new List<int>()
                {
                    id
                }
            };
            var postProductsToCartRequest =
                await WebApiService.PostCall("api/cart", jsonObject, VersionManager.Instance.ApplicationUser.JwtToken!);
            try
            {
                if (postProductsToCartRequest.IsSuccessStatusCode)
                {
                    var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await postProductsToCartRequest.Content.ReadAsStreamAsync();
                    var cart = await JsonSerializer.DeserializeAsync<ShoppingCartResponseDto>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (cart.Products.Any(p => p.Id == Product.Id))
                    {
                        IsInCart = true;
                    }
                }
                else
                {
                    var msg = await postProductsToCartRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{postProductsToCartRequest.ReasonPhrase}\n{msg}");
                }
            }
            catch (Exception exception)
            {
                await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
            }
        }

        private async Task OnRemoveFromCartButtonClick(int id)
        {
            var removeProductsFromCartRequest =
                await WebApiService.DeleteCall($"api/cart?productId={id}",
                    VersionManager.Instance.ApplicationUser.JwtToken!);
            try
            {
                if (removeProductsFromCartRequest.IsSuccessStatusCode)
                {
                    IsInCart = false;
                }
                else
                {
                    var msg = await removeProductsFromCartRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{removeProductsFromCartRequest.ReasonPhrase}\n{msg}");
                }
            }
            catch (Exception exception)
            {
                await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
            }
        }

        public async void OnReviewAddButtonClick()
        {
            if (Reviews is not null)
                if (Reviews.Any(r => r.IsOwn))
                {
                    await MaterialDialog.Instance.AlertAsync(message: "You can only add one review per product".Trim());
                    return;
                }

            if (IsReviewAdded)
            {
                var reviewDto = new ReviewCreateDto()
                {
                    Grade = NewReview!.Grade,
                    ProductId = Product.Id,
                    Text = NewReview.Text
                };
                var postReviewRequest =
                    await WebApiService.PostCall("api/reviews", reviewDto, VersionManager.Instance.ApplicationUser.JwtToken!);
                try
                {
                    if (postReviewRequest.IsSuccessStatusCode)
                    {
                        IsReviewAdded = false;

                        if (Product.Id is not 0)
                            await UpdateProduct(Product.Id);
                        NewReview = new ReviewCreateDto();
                    }
                    else
                    {
                        var msg = await postReviewRequest.Content.ReadAsStringAsync();
                        throw new InvalidOperationException($"{postReviewRequest.ReasonPhrase}\n{msg}");
                    }
                }
                catch (Exception exception)
                {
                    await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
                }
            }
            else
                IsReviewAdded = !IsReviewAdded;
        }

        public async void OnReviewEditButtonClick(object reviewResponseDtoObject)
        {
            if (reviewResponseDtoObject is not CustomReview reviewResponseDto) return;
            if (reviewResponseDto.IsModified)
            {
                var reviewEditDto = new ReviewEditDto
                {
                    Id = reviewResponseDto.Id,
                    Grade = reviewResponseDto.Grade,
                    Text = reviewResponseDto.Text
                };
                var putReviewRequest =
                    await WebApiService.PutCall("api/reviews", reviewEditDto, VersionManager.Instance.ApplicationUser.JwtToken!);
                try
                {
                    if (putReviewRequest.IsSuccessStatusCode)
                    {
                        reviewResponseDto.IsModified = false;

                        if (Product.Id is not 0)
                            await UpdateProduct(Product.Id);
                        NewReview = new ReviewCreateDto();
                    }
                    else
                    {
                        var msg = await putReviewRequest.Content.ReadAsStringAsync();
                        throw new InvalidOperationException($"{putReviewRequest.ReasonPhrase}\n{msg}");
                    }
                }
                catch (Exception exception)
                {
                    await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
                }
            }
            else
            {
                reviewResponseDto.IsModified = true;
            }
        }

        public async void OnReviewRemoveButtonClick(object idObject)
        {
            try
            {
                if (idObject is not int id) return;
                var deleteReviewRequest =
                    await WebApiService.DeleteCall($"api/reviews/{id}", VersionManager.Instance.ApplicationUser.JwtToken!);
                if (deleteReviewRequest.IsSuccessStatusCode)
                {
                    if (Product.Id is not 0)
                        await UpdateProduct(Product.Id);
                }
                else
                {
                    var msg = await deleteReviewRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{deleteReviewRequest.ReasonPhrase}\n{msg}");
                }
            }
            catch (Exception exception)
            {
                await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
            }
        }
    }
}