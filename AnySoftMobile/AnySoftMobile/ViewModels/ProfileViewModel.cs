using System.Collections.ObjectModel;
using AnySoftBackend.Library.DataTransferObjects.Order;
using AnySoftDesktop.Models;

namespace AnySoftMobile.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private ApplicationUser _applicationUser;
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
}