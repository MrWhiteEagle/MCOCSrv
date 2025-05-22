using MCOCSrv.Resources.Classes;
using MCOCSrv.Resources.Content;
using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MCOCSrv;

public partial class ConsolePage : ContentPage, INotifyPropertyChanged
{
    ObservableCollection<InstanceModel> Running => Manager.running;
    public ObservableCollection<ConsoleTemplate> Tabs { get; } = new();
    private ConsoleTemplate _currentConsole;
    public ConsoleTemplate CurrentConsole
    {
        get => _currentConsole;
        set
        {
            if (_currentConsole != value)
            {
                _currentConsole = value;
                OnPropertyChanged(nameof(CurrentConsole));
                ActiveWindow.Content = _currentConsole;
            }
        }
    }
    InstanceManager Manager;
    public ConsolePage(InstanceManager instanceManager)
    {
        InitializeComponent();
        BindingContext = this;
        this.Manager = instanceManager;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Tabs.Clear();
        foreach (var instance in Running)
        {
            var tab = new ConsoleTemplate();
            tab.SetupTab(instance);
            Tabs.Add(tab);
        }
        if (Running.Count == 0)
        {
            var tab = new ConsoleTemplate();
            Tabs.Add(tab);
        }
        if (Tabs.Any())
        {
            CurrentConsole = Tabs.First();
            ActiveWindow.Content = CurrentConsole;
        }
    }

    private void Tabs_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection is ConsoleTemplate selected)
        {
            CurrentConsole = selected;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}