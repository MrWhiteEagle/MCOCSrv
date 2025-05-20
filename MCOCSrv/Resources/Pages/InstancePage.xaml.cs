using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MCOCSrv;

public partial class InstancePage : ContentPage
{
    public ObservableCollection<InstanceModel> Instances => manager.instances;
    public ObservableCollection<InstanceModel> Running => manager.running;

    private InstanceManager manager;
    public InstancePage(InstanceManager instanceManager)
    {
        InitializeComponent();
        this.manager = instanceManager;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await manager.FetchInstances();
    }

    void onCreateInstance(object sender, EventArgs e)
    {
        CreateInstancePopup.IsVisible = true;
        CreateInstancePopup.InputTransparent = false;
        CreateInstancePopup.resetInstancePopup();
    }

    void OnDeleteRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            Debug.WriteLine($"DELETING INSTANCE {instance.Name}");
            DeletionConfirmationPopup.Show(instance);
        }
    }

    void OnStartRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            instance.InitializeConsole();
            manager.running.Add(instance);
        }
    }
}