namespace MCOCSrv;

public partial class InstancePage : ContentPage
{
    public InstancePage()
    {
        InitializeComponent();
    }

    void onCreateInstance(object sender, EventArgs e)
    {
        CreateInstancePopup.IsVisible = true;
        CreateInstancePopup.InputTransparent = false;
        CreateInstancePopup.resetInstancePopup();
    }
}