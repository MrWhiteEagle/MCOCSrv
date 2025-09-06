using MCOCSrv.Resources.Classes;

namespace MCOCSrv
{
    public partial class MainPage : ContentPage
    {
        private readonly ServerVersionFetcher serverVersionFetcher;
        int count = 0;
        int textID = 0;

        public MainPage(ServerVersionFetcher serverVersionFetcher)
        {
            InitializeComponent();
            this.serverVersionFetcher = serverVersionFetcher;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            UILogger.LogUI("[MCOCSrv] Loaded Main App");

            try
            {
                UILogger.LogUI("[MCOCSrv] Trying Fetch....");
                LoadingSourcePopup.IsVisible = true;
                LoadingSourcePopup.IsEnabled = true;
                LoadingSourcePopup.InputTransparent = false;
                await serverVersionFetcher.InitializeSources();
                LoadingSourcePopup.IsVisible = false;
                LoadingSourcePopup.IsEnabled = false;
                LoadingSourcePopup.InputTransparent = true;
            }
            catch (Exception ex)
            {
                LoadingSourcePopup.IsVisible = false;
                await DisplayAlert("[MCOCSrv] Error fetching sources!", $"Caught exception: {ex.Message}", "Got it");
            }
        }
    }

}
