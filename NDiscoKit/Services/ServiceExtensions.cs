using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace NDiscoKit.Services;
public static class ServiceExtensions
{
    public static void AddNDiscoKitServices(
        this IServiceCollection services,
        Func<IServiceProvider, IAppDataService> appDataServiceFactory,
        Func<IServiceProvider, AudioRecordingService> audioRecordingServiceFactory,
        Func<IServiceProvider, IPythonService> pythonServiceFactory
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

        services.AddSingleton(audioRecordingServiceFactory);

        services.AddSingleton(pythonServiceFactory);
        services.AddSingleton<AudioTempoService>();

        services.AddSingleton<DiscoService>();
        services.AddSingleton<DiscoLightService>();
    }
}
