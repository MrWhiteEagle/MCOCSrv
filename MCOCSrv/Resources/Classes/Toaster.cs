using CommunityToolkit.Maui.Alerts;

namespace MCOCSrv.Resources.Classes
{
    public static class Toaster
    {
        public static void Toastify(string message)
        {
            Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }

        public static void ToastifyLong(string message)
        {
            Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
    }
}
