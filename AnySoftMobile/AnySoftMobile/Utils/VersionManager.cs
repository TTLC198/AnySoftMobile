using AnySoftDesktop.Models;
using Xamarin.Forms;

namespace AnySoftMobile.Utils;

public class VersionManager : BindableObject
{
    private static readonly BindableProperty ApplicationUserProperty =
        BindableProperty.Create( "ApplicationUser", 
            typeof(ApplicationUser),
            typeof(VersionManager), 
            new ApplicationUser());

    public ApplicationUser ApplicationUser {
        get => (ApplicationUser)GetValue(ApplicationUserProperty);
        set => SetValue(ApplicationUserProperty, value );
    }
    
    private static readonly BindableProperty IsAuthorizedProperty =
        BindableProperty.Create( "IsAuthorized", 
            typeof(bool),
            typeof(VersionManager), 
            false);

    public bool IsAuthorized {
        get => (bool)GetValue(IsAuthorizedProperty);
        set => SetValue(IsAuthorizedProperty, value );
    }
    
    private static readonly BindableProperty ApiUrlProperty =
        BindableProperty.Create( 
            nameof(ApiUrl), 
            typeof(string),
            typeof(VersionManager));

    public string? ApiUrl
    {
        get => (string)GetValue(ApiUrlProperty);
        set => SetValue(ApiUrlProperty, value);
    }
    
    private static readonly BindableProperty CdnUrlProperty =
        BindableProperty.Create( 
            nameof(CdnUrl), 
            typeof(string),
            typeof(VersionManager));
    
    public string? CdnUrl
    {
        get => (string)GetValue(CdnUrlProperty);
        set => SetValue(CdnUrlProperty, value );
    }

    public static VersionManager Instance { get; private set; }

    static VersionManager() {
        Instance = new VersionManager();
    }
}