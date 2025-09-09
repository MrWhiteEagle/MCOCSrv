using CommunityToolkit.Maui.Storage;
using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Models;
using MCOCSrv.Resources.Pages.SingletonPages;
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
    InstanceType Type;
    InstanceModel Instance;
    InstanceManager manager;
    public ObservableCollection<QuickAction> Actions { get; set; } = new();
    public ObservableCollection<PlayerData> OnlinePlayers { get; set; } = new();
    public ObservableCollection<PlayerData> BannedPlayers { get; set; }
    public ObservableCollection<PlayerData> OppedPlayers { get; set; }
    private string? Version;
    private string? Path;

    //============CONSOLE PROPERTIES============
    private ConsoleWrapper Console;
    private ObservableCollection<string> ConsoleOutputLines { get; set; } = new(); //List of console output for display

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

    //============COMMANDS FOR ACTIONS============
    public ICommand SendCommand { get; set; }
    public ICommand StopServer { get; set; }
    public ICommand StartServer { get; set; }
    public ICommand RestartServer { get; set; }
    public ICommand ForceSave { get; set; }
    public ICommand ForceBackup { get; set; }
    public ICommand SchedulerOpen { get; set; }
    public ICommand RestoreBackup { get; set; }


    public ConsoleTemplate()
    {
        SendCommand = new Command(() => ExecuteSendCommand());
        StopServer = new Command(() => ExecuteStopServer());
        StartServer = new Command(() => ExecuteStartServer());
        RestartServer = new Command(async () => await ExecuteRestartServer());
        ForceSave = new Command(() => ExecuteForceSave());
        ForceBackup = new Command(async () => await ExecuteForceBackup());
        SchedulerOpen = new Command(() => { UILogger.LogUI("Scheduler Pressed"); });
        RestoreBackup = new Command(() => { UILogger.LogUI("Restore Pressed"); });

        InitializeComponent();
        BindingContext = this;
        manager = App.Current?.Handler?.GetService<InstanceManager>() ?? throw new NullReferenceException("Couldnt fetch manager on ConsoleTemplate");

        //SetActionButtons();
        AttachButtonAnimations();
    }

    #region Setup
    //Setup tab once instance is run by console
    public void SetupTab(InstanceModel instance)
    {
        this.Instance = instance;
        this.Name = instance.Name;
        this.Type = instance.Type;
        this.Version = instance.Version;
        this.Path = instance.GetPath();
        //Defensive - Check for instance containing a console, create if not
        if (instance.Console == null)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Somehow instance has no console - adding one...");
            instance.Console = new ConsoleWrapper(instance);
            Console.BoundConsole = this;
        }
        //Bind wrapper and console to each other
        this.Console = instance.Console;
        Console.BoundConsole = this;

        //Load player data
        BannedPlayers = Instance.BannedPlayers;
        OppedPlayers = Instance.OppedPlayers;

        //Reload UI elements
        ReloadActions();
        if (!Console.IsRunning && Instance.IsRunning())
            SetInitialSidebarActionStates();
        UpdateUIState();
        PlayerListPopup.Initialize(this);

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

        ConsoleInput.ReturnCommand = SendCommand;
    }
    #endregion

    //============Console Handler Methods============
    #region Console Handlers
    private void OnConsoleOutput(object? sender, string data)
    {
        Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
        {
            ConsoleOutputLines.Add(data);
            CombinedConsoleOutput += data + Environment.NewLine;

            // Check for player join/leave events and update player list accordingly
            if (data.Contains("joined"))
                PlayerListListener(data, true);
            if (data.Contains("left"))
                PlayerListListener(data, false);
            if (data.ToLower().Contains("banned") || data.ToLower().Contains("unbanned"))
            {
                BannedPlayers = Instance.BannedPlayers = manager.GetBannedPlayers(Instance);
                PlayerBanListPopup.RequestRefresh();
            }
            if (data.ToLower().Contains("opped") || data.ToLower().Contains("de-opped"))
            {
                OppedPlayers = Instance.OppedPlayers = manager.GetOppedPlayers(Instance);
                PlayerOpListPopup.RequestRefresh();
            }

            //Scroll to end of console on new output
            ScrollToConsoleEnd();
        });
    }

    // Handle console exit event - update Ui and notify on output
    private void OnConsoleExit(object? sender, int code)
    {
        // Notify UI about server exit
        Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
        {
            ConsoleOutputLines.Add($"[MCOCSrv] Server exited with code {code}.");
            CombinedConsoleOutput += $"[MCOCSrv] Server exited with code {code}." + Environment.NewLine;
            ScrollToConsoleEnd();
            UpdateUIState();
        });
    }

    private void ExecuteSendCommand()
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

    // Scroll to end of output on new output
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
    #endregion

    //============Actions Implementation============
    #region Actions Handlers
    private void ExecuteStartServer()
    {
        if (!Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Requesting Server Start...");
            Console.StartServer();
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot request start - server running already or console is null.");
        }
        UpdateUIState();
    }

    private void ExecuteStopServer()
    {

        if (Console.IsRunning)
        {
            UILogger.LogUI($"[CONSOLE {Name}] Requesting Server Stop...");
            Console.StopServer();
        }
        else
        {
            UILogger.LogUI($"[CONSOLE {Name}] Cannot request stop - server is already not running or console is null.");
        }
        UpdateUIState();
    }

    private async Task ExecuteRestartServer()
    {
        if (Console != null)
        {
            await Console.RestartServer();
            UpdateUIState();
        }
    }


    private void ExecuteForceSave()
    {
        if (Console.IsRunning)
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

    // Handle force backup - pick world folder and zip it to backups folder
    private async Task ExecuteForceBackup()
    {
        var world = await FolderPicker.PickAsync(Console.GetWorkingPath());
        if (world.IsSuccessful && world.Folder != null)
        {
            await Console.ForceServerBackup(world.Folder.Name);
        }
    }

    private void SetActionButtons()
    {
        RestoreButton.Command = RestoreBackup;
    }
    #endregion

    //============QuickActions============
    #region QuickAction
    private void ExecuteQuickAction(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Button button && button.BindingContext is QuickAction action)
        {
            Console.SendCommand(action.Command);
        }
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
    public void ReloadActions()
    {
        Actions.Clear();
        foreach (var action in Instance.Actions)
        {
            Actions.Add(action);
        }
        AttachButtonAnimationsQuickActions();
    }
    #endregion

    //============SIDEBAR ACTIONS HANDLERS==============
    #region Sidebar Handlers
    // Set initial values that mirror current settings
    public void SetInitialSidebarActionStates()
    {
        if (Instance != null && Console != null)
        {
            foreach (var setting in Instance.Settings)
            {
                if (setting.Name == "difficulty")
                    DifficultyPicker.SelectedIndex = int.Parse(setting.Value);
                if (setting.Name == "white-list" && setting.Value == "true")
                    WhitelistSwitch.IsToggled = true;
                else
                    WhitelistSwitch.IsToggled = false;
                if (setting.Name == "hardcore" && setting.Value == "true")
                    HardcoreSwitch.IsToggled = true;
                else
                    HardcoreSwitch.IsToggled = false;
            }
        }
    }
    private async void OpenConfigRequest(object? sender, EventArgs e)
    {
        if (Instance != null)
        {
            await Shell.Current.Navigation.PushAsync(new InstanceSettingsPage(Instance));
        }
    }

    private void ForceReload(object? sender, EventArgs e)
    {
        if (Instance != null && Console != null)
        {
            Console.SendCommand("reload");
        }
    }

    // Handle picker actions - console panel
    private void HandleChangeOption(object? sender, EventArgs e)
    {
        if (sender is Picker && Console != null)
        {
            if (sender == DifficultyPicker)
            {
                int value = DifficultyPicker.SelectedIndex;
                if (value >= 0 && value <= 3)
                    Console.SendCommand("difficulty " + value);
            }
            if (sender == WeatherPicker && Console != null)
            {
                int value = WeatherPicker.SelectedIndex;
                switch (value)
                {
                    case 0:
                        Console.SendCommand("weather clear");
                        break;
                    case 1:
                        Console.SendCommand("weather rain");
                        break;
                    case 2:
                        Console.SendCommand("weather thunder");
                        break;
                    default:
                        return;
                }
            }
        }
    }

    private void HandleSwitches(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch)
        {
            if (sender == WhitelistSwitch)
                Console.SendCommand(e.Value == true ? "whitelist on" : "whitelist off");
            if (sender == PvpSwitch)
                Console.SendCommand(e.Value == true ? "gamerule pvp true" : "gamerule pvp false");
            //if (sender == HardcoreSwitch)
            //    Console.SendCommand(e.Value == true ? "gamemode hardcore @a" : "gamemode survival @a");
            if (sender == KeepInventorySwitch)
                Console.SendCommand(e.Value == true ? "gamerule keepInventory true" : "gamerule keepInventory false");
            if (sender == DayCycleSwitch)
                Console.SendCommand(e.Value == true ? "gamerule doDaylightCycle true" : "gamerule doDaylightCycle false");
            if (sender == MobSpawnSwitch)
                Console.SendCommand(e.Value == true ? "gamerule doMobSpawning true" : "gamerule doMobSpawning false");
        }
    }

    #endregion
    //============OTHER METHODS============

    //Update UI based on console state
    public void UpdateUIState()
    {
        bool isRunning = Console.IsRunning;
        if (isRunning)
        {
            StartButton.ActionName = "Stop";
            StartButton.Glyph = MDIcons.CheckboxBlankOutline;
            StartButton.GlyphColor = Colors.Red;
            StartButton.Command = StopServer;
            Status_Blimp.Fill = new SolidColorBrush(Colors.LimeGreen);
            Status_Text.Text = "Running";
        }
        else
        {
            StartButton.ActionName = "Start";
            StartButton.Glyph = MDIcons.PlayOutline;
            StartButton.GlyphColor = Colors.LawnGreen;
            StartButton.Command = StartServer;
            Status_Blimp.Fill = new SolidColorBrush(Colors.Red);
            Status_Text.Text = "Stopped";
        }
    }

    //============PLAYER LIST HANDLER============
    #region Player List Handler

    // ON JOIN/LEAVE OUTPUT CAUGHT, UPDATE PLAYER LIST
    public void PlayerListListener(string data, bool join_leave)
    {
        PlayerData? modify = new PlayerData("", "");
        if (join_leave)
        {
            List<string> words = data.Split(" ").ToList();
            words.RemoveRange(0, 3);
            words.RemoveRange(1, words.Count - 1);
            OnlinePlayers.Add(new PlayerData("", words[0]));
        }
        else
        {
            List<string> words = data.Split(" ").ToList();
            words.RemoveRange(0, 3);
            words.RemoveRange(1, words.Count - 1);
            foreach (var player in OnlinePlayers)
            {
                if (player.Name == words[0])
                {
                    // Need to have placeholder in order to avoid modifying collection white iterating
                    modify = player;
                }
            }
            if (modify != null)
            {
                OnlinePlayers.Remove(modify);
            }
        }
        PlayerListPopup.RequestRefresh();
    }

    private void HandleShowPlayerList(object? sender, EventArgs e)
    {
        if (sender == ShowPlayerListButton)
        {
            PlayerListPopup.Initialize(this);
            PlayerListPopup.OnOpen();
        }
        if (sender == ShowBannedPlayersButton)
        {
            PlayerBanListPopup.Initialize(this);
            PlayerBanListPopup.OnOpen();
        }
        if (sender == ShowOppedPlayersButton)
        {
            PlayerOpListPopup.Initialize(this);
            PlayerOpListPopup.OnOpen();
        }
    }

    //Used by sidebar actions
    public void RequestPlayerCommand(PlayerData player, string cmd)
    {
        Console.SendCommand($"{cmd} {player.Name}");
    }

    #endregion

    #region Animation
    private void AttachButtonAnimations()
    {
        AttachButtonAnimationsQuickActions();
        foreach (var item in ConsoleSidePanel.Children.OfType<Button>())
        {
            Animations.Animations.AttachHoverButtonAnimation(item);
        }
        Animations.Animations.AttachHoverButtonAnimation(ManageQuickActionsBtn);
    }

    private void AttachButtonAnimationsQuickActions()
    {
        foreach (var item in QuickActionList.Children.OfType<Button>())
        {
            Animations.Animations.AttachHoverButtonAnimation(item);
        }
    }



    #endregion
}