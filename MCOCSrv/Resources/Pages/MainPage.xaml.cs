namespace MCOCSrv
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        int textID = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void ChangeText(object sender, EventArgs e)
        {   
            if (textID == 0)
            {
                textID = 1;
                ChangingText.Text = "Text was changed";
            }
            else
            {
                textID = 0;
                ChangingText.Text = "But it's still a .NET MAUI APP!";
            }
            
        }
    }

}
