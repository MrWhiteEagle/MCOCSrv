using MCOCSrv.Resources.Classes;

namespace MCOCSrv
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            InstanceManager? manager = App.Current?.Handler.GetService<InstanceManager>();
            if (manager != null)
            {
                UILogger.LogUI("[MCOCSrv] Manager initialized, fetching instances...");
                await manager.FetchInstances();
            }
            else
            {
                Toaster.Toastify("Could not initialize instance manager. Try again, then attempt reinstall or report the issue.");
                App.Current?.Quit();
            }
        }


        //ON APP DESTROY, CLEANUP RESOURCES AND CLOSE CONSOLES
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
            window.Destroying += (s, e) =>
            {
                UILogger.LogUI("[MCOCSrv] APP CLOSING - DISPOSING OF REOURCES...");
                Cleanup();
            };
            return window;
        }

        //CLEANUP AFTER LINGERING PROCESSES
        //CLOSE ALL SERVERS AND DISPOSE
        private void Cleanup()
        {
            var manager = App.Current?.Handler.GetService<InstanceManager>();
            foreach (var instance in manager.running)
            {
                if (instance.Console != null && instance.Console.IsRunning)
                {
                    instance.Console.StopServer();
                    instance.Console.Dispose();
                }
            }
        }
    }
}