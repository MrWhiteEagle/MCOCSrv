using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Pages.SingletonPages;
//[QueryProperty(nameof(InstanceName), "InstanceName")]
public partial class InstanceSettingsPage : ContentPage
{
    private readonly InstanceManager manager;
    private InstanceModel instance;
    public string InstanceName { get; set; }
    public List<Setting> SettingsList { get; set; }
    public InstanceSettingsPage(InstanceModel instance)
    {
        InitializeComponent();
        this.manager = App.Current.Handler.GetService<InstanceManager>();
        this.instance = instance;
        this.InstanceName = instance.Name;
        SettingsList = manager.GetInstanceSettings(instance);
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SettingsList = manager.GetInstanceSettings(instance);
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Shell.Current.Navigation.PopAsync();
    }

    private void SaveClicked(object? sender, EventArgs e)
    {

    }
}