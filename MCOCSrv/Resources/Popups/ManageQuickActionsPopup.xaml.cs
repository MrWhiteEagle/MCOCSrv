using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class ManageQuickActionsPopup : PopupBase
{
    public ObservableCollection<QuickAction> Actions { get; set; } = new();
    private InstanceModel instance;
    private ConsoleTemplate console;
    public ManageQuickActionsPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // INITALIZE THE POPUP WITH INSTANCE AND CONSOLE
    public void Initialize(InstanceModel instance, ConsoleTemplate console)
    {
        if (console != null && instance != null)
        {
            this.instance = instance;
            this.console = console;
            LoadActions();
            Show();
        }
        else if (instance == null)
        {
            throw new NullReferenceException("Instance is null, cannot initialize ManageQuickActionsPopup.");
        }
        else if (console == null)
        {
            throw new NullReferenceException("Console or Instance is null, cannot initialize ManageQuickActionsPopup.");
        }
        AttachAnimations();
    }

    // HANDLING SAVING OR CANCELLING
    private async void CancelButtonClicked(object sender, EventArgs e)
    {
        await Hide();
        Actions.Clear();
    }

    private async void SaveButtonClicked(object sender, EventArgs e)
    {
        await Hide();
        if (console != null)
            console.OnActionsEdited(Actions);
        else
            throw new NullReferenceException("Console is null, cannot save actions.");
        Actions.Clear();
    }

    // HANDLING ACTION EDITING, DELETING AND ADDING - CALLS THE ACTION EDIT POPUP TO SHOW
    private void DeleteActionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is QuickAction action)
        {
            Actions.Remove(action);
        }
    }

    private void EditActionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is QuickAction action)
        {
            ActionEditPopup.Initialize(action, this);
        }
    }
    private void AddActionClicked(object sender, EventArgs e)
    {
        ActionEditPopup.Initialize(new QuickAction("New Action", ""), this);
    }

    // HANDLING ACTION CHANGES FROM ACTION EDIT POPUP
    public void EditActionCallback(QuickAction action, QuickAction newAction)
    {
        int i = Actions.IndexOf(action);
        if (i >= 0)
            Actions[i] = newAction;
        else
            Actions.Add(newAction);
        AttachAnimations();
    }

    // RELOAD ACTIONS - REEVALUATE LIST AND SYNC WITH CONSOLE
    private void LoadActions()
    {
        if (console != null)
        {
            Actions.Clear();
            foreach (var action in console.Actions)
            {
                Actions.Add(action);
            }
            if (Actions.Count != 0)
            {
                ActionList.IsVisible = true;
                EmptyActionFallBack.IsVisible = false;
            }
            else
            {
                ActionList.IsVisible = false;
                EmptyActionFallBack.IsVisible = true;
            }

        }
        else
        {
            throw new NullReferenceException("Console is null, cannot load actions.");
        }
    }

    private void AttachAnimations()
    {
        Animations.Animations.AttachHoverButtonAnimation(AddActionButton);
        Animations.Animations.AttachHoverButtonAnimation(CancelButton);
        Animations.Animations.AttachHoverButtonAnimation(SaveButton);
        foreach (var item in ActionList.Children.OfType<Border>())
        {
            if (item.Content is Grid grid)
            {
                foreach (var child in grid.Children.OfType<Button>())
                {
                    Animations.Animations.AttachHoverButtonAnimation(child);
                }
            }
        }
    }
}