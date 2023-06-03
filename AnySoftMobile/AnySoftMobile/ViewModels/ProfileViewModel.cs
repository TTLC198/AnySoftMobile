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
using AnySoftBackend.Library.DataTransferObjects.Payment;
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
    
    public ICommand OnAddPaymentClickedCommand { get; set; }
    
    public ICommand OnSavePaymentClickedCommand { get; set; }
    
    public ICommand OnRemovePaymentClickedCommand { get; set; }
    
    public ICommand OnEditPaymentClickedCommand { get; set; }

    public ProfileViewModel()
    {
        OnAddPaymentClickedCommand = new Command(AddPayment);
        OnSavePaymentClickedCommand = new Command(SavePayment);
        OnRemovePaymentClickedCommand = new Command(RemovePayment);
        OnEditPaymentClickedCommand = new Command(EditPayment);
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

    private async void AddPayment()
    {
        PaymentMethods.Add(new CustomPayment() { CardName = "", Number = "", Cvc = "", IsEditComplete = true });
    }

    private async void SavePayment(object paymentObject)
    {
        try
        {
            if (paymentObject is not CustomPayment payment) return;
            var paymentDto = new PaymentCreateDto()
            {
                CardName = payment.CardName,
                Number = payment.Number,
                ExpirationDate = new DateTime(payment.ExpirationDate!.Year, payment.ExpirationDate.Month, 1),
                Cvc = payment.Cvc
            };
            var postPaymentMethodRequest =
                await WebApiService.PostCall("api/payment", paymentDto, VersionManager.Instance.ApplicationUser.JwtToken!);
            if (postPaymentMethodRequest.IsSuccessStatusCode)
            {
                await UpdatePayments();
            }
            else
            {
                var msg = await postPaymentMethodRequest.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{postPaymentMethodRequest.ReasonPhrase}\n{msg}");
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }

    private async void EditPayment(object paymentObject)
    {
        if (paymentObject is not CustomPayment payment) return;
        if (payment.IsEditComplete)
        {
            try
            {
                var paymentEditDto = new PaymentEditDto()
                {
                    Id = payment.Id.Value,
                    Number = payment.Number,
                    CardName = payment.CardName,
                    ExpirationDate = new DateTime(payment.ExpirationDate!.Year, payment.ExpirationDate.Month, 1),
                    Cvc = payment.Cvc
                };
                var putPaymentRequest =
                    await WebApiService.PutCall($"api/payment", paymentEditDto, VersionManager.Instance.ApplicationUser.JwtToken!);
                if (putPaymentRequest.IsSuccessStatusCode)
                {
                    var timeoutAfter = TimeSpan.FromMilliseconds(3000);
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream =
                        await putPaymentRequest.Content.ReadAsStreamAsync();
                    var editedPayment = await JsonSerializer.DeserializeAsync<Payment>(
                        responseStream,
                        CustomJsonSerializerOptions.Options,
                        cancellationToken: cancellationTokenSource.Token);
                    var existedPayment = PaymentMethods.FirstOrDefault(p => p.Id == payment.Id);
                    if (editedPayment is null)
                        throw new InvalidOperationException("Existed payment does not exist");
                    if (existedPayment is null)
                        throw new InvalidOperationException("Existed payment does not exist");
                    existedPayment.IsEditComplete = false;
                    existedPayment.Number = editedPayment.Number;
                    existedPayment.CardName = editedPayment.CardName;
                    existedPayment.ExpirationDate = new CustomDate()
                        {Month = editedPayment.ExpirationDate.Month, Year = editedPayment.ExpirationDate.Year};
                    existedPayment.Cvc = editedPayment.Cvc;
                }
                else
                {
                    var msg = await putPaymentRequest.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"{putPaymentRequest.ReasonPhrase}\n{msg}");
                }
            }
            catch (Exception exception)
            {
                await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
            }
        }
        else
        {
            var existedPayment = PaymentMethods.FirstOrDefault(pm => pm.Id == payment.Id);
            if (existedPayment is not null)
                existedPayment.IsEditComplete = true;
        }
    }

    private async void RemovePayment(object idObject)
    {
        try
        {
            if (idObject is not int id) return;
            if (id == 0)
            {
                await UpdatePayments();
                return;
            }

            var postProductsToCartRequest =
                await WebApiService.DeleteCall($"api/payment/{id}", VersionManager.Instance.ApplicationUser.JwtToken!);
            if (postProductsToCartRequest.IsSuccessStatusCode)
            {
                var paymentToDelete = PaymentMethods.FirstOrDefault(p => p.Id == id);
                if (paymentToDelete is null)
                    throw new InvalidOperationException("Payment not found");
                PaymentMethods.Remove(paymentToDelete);
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
    
    public async void OnLogoutClicked()
    {
        VersionManager.Instance.IsAuthorized = false;
        VersionManager.Instance.ApplicationUser = new ApplicationUser();
        await Navigation.PopAsync();
    }
}