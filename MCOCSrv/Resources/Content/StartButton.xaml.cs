using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;

namespace MCOCSrv.Resources.Content;

public partial class StartButton : ContentView
{
    public event EventHandler<object>? StartRequested;
    public event EventHandler<object>? StopRequested;
    private InstanceModel BoundInstance;
    public StartButton()
    {
        InitializeComponent();
        this.Unloaded += OnUnloaded;
        this.Loaded += OnLoaded;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
    }

    private void Start_Instance_Clicked(object sender, EventArgs a)
    {
        StartRequested?.Invoke(this, BindingContext);
    }

    private async void Stop_Instance_Clicked(object sender, EventArgs a)
    {
        StopBorder.IsEnabled = false;
        StopBorder.IsVisible = false;
        StopBorder.InputTransparent = true;
        WorkingBorder.IsEnabled = true;
        WorkingBorder.IsVisible = true;
        WorkingBorder.InputTransparent = false;
        await Task.Delay(500);
        StopRequested?.Invoke(this, BindingContext);
    }

    private async void OnConsoleStateChange(object sender, int state)
    {
        UILogger.LogUI($"[BUTTON - {BoundInstance.Name}] State changed to {state}");
        if (sender == BoundInstance?.Console)
        {
            UILogger.LogUI($"State changed to {state}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                switch (state)
                {
                    case 0:
                        StartBorder.IsEnabled = true;
                        StartBorder.IsVisible = true;
                        StartBorder.InputTransparent = false;
                        WorkingBorder.IsEnabled = false;
                        WorkingBorder.IsVisible = false;
                        WorkingBorder.InputTransparent = true;
                        StopBorder.IsEnabled = false;
                        StopBorder.IsVisible = false;
                        StopBorder.InputTransparent = true;
                        break;
                    case 1:
                        StartBorder.IsEnabled = false;
                        StartBorder.IsVisible = false;
                        StartBorder.InputTransparent = true;
                        WorkingBorder.IsEnabled = true;
                        WorkingBorder.IsVisible = true;
                        WorkingBorder.InputTransparent = false;
                        StopBorder.IsEnabled = false;
                        StopBorder.IsVisible = false;
                        StopBorder.InputTransparent = true;
                        break;
                    case 2:
                        StartBorder.IsEnabled = false;
                        StartBorder.IsVisible = false;
                        StartBorder.InputTransparent = true;
                        WorkingBorder.IsEnabled = false;
                        WorkingBorder.IsVisible = false;
                        WorkingBorder.InputTransparent = true;
                        StopBorder.IsEnabled = true;
                        StopBorder.IsVisible = true;
                        StopBorder.InputTransparent = false;
                        break;
                }
            });

        }
    }

    // On unload remove the listener (memory leaks prevention
    private void OnUnloaded(object sender, EventArgs a)
    {
        if (BoundInstance != null && BoundInstance.Console != null)
        {
            BoundInstance.Console.ServerStateHandler -= OnConsoleStateChange;
        }
    }

    // On reload reevaluate state
    private void OnLoaded(object sender, EventArgs a)
    {
        if (BoundInstance != null && BoundInstance.Console != null)
        {
            BoundInstance.Console.ServerStateHandler -= OnConsoleStateChange;
        }
        if (BindingContext is InstanceModel instance && instance.Console != null)
        {
            UILogger.LogUI($"[BUTTON ] Instance is {instance.Name}");
            BoundInstance = instance;
            BoundInstance.Console.ServerStateHandler += OnConsoleStateChange;
            OnConsoleStateChange(BoundInstance.Console, BoundInstance.Console.State);
        }
        else
        {
            UILogger.LogUI($"[BUTTON - {BoundInstance?.Name}]Bound instance is not an instance, or its console is null!");
            BoundInstance = null;
        }
    }



}