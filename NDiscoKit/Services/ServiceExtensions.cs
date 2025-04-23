using Microsoft.Extensions.DependencyInjection;

namespace NDiscoKit.Services;
public static class ServiceExtensions
{
    public static void AddNDiscoKitServices(
        this IServiceCollection services,
        IAppDataService appDataService
    )
    {
        services.AddSingleton(appDataService);

        services.AddSingleton<SettingsService>();
        services.AddSingleton<DiscoLightService>();
    }
}
