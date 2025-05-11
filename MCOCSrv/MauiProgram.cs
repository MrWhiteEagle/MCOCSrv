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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<ServerVersionFetcher>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
