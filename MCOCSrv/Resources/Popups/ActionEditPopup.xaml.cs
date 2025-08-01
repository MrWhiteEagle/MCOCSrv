using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Popups;

public partial class ActionEditPopup : ContentView
{
    ManageQuickActionsPopup? context;
    public ActionEditPopup()
    {
        InitializeComponent();
    }

    //HANDLE SAVE AND CANCEL
    private void SaveButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is QuickAction && context != null)
        {
            if (string.IsNullOrWhiteSpace(ActionNameField.Text) || string.IsNullOrWhiteSpace(ActionCommandField.Text))
            {
                UILogger.LogUI("[ACTION EDIT] Action name or command cannot be empty.");
            }
            else
            {
                context.EditActionCallback((QuickAction)BindingContext, new QuickAction(ActionNameField.Text, ActionCommandField.Text));
                this.IsVisible = false;
                this.InputTransparent = true;
            }
        }
    }

    private void CancelButtonClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
        this.InputTransparent = true;
        ActionNameField.Text = string.Empty;
        ActionCommandField.Text = string.Empty;
    }

    // INITIALIZE POPUP
    public void Initialize(QuickAction action, ManageQuickActionsPopup context)
    {
        this.context = context;
        BindingContext = action;
        ActionNameField.Text = action.Name;
        ActionCommandField.Text = action.Command;
        this.IsVisible = true;
        this.InputTransparent = false;
    }
}