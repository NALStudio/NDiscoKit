using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;

namespace NDiscoKit.Windows.Forms;

public partial class MainPage : Form
{
    public MainPage(IServiceProvider services)
    {
        InitializeComponent();

        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = services;
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
}
