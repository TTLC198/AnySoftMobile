using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnySoftMobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnySoftMobile.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProfileView : BaseProfileView
{
    public ProfileView(ProfileViewModel profileViewModel)
    {
        InitializeComponent();
    }
}

public abstract class BaseProfileView : BaseView<ProfileViewModel>
{
}