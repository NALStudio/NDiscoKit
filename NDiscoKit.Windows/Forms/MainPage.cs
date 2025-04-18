using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using MudBlazor.Services;

namespace NDiscoKit.Windows.Forms;

public partial class MainPage : Form
{
    public MainPage()
    {
        InitializeComponent();

        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = BuildServices();
        blazorWebView.RootComponents.Add<WinApp>("#app");

        blazorWebView.BlazorWebViewInitializing += WebViewInitializationStarted;
        blazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
    }


    private void WebViewInitializationStarted(object? sender, BlazorWebViewInitializingEventArgs e)
    {
        e.UserDataFolder = Constants.GetAppDataDirectory(Constants.AppDataPaths.WebViewUserDataFolder);
    }

    private void CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess)
            blazorWebView.WebView.CoreWebView2.DocumentTitleChanged += (_, _) => UpdateWindowTitle();
    }

    // Adapted from NLauncher
    private void UpdateWindowTitle()
    {
        Text = blazorWebView.WebView.CoreWebView2.DocumentTitle;
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
        services.AddScoped(_ => Program.HttpClient);
        services.AddMudServices();

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();
    }
}
