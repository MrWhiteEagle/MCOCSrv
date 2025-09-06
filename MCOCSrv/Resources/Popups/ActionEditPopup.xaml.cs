using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Popups;

public partial class ActionEditPopup : PopupBase
{
    ManageQuickActionsPopup? context;
    public ActionEditPopup()
    {
        InitializeComponent();
    }

    //HANDLE SAVE AND CANCEL
    private async void SaveButtonClicked(object sender, EventArgs e)
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
                await Animations.Animations.FadeOutAnimation(this);
                this.IsVisible = false;
                this.InputTransparent = true;
            }
        }
    }

    private async void CancelButtonClicked(object sender, EventArgs e)
    {
        await Hide();
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
        Show();
        AttachAnimations();
    }

    private void AttachAnimations()
    {
        Animations.Animations.AttachHoverButtonAnimation(CancelButton);
        Animations.Animations.AttachHoverButtonAnimation(SaveButton);
    }
}