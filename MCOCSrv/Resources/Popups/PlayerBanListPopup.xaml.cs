using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class PlayerBanListPopup : PopupBase
{
    public ObservableCollection<PlayerData> BannedPlayers { get; set; } = new();
    private ConsoleTemplate? console;
    public PlayerBanListPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Beggining of player list lifecycle - assign a console to the popup on Console lifecycle start.
    public void Initialize(ConsoleTemplate console)
    {
        BindingContext = this;
        this.console = console;
        RequestRefresh();
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
        if (console != null)
        {
            BannedPlayers.Clear();
            foreach (var player in console.BannedPlayers)
            {
                BannedPlayers.Add(player);
            }
        }
        else
        {
            throw new NullReferenceException("Console not initialized in PlayerBanListPopup");
        }
    }

    // Request player actions from console.
    #region Player Actions

    private void PardonPlayerRequest(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PlayerData player && console != null)
        {
            console.RequestPlayerCommand(player, "pardon");
        }
        else if (console == null)
        {
            throw new NullReferenceException("Console not initialized in PlayerBanListPopup");
        }
    }
    #endregion
}