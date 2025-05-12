using MCOCSrv.Resources.Classes;

namespace MCOCSrv.Resources.Popups;

public partial class LoadingPopup : ContentView
{
    public LoadingPopup()
    {
        InitializeComponent();
        InitializeLogger();
    }

    private void InitializeLogger()
    {
        UILogger.LogCallback = async msg =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PopupFetchOutput.Text += msg + Environment.NewLine;
                PopupFetchOutputField.ScrollToAsync(0, PopupFetchOutputField.ContentSize.Height, false);
            });
        };
    }
}