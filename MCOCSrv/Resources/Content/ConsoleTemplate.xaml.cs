using MCOCSrv.Resources.Models;
using MCOCSrv.Resources.Raw;
using System.Diagnostics;

namespace MCOCSrv.Resources.Content;

public partial class ConsoleTemplate : ContentView
{
    public string Name { get; set; } = "Empty";
    InstanceType? Type;
    string? Version;
    string? Path;
    public ConsoleTemplate()
    {
        InitializeComponent();
        BindingContext = this;
        Debug.WriteLine(Name);
    }

    public void SetupTab(InstanceModel instance)
    {
        this.Name = instance.Name;
        this.Type = instance.Type;
        this.Version = instance.TypeVersion;
        this.Path = instance.GetPath();
    }
}