using System.Diagnostics;

namespace MCOCSrv.Resources.Classes
{
    public static class UILogger
    {
        public static Action<string>? LogCallback;

        public static void LogUI(string msg)
        {
            Debug.WriteLine(msg);
            LogCallback.Invoke(msg);
        }
    }
}
