using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.Diagnostics;

namespace MCOCSrv.Resources.Pages.SingletonPages;
//[QueryProperty(nameof(InstanceName), "InstanceName")]
public partial class InstanceSettingsPage : ContentPage
{
    private readonly InstanceManager manager;
    private InstanceModel? instance;
    public string InstanceName { get; set; }
    public Dictionary<string, string> SettingsList { get; set; } = new();
    public InstanceSettingsPage(InstanceModel instance)
    {
        InitializeComponent();
        this.manager = App.Current.Handler.GetService<InstanceManager>();
        this.instance = instance;
        this.InstanceName = instance.Name;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine($"Got Instance {InstanceName}");
    }
}