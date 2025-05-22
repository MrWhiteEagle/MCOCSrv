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

    void OnDeleteRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            Debug.WriteLine($"DELETING INSTANCE {instance.Name}");
            DeletionConfirmationPopup.Show(instance);
        }
    }

    async void OnStartRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            if (instance.Console != null)
            {
                await instance.Console.StartServer();
            }
            if (!manager.running.Contains(instance))
            {
                manager.running.Add(instance);
            }
        }
    }

    async void OnStopRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            if (instance.Console != null)
            {
                await instance.Console.StopServer();
                instance.Console.Dispose();
            }
            manager.running.Remove(instance);
        }
    }

}