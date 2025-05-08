using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDiscoKit.Services;
using NDiscoKit.Windows.Forms;
using NDiscoKit.Windows.Services;
using System.Net.Http.Headers;

namespace NDiscoKit.Windows;

internal static class Program
{
    public static HttpClient HttpClient { get; } = CreateHttp();

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ServiceProvider services = BuildServices();

        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainPage(services));
        }
        finally
        {
            // Dispose synchronously as I can't make the main function as async Task under STAThread
            services.DisposeAsync().Preserve().GetAwaiter().GetResult();
        }
    }

    private static HttpClient CreateHttp()
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                ProductHeaderValue.Parse(Constants.UserAgent)
            )
        );

        return http;
    }

    private static ServiceProvider BuildServices()
    {
        ServiceCollection services = new();
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.AddDebug();
#endif
        });

        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton(HttpClient);

        services.AddNDiscoKitServices(
            appDataServiceFactory: static _ => new WindowsAppDataService(),
            audioRecordingServiceFactory: static services => new WindowsAudioRecordingService(services.GetRequiredService<ILogger<WindowsAudioRecordingService>>()));

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();
    }
}