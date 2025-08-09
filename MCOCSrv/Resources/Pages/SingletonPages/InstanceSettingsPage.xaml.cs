using CommunityToolkit.Maui.Storage;
using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.Diagnostics;

namespace MCOCSrv.Resources.Pages.SingletonPages;
//[QueryProperty(nameof(InstanceName), "InstanceName")]
public partial class InstanceSettingsPage : ContentPage
{
    private readonly InstanceManager manager;
    private InstanceModel instance;
    public string InstanceName { get; set; }
    public List<Setting> SettingsList { get; set; }
    public string MaxHeap { get; set; }
    public string MinHeap { get; set; }
    public string Arguments { get; set; }
    public InstanceSettingsPage(InstanceModel instance)
    {
        InitializeComponent();
        this.manager = App.Current?.Handler.GetService<InstanceManager>();
        this.instance = instance;
        this.InstanceName = instance.Name;
        MaxHeap = instance.MaxHeap;
        MinHeap = instance.MinHeap;
        Arguments = instance.LaunchArguments;
        SettingsList = instance.Settings;
        if (manager.running.Contains(instance))
        {
            MoveButton.IsEnabled = false;
        }
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        Shell.Current.Navigation.PopAsync();
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        instance.MaxHeap = MaxHeap;
        instance.MinHeap = MinHeap;
        await manager.SaveInstanceSettings(SettingsList, instance, ConvertToArguments(Arguments));
        await Shell.Current.Navigation.PopAsync();
    }

    private async void MoveButtonClicked(object? sender, EventArgs e)
    {
        var NewPath = await FolderPicker.PickAsync(instance.GetPath());
        if (NewPath.Folder != null)
        {
            manager.MoveInstance(instance, NewPath.Folder.Path);
        }
    }

    private string ConvertToArguments(string data)
    {
        data.Trim();
        var list = data.Split(" ");
        foreach (var argument in list)
        {
            argument.Trim();
        }
        Debug.WriteLine(string.Join(" ", list));
        return string.Join(" ", list);
    }
}