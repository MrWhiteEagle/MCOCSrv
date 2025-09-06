using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class CreateInstanceRequestPopup : PopupBase
{
    public ObservableCollection<string> AvaibleVersions { get; set; }
    private readonly ServerVersionFetcher serverVersionFetcher;
    private readonly InstanceManager manager;
    public CreateInstanceRequestPopup()
    {

        BindingContext = this;
        this.serverVersionFetcher = App.Current?.Handler?.MauiContext?.Services.GetService<ServerVersionFetcher>() ??
            throw new NullReferenceException("Couldnt find server version fetcher class on CreateInstanceRequestPopup");
        this.manager = App.Current?.Handler?.MauiContext?.Services.GetService<InstanceManager>() ??
            throw new NullReferenceException("Couldnt find instance manager class on CreateInstanceRequestPopup");
        AvaibleVersions = new ObservableCollection<string>();
        InitializeComponent();
        Animations.Animations.AttachHoverButtonAnimation(AddInstanceButtonConfirm);
        Animations.Animations.AttachHoverButtonAnimation(CancelAddInstanceButton);

    }

    async void onConfirm(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(InstanceNameField.Text) &&
            InstanceTypeField.SelectedIndex != -1 &&
            InstanceVersionField.SelectedIndex != -1 &&
            NameCheck(InstanceNameField.Text))
        {
            InstanceModel newInstance = new(
                Name: InstanceNameField.Text,
                Description: null,
                Type: (InstanceType)InstanceTypeField.SelectedItem,
                Version: InstanceVersionField.SelectedItem.ToString() ?? "Null",
                CustomPath: string.IsNullOrEmpty(CustomPathField.Text) || CustomPathSwitch.IsToggled == false ? null : CustomPathField.Text);
            await Hide();
            await manager.CreateInstance(newInstance);
        }
        else if (!NameCheck(InstanceNameField.Text))
        {
            WarningText.Text = "Name already exists!";
            WarningText.IsVisible = true;
        }
        else
        {
            WarningText.Text = "Some fields are not filled correctly!";
            WarningText.IsVisible = true;
        }
    }

    async void onCancel(object sender, EventArgs e)
    {
        await Hide();
    }

    private void CustomPathSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value == true)
        {
            CustomPathField.IsEnabled = true;
            CustomPathSelectorButton.IsEnabled = true;

        }
        else
        {
            CustomPathField.IsEnabled = false;
            CustomPathSelectorButton.IsEnabled = false;
        }

    }

    private async void CustomPathSelectorButton_Clicked(object sender, EventArgs e)
    {
        var folder = await FolderPicker.PickAsync(default);
        if (folder.Folder != null)
        {
            CustomPathField.Text = folder.Folder?.Path;
        }
        else
        {
            CustomPathField.Text = "";
        }
    }

    public void Setup()
    {
        InstanceNameField.Text = null;
        InstanceTypeField.SelectedIndex = -1;
        InstanceVersionField.SelectedIndex = -1;
        CustomPathSwitch.IsToggled = false;
        CustomPathField.Text = null;
        AvaibleVersions.Clear();
        WarningText.IsVisible = false;
        InstanceVersionField.IsEnabled = false;
        Show();
    }

    private void InstanceTypeField_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (InstanceTypeField.SelectedItem is InstanceType selectedType)
        {
            AvaibleVersions.Clear();
            var versions = serverVersionFetcher.GetVersions(selectedType).ToObservableCollection<string>();
            foreach (var version in versions)
            {
                AvaibleVersions.Add(version);
            }
            InstanceVersionField.IsEnabled = true;

        }
    }

    private bool NameCheck(string name)
    {
        foreach (var instance in manager.instances)
        {
            if (instance.Name == name)
                return false;
        }
        return true;
    }
}