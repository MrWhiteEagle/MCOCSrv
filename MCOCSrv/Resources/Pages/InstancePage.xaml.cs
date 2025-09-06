using MCOCSrv.Resources.Animations;
using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Content;
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
        AttachButtonAnimations();
    }

    void onCreateInstance(object sender, EventArgs e)
    {
        CreateInstancePopup.Setup();
        CreateInstancePopup.Show();
    }

    // Request instance deletion, and show confirmation popup
    void OnDeleteRequest(object sender, object item)
    {
        if (item is InstanceModel instance)
        {
            Debug.WriteLine($"Delete request: {instance.Name}");
            DeletionConfirmationPopup.Setup(instance);
            DeletionConfirmationPopup.Show();
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

    #region Animations

    private void AttachButtonAnimations()
    {
        Animations.AttachHoverButtonAnimation(AddNewInstanceButton);
    }
    private void InstanceListButtonLoaded(object sender, EventArgs e)
    {
        if (sender is StartButton || sender is SettingButton || sender is DeleteButton)
        {
            Animations.AttachHoverButtonAnimation(sender);
        }
    }
    #endregion

}