using AnySoftMobile.Utils.Dialogs;

namespace AnySoftMobile.ViewModels;

public class CartViewModel : MultipleProductsViewModel
{
    public CartViewModel(IJobDialogService dialogService) : base(dialogService)
    {
    }
}