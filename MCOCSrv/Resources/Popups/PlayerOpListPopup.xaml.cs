using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;

namespace MCOCSrv.Resources.Popups;

public partial class PlayerOpListPopup : PopupBase
{
    public ObservableCollection<PlayerData> OppedPlayers { get; set; } = new();
    private ConsoleTemplate console;
    public PlayerOpListPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Beggining of player list lifecycle - assign a console to the popup on Console lifecycle start.
    public void Initialize(ConsoleTemplate console)
    {
        BindingContext = this;
        this.console = console ?? throw new NullReferenceException("Console was null in OPPLayerList");
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
        OppedPlayers.Clear();
        foreach (var player in console.OppedPlayers)
        {
            OppedPlayers.Add(player);
        }
    }

    // Request player actions from console.
    #region Player Actions
    private void DeopPlayerRequest(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is PlayerData player)
        {
            console.RequestPlayerCommand(player, "deop");
        }
    }
    #endregion
}