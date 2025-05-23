using MCOCSrv.Resources.Models;
using MCOCSrv.Resources.Pages.SingletonPages;

namespace MCOCSrv.Resources.Content;

public partial class SettingButton : ContentView
{
    private InstanceModel BoundInstance;
    public SettingButton()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnSettingsRequest(object sender, EventArgs a)
    {
        await Shell.Current.Navigation.PushAsync(new InstanceSettingsPage(BoundInstance));
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (BindingContext is InstanceModel instance)
        {
            BoundInstance = instance;
        }
        else
        {
            BoundInstance = null;
        }
    }
}