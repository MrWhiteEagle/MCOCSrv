using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using MCOCSrv.Resources.Raw;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MCOCSrv.Resources.Popups;

public partial class CreateInstanceRequestPopup : ContentView
{

    public CreateInstanceRequestPopup()
    {

        BindingContext = this;
        this.serverVersionFetcher = App.Current.Handler.MauiContext.Services.GetService<ServerVersionFetcher>();
        AvaibleVersions = new ObservableCollection<string>();
        InitializeComponent();

    }
    public ObservableCollection<string> AvaibleVersions { get; set; }
    private readonly ServerVersionFetcher serverVersionFetcher;

    async void onConfirm(object sender, EventArgs e)
    {
        this.IsVisible = false;
        this.InputTransparent = true;
        if (!string.IsNullOrEmpty(InstanceNameField.Text) &&
            InstanceTypeField.SelectedIndex != -1 &&
            !string.IsNullOrEmpty(InstanceTypeVersionField.SelectedItem.ToString()))
        {
            InstanceModel newInstance = new(
                Name: InstanceNameField.Text,
                Description: null,
                Type: (InstanceType)InstanceTypeField.SelectedItem,
                TypeVersion: InstanceTypeVersionField.SelectedItem.ToString(),
                CustomPath: string.IsNullOrEmpty(CustomPathField.Text) || CustomPathCheckbox.IsChecked == false ? null : CustomPathField.Text);
            await newInstance.CreateInstance();
            await serverVersionFetcher.DownloadInstance(newInstance);
        }
        else
        {
            Debug.WriteLine("not all fields are correctly filled!");
        }
    }

    void onCancel(object sender, EventArgs e)
    {
        this.IsVisible = false;
        this.InputTransparent = true;
    }

    private void CustomPathCheckbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
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

    public void resetInstancePopup()
    {
        InstanceNameField.Text = null;
        InstanceTypeField.SelectedIndex = -1;
        InstanceTypeVersionField.SelectedIndex = -1;
        CustomPathCheckbox.IsChecked = false;
        CustomPathField.Text = null;
        AvaibleVersions.Clear();
    }

    private void InstanceTypeField_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (InstanceTypeField.SelectedItem is InstanceType selectedType)
        {
            AvaibleVersions.Clear();
            var versions = serverVersionFetcher.getVersions(selectedType).ToObservableCollection<string>();
            foreach (var version in versions)
            {
                AvaibleVersions.Add(version);
            }

        }
    }
}