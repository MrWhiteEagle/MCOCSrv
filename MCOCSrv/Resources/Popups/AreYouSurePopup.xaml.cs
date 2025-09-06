using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Popups;

public partial class AreYouSurePopup : PopupBase
{
    private InstanceModel? toDelete;
    private InstanceManager? manager;
    public AreYouSurePopup()
    {
        InitializeComponent();
        BindingContext = this;
        this.manager = App.Current?.Handler.GetService<InstanceManager>();
        this.IsVisible = false;
        this.InputTransparent = true;
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

    public void Setup(InstanceModel context)
    {
        toDelete = context;
        DeletionInfo.Text = $"{toDelete.Name} will be deleted forever!";
    }
}