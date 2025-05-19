namespace MCOCSrv.Resources.Content;

public partial class StartButton : ContentView
{
    public event EventHandler<object> StartRequested;
    public StartButton()
    {
        InitializeComponent();
    }
    public void Start_Instance_Clicked(object sender, EventArgs a)
    {
        StartRequested?.Invoke(this, BindingContext);
    }

}