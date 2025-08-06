using System.Diagnostics;

namespace MCOCSrv.Resources.Classes
{
    public static class UILogger
    {
        public static event EventHandler<string>? LogReceived;

        public static void LogUI(string msg)
        {
            Debug.WriteLine(msg);
            LogReceived?.Invoke(null, msg);
        }
    }
}
