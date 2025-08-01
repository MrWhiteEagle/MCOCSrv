using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MCOCSrv.Resources.Content;

public partial class ConsoleTemplate : ContentView, INotifyPropertyChanged
{
    // ============INSTANCE/TAB PROPERTIES============
    private string _name = "Empty";
    public string Name
    {
        get => _name;

        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    InstanceType? Type;
    InstanceModel Instance;
    InstanceManager manager;
    public ObservableCollection<QuickAction> Actions { get; set; } = new();
    string? Version;
    string? Path;

    //============CONSOLE PROPERTIES============
    ConsoleWrapper Console;
    private ObservableCollection<string> ConsoleOutputLines { get; set; } = new();

    //OUTPUT PROPERTY
    private string _combinedConsoleOutput;
    public string CombinedConsoleOutput
    {
        get => _combinedConsoleOutput;
        set
        {
            if (_combinedConsoleOutput != value)
            {
                _combinedConsoleOutput = value;
                OnPropertyChanged();
            }
        }
    }

    //============COMMANDS FOR QUICK ACTIONS============
    public ICommand SendCommand { get; set; }
    public ICommand StopServer { get; set; }
    public ICommand StartServer { get; set; }
    public ICommand RestartServer { get; set; }
    public ICommand ForceSave { get; set; }
    public ICommand ForceBackup { get; set; }


    public ConsoleTemplate()
    {
        SendCommand = new Command(async () => await ExecuteSendCommand());
        StopServer = new Command(async () => await ExecuteStopServer());
        StartServer = new Command(() => ExecuteStartServer());
        RestartServer = new Command(async () => await ExecuteRestartServer());
        ForceSave = new Command(() => ExecuteForceSave());
        //ForceBackup = new Command(async () => await ExecuteForceBackup());

        InitializeComponent();
        BindingContext = this;
        manager = App.Current.Handler.GetService<InstanceManager>();
    }

    public void SetupTab(InstanceModel instance)
    {
        this.Instance = instance;
        this.Name = instance.Name;
        this.Type = instance.Type;
        this.Version = instance.Version;
        this.Path = instance.GetPath();
        //!!!!!!!!!!!!!!!!this.Actions = instance.Actions;
        //Defensive - Check for instance containing a console, create if not
        if (instance.Console == null)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Somehow instance has no console - adding one...");
            instance.Console = new ConsoleWrapper(instance);
        }
        this.Console = instance.Console;
        ReloadActions();

        //Defensive - duplicate event handlers prevention
        Console.ConsoleOutputHandler -= OnConsoleOutput;
        Console.ConsoleExitHandler -= OnConsoleExit;

        Console.ConsoleOutputHandler += OnConsoleOutput;
        Console.ConsoleExitHandler += OnConsoleExit;

        //Restore console history from wrapper - if it exists, if not - assign empty
        ConsoleOutputLines.Clear();
        if (Console.ConsoleOutput != null)
        {
            foreach (string line in Console.ConsoleOutput)
            {
                ConsoleOutputLines.Add(line);
                CombinedConsoleOutput += line + Environment.NewLine;
            }
            ScrollToConsoleEnd();
        }
        else
        {
            CombinedConsoleOutput = string.Empty;
        }
        UpdateActionsState();

        ConsoleInput.ReturnCommand = SendCommand;


    }

    //============Console Handler Methods============
    private void OnConsoleOutput(object? sender, string data)
    {
        Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
        {
            ConsoleOutputLines.Add(data);
            CombinedConsoleOutput += data + Environment.NewLine;
            ScrollToConsoleEnd();
        });
    }

    private void OnConsoleInput()
    {

    }

    private void OnConsoleExit(object? sender, int code)
    {
        Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
        {
            ConsoleOutputLines.Add($"[MCOCSrv] Server exited with code {code}.");
            CombinedConsoleOutput += $"[MCOCSrv] Server exited with code {code}." + Environment.NewLine;
            ScrollToConsoleEnd();
            UpdateActionsState();
        });
    }

    //============Actions Implementation============
    private void ExecuteStartServer()
    {
        if (Console != null && !Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Requesting Server Start...");
            Console.StartServer();
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot request start - server running already or console is null.");
        }
        UpdateActionsState();
    }

    private async Task ExecuteStopServer()
    {

        if (Console != null && Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Requesting Server Stop...");
            await Console.StopServer();
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot request stop - server is already not running or console is null.");
        }
        UpdateActionsState();
    }

    private async Task ExecuteSendCommand()
    {
        if (Console != null && Console.IsRunning && !string.IsNullOrEmpty(ConsoleInput.Text))
        {
            UILogger.LogUI($"[CONSOLE {Name}] Sending command: {ConsoleInput.Text}");
            Console.SendCommand(ConsoleInput.Text);
        }
        else if (Console == null)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot send command - console is NULL");
        }
        else if (Console != null && !Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot send command - server not running");
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot send command - command is empty or null.");
            UILogger.LogUI($"[CONSOLE {Name}] ");
        }

        ConsoleInput.Text = string.Empty;
    }

    private async Task ExecuteRestartServer()
    {
        if (Console != null)
        {
            await Console.RestartServer();
            UpdateActionsState();
        }
    }

    private void ExecuteForceSave()
    {
        if (Console != null && Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Requesting save...");
            Console.SendCommand("save-all flush");
        }
        else if (Console == null)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot force a save - console is NULL");
        }
        else if (!Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot force a save - server not running");
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot force a save. No reason, just not feeling like it");
        }
    }

    private void ExecuteQuickAction(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is QuickAction action)
        {
            Console.SendCommand(action.Command);
        }
    }


    //============OTHER METHODS============
    private void UpdateActionsState()
    {
        bool isRunning = Console.IsRunning;
        if (isRunning)
        {
            Start_Stop_Image.Source = "stop_icon_console.png";
            Start_Stop_Button.Command = StopServer;
        }
        else
        {
            Start_Stop_Image.Source = "start_icon_console.png";
            Start_Stop_Button.Command = StartServer;
        }
    }
    private async void ScrollToConsoleEnd()
    {
        await Task.Delay(1);
        await ConsoleScrollView.ScrollToAsync(ConsoleOutput, ScrollToPosition.End, true);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // OPEN MANAGE QUICK ACTIONS POPUP
    private void ManageQuickActionsBtn_Clicked(object sender, EventArgs e)
    {
        QuickActionsPopup.Initialize(Instance, this);
    }

    // CALLBACK FOR ACTION EDITING FROM POPUP - SAVE INSTANCE AND RELOAD ACTIONS
    public async void OnActionsEdited(ObservableCollection<QuickAction> newActions)
    {
        Instance.Actions.Clear();
        foreach (var action in newActions)
        {
            Instance.Actions.Add(action);
        }
        await manager.SaveInstance(Instance);
        ReloadActions();
        UILogger.LogUI($"[CONSOLE {Name}] Actions saved successfully.");
    }

    // LOAD ACTIONS FROM INSTANCE
    private void ReloadActions()
    {
        Actions.Clear();
        foreach (var action in Instance.Actions)
        {
            Actions.Add(action);
        }
    }
}