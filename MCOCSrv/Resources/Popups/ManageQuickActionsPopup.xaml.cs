using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class ManageQuickActionsPopup : ContentView
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
        this.instance = instance;
        this.console = console;
        if (console != null && instance != null)
        {
            LoadActions();
            this.IsVisible = true;
            this.InputTransparent = false;
        }
        else
        {
            UILogger.LogUI($"[MANAGE QUICK ACTIONS] Instance or console is null, cannot initialize popup.");
        }
    }

    // HANDLING SAVING OR CANCELLING
    private void CancelButtonClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
        this.InputTransparent = true;
        Actions.Clear();
    }

    private void SaveButtonClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
        this.InputTransparent = true;
        console.OnActionsEdited(Actions);
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
    }

    // RELOAD ACTIONS - REEVALUATE LIST AND SYNC WITH CONSOLE
    private void LoadActions()
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
}