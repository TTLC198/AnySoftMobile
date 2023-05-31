using System;
using AnySoftDesktop.Models;
using AnySoftMobile.Views;

namespace AnySoftMobile.ViewModels;

public class MainViewModel : BaseViewModel
{
    public bool IsAuthorized { get; set; }
    
    private ApplicationUser _currentUser = new ApplicationUser();

    public ApplicationUser CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }
    
    public override void OnViewAppearing(object sender, EventArgs args)
    {
    }
}