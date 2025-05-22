using MCOCSrv.Resources.Classes;

namespace MCOCSrv.Resources.Popups;

public partial class LoadingPopup : ContentView
{
    public LoadingPopup()
    {
        InitializeComponent();
    }

    private async void OnLog(object sender, string msg)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            PopupFetchOutput.Text += msg + Environment.NewLine;
            Task.Delay(1);
            PopupFetchOutputField.ScrollToAsync(0, PopupFetchOutputField.ContentSize.Height, false);
        });
    }

    private void Loaded(object sender, EventArgs a)
    {
        UILogger.LogReceived += OnLog;
    }

    private void Unloaded(object sender, EventArgs a)
    {
        UILogger.LogReceived -= OnLog;
    }
}