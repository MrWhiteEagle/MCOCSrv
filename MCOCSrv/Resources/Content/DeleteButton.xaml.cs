namespace MCOCSrv.Resources.Content;

public partial class DeleteButton : ContentView
{
    public event EventHandler<object>? DeleteRequested;
    public DeleteButton()
    {
        InitializeComponent();
    }

    private void Delete_Button_Clicked(object sender, EventArgs args)
    {
        DeleteRequested?.Invoke(this, BindingContext);
    }
}