using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System.Runtime.Versioning;

namespace NDiscoKit.Services;
public static class ServiceExtensions
{
    [UnsupportedOSPlatform("browser")]
    public static void AddNDiscoKitServices(
        this IServiceCollection services,
        Func<IServiceProvider, IAppDataService> appDataServiceFactory,
        Func<IServiceProvider, IAudioRecordingService> audioRecordingServiceFactory
    )
    {
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.VisibleStateDuration = 10000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
        });

        services.AddSingleton(appDataServiceFactory);

        services.AddSingleton<SettingsService>();

        services.AddSingleton<PythonService>();

        services.AddSingleton(audioRecordingServiceFactory);

        services.AddSingleton<DiscoService>();
        services.AddSingleton<DiscoLightService>();
    }
}
