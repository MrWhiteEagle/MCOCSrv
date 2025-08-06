using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.ComponentModel;

namespace MCOCSrv.Resources.Popups;

public partial class AreYouSurePopup : ContentView, INotifyPropertyChanged
{
    private InstanceModel? toDelete;
    private InstanceManager? manager;
    public AreYouSurePopup()
    {
        InitializeComponent();
        BindingContext = this;
        this.manager = App.Current?.Handler.GetService<InstanceManager>();
    }

    private void Cancel_Button_Clicked(object sender, EventArgs e)
    {
        Hide();
    }

    private async void Confirm_Button_Clicked(Object sender, EventArgs e)
    {
        if (manager != null && toDelete != null)
            await manager.DeleteInstance(toDelete);
        Hide();
    }

    public void Show(InstanceModel context)
    {
        this.IsEnabled = true;
        this.IsVisible = true;
        this.InputTransparent = false;
        toDelete = context;
        DeletionInfo.Text = $"{toDelete.Name} will be deleted forever!";
    }

    public void Hide()
    {
        this.IsEnabled = false;
        this.IsVisible = false;
        this.InputTransparent = true;
    }
}