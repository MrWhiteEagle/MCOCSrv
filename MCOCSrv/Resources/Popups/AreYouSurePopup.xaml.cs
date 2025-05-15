using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Popups;

public partial class AreYouSurePopup : ContentView
{
    private InstanceModel toDelete;
    private InstanceManager manager;
    public AreYouSurePopup()
    {
        InitializeComponent();
        this.manager = Application.Current.Handler.MauiContext.Services.GetService<InstanceManager>();
    }

    private void Cancel_Button_Clicked(object sender, EventArgs e)
    {
        Hide();
    }

    private async void Confirm_Button_Clicked(Object sender, EventArgs e)
    {
        await manager.DeleteInstance(toDelete);
        Hide();
    }

    public void Show(InstanceModel context)
    {
        this.IsEnabled = true;
        this.IsVisible = true;
        this.InputTransparent = false;
        toDelete = context;
    }

    public void Hide()
    {
        this.IsEnabled = false;
        this.IsVisible = false;
        this.InputTransparent = true;
    }
}