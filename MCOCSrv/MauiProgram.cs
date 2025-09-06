using CommunityToolkit.Maui;
using MCOCSrv.Resources.Classes;
using Microsoft.Extensions.Logging;

namespace MCOCSrv;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        Global.EnsurePathsExist();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldEnableSnackbarOnWindows(true);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
                fonts.AddFont("Roboto-ExtraBold.ttf", "RobotoExtraBold");
                fonts.AddFont("Roboto-Thin.ttf", "RobotoThin");
                fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
                fonts.AddFont("fa-solid-900.ttf", "FA");
                fonts.AddFont("MDI.ttf", "MDI");

            });
        builder.Services.AddSingleton<ServerVersionFetcher>();
        builder.Services.AddSingleton<InstanceManager>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
