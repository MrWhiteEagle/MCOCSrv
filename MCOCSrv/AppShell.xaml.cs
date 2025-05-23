using System.Diagnostics;

namespace MCOCSrv
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Navigated += onNavigation;
        }

        private void onNavigation(object? sender, ShellNavigatedEventArgs e)
        {

            var route = e.Current?.Location?.ToString();
            Debug.WriteLine(route.ToString());

            string newTitle = route switch
            {
                "//MainPage" => "MCOCSrv - Home",
                "//InstancePage" => "MCOCSrv - Instances",
                "//ConsolePage" => "MCOCSrv - Server Console",
                "//AboutPage" => "MCOCSrv - App Info",
                "//SettingsPage" => "MCOCSrv - App Settings",
                "//InstanceSettingsPage" => "MCOCSrv - Instance Settings",
                _ => "MCOCSrv"
            };

            this.Title = newTitle;
        }
    }
}
