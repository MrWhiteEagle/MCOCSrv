using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class PlayerListPopup : PopupBase
{
    public ObservableCollection<PlayerData> Players { get; set; } = new();
    private ConsoleTemplate console;
    public PlayerListPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Beggining of player list lifecycle - assign a console to the popup on Console lifecycle start.
    public void Initialize(ConsoleTemplate console)
    {
        this.console = console ?? throw new NullReferenceException("Console was null in playerlist");
        RequestRefresh();
        BindingContext = this;
    }

    //Open popup
    public void OnOpen()
    {
        Show();
        RequestRefresh();
    }

    //Close popup
    private async void OnClose(object? sender, EventArgs e)
    {
        await Hide();
    }

    // Request reload from console and update the player list.
    public void RequestRefresh()
    {
        Players.Clear();
        foreach (var player in console.OnlinePlayers)
        {
            Players.Add(player);
        }
        if (Players.Count == 0)
        {
            EmptyPlaceholder.IsVisible = true;
        }
        else
        {
            EmptyPlaceholder.IsVisible = false;
        }
    }

    // Request player actions from console.
    #region Player Actions
    private async void KickPlayerRequest(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PlayerData player)
        {
            console.RequestPlayerCommand(player, "kick");
            await Task.Delay(500);
            RequestRefresh();
        }
    }

    private async void BanPlayerRequest(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PlayerData player)
        {
            console.RequestPlayerCommand(player, "ban");
            await Task.Delay(500);
            RequestRefresh();
        }
    }

    private void OpPlayerRequest(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PlayerData player)
        {
            console.RequestPlayerCommand(player, "op");
        }
    }
    #endregion
}