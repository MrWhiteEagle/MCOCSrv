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

    void onCreateInstance(object sender, EventArgs e)
    {
        CreateInstancePopup.IsVisible = true;
        CreateInstancePopup.InputTransparent = false;
        CreateInstancePopup.resetInstancePopup();
    }

    // Request instance deletion, and show confirmation popup
    void OnDeleteRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            Debug.WriteLine($"Delete request: {instance.Name}");
            DeletionConfirmationPopup.Show(instance);
        }
    }

    // Request instance startup to console, and add instance to running list
    void OnStartRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            if (instance.Console != null)
            {
                instance.Console.StartServer();
            }
            if (!manager.running.Contains(instance))
            {
                manager.running.Add(instance);
            }
        }
    }

    // Request instance shutdown, remove from running list, dispose console
    void OnStopRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            if (instance.Console != null)
            {
                instance.Console.StopServer();
                instance.Console.Dispose();
            }
            manager.running.Remove(instance);
        }
    }

}