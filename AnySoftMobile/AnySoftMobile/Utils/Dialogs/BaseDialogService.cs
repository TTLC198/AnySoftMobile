using System.Threading.Tasks;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.Utils.Dialogs
{
    public abstract class BaseDialogService
    {
        public static IMaterialDialog DialogFacade => MaterialDialog.Instance;

        protected static Task Alert(string message)
        {
            return DialogFacade.AlertAsync(message);
        }
    }
}