using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.Order;
using AnySoftBackend.Library.DataTransferObjects.Product;
using AnySoftBackend.Library.DataTransferObjects.User;
using AnySoftDesktop.Models;
using AnySoftDesktop.Utils;
using AnySoftMobile.Models;
using AnySoftMobile.Services;
using AnySoftMobile.Utils;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private ApplicationUser _applicationUser = VersionManager.Instance.ApplicationUser;
    public ApplicationUser ApplicationUser
    {
        get => _applicationUser;
        set
        {
            _applicationUser = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<CustomPayment> _paymentMethods = new ObservableCollection<CustomPayment>();

    
    public ObservableCollection<CustomPayment> PaymentMethods
    {
        get => _paymentMethods;
        set
        {
            _paymentMethods = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<OrderResponseDto> _orders = new ObservableCollection<OrderResponseDto>();

    
    public ObservableCollection<OrderResponseDto> Orders
    {
        get => _orders;
        set
        {
            _orders = value;
            OnPropertyChanged();
        }
    }
    
    private string? _userImagePath;

    public string? UserImagePath
    {
        get => _userImagePath;
        set
        {
            _userImagePath = value;
            OnPropertyChanged();
        }
    }

    public ICommand OnProfileChangedCommand { get; set; }
    
    public ICommand OnLogoutClickedCommand { get; set; }

    public ProfileViewModel()
    {
        OnProfileChangedCommand = new Command(OnProfileChanged);
        OnLogoutClickedCommand = new Command(OnLogoutClicked);
    }
    
    public override async void OnViewPushed(object navigationParameter = null!)
    {
        await UpdatePayments();
        await UpdateOrders();
    }

    public async Task UpdatePayments()
    {
        try
        {
            var getPaymentsRequest = await WebApiService.GetCall("api/payment", VersionManager.Instance.ApplicationUser.JwtToken!);
            if (getPaymentsRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getPaymentsRequest.Content.ReadAsStreamAsync();
                var payments = await JsonSerializer.DeserializeAsync<IEnumerable<Payment>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                PaymentMethods = new ObservableCollection<CustomPayment>(payments
                    .ToList()
                    .Select(p => new CustomPayment()
                    {
                        Id = p.Id,
                        CardName = p.CardName,
                        Number = p.Number,
                        Cvc = p.Cvc,
                        ExpirationDate =
                            new CustomDate() {Month = p.ExpirationDate.Month, Year = p.ExpirationDate.Year},
                        UserId = p.UserId,
                        IsActive = p.IsActive
                    }));
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
    
    public async Task UpdateOrders()
    {
        try
        {
            var getOrdersRequest = await WebApiService.GetCall("api/orders", VersionManager.Instance.ApplicationUser.JwtToken!);
            if (getOrdersRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getOrdersRequest.Content.ReadAsStreamAsync();
                var orders = await JsonSerializer.DeserializeAsync<IEnumerable<OrderResponseDto>>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                Orders = new ObservableCollection<OrderResponseDto>(orders.ToList());
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
    
    private async void OnProfileChanged()
    {
        try
        {
            var userEdit = new UserEditDto()
            {
                Id = ApplicationUser.Id ?? 0,
                Login = ApplicationUser.Login,
                Email = ApplicationUser.Email,
                Password = ApplicationUser.Password
            };
            var putChangesOfUserRequest =
                await WebApiService.PutCall("api/users", userEdit, VersionManager.Instance.ApplicationUser.JwtToken!);
            if (putChangesOfUserRequest.IsSuccessStatusCode)
            {
                var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream =
                    await putChangesOfUserRequest.Content.ReadAsStreamAsync();
                var editedUser = await JsonSerializer.DeserializeAsync<User>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                if (editedUser is null)
                    throw new InvalidOperationException("User is null");
            }
            else
            {
                var msg = await putChangesOfUserRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{putChangesOfUserRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
    
    public async void OnLogoutClicked()
    {
        VersionManager.Instance.IsAuthorized = false;
        VersionManager.Instance.ApplicationUser = new ApplicationUser();
        await Navigation.PopAsync();
    }
}