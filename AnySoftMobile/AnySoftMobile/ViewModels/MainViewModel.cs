using System;
using AnySoftDesktop.Models;
using AnySoftMobile.Utils;
using AnySoftMobile.Views;
using XF.Material.Forms.UI.Dialogs;

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
    
    public override async void OnViewAppearing(object sender, EventArgs args)
    {
        
    }
}