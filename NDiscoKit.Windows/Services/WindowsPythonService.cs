using NDiscoKit.Python;
using NDiscoKit.Services;

namespace NDiscoKit.Windows.Services;
public class WindowsPythonService : IPythonService, IAsyncDisposable
{
    private readonly SettingsService settings;
    public WindowsPythonService(SettingsService settings)
    {
        this.settings = settings;
    }

    private NDKPython? python;
    private Task<NDKPython>? pythonLoadTask;

    /// <summary>
    /// <para>Returns the initialized python instance if available.</para>
    /// <para>Otherwise waits until <see cref="InitializeAsync"/> finishes.</para>
    /// </summary>
    public ValueTask<NDKPython> GetPythonAsync()
    {
        if (python is not null)
            return new(python);

        pythonLoadTask ??= LoadPythonAsync();
        return new(pythonLoadTask);
    }

    private async Task<NDKPython> LoadPythonAsync()
    {
        int? pythonDependencies = (await settings.GetSettingsAsync()).PythonDependenciesVersion;
        string pythonVenv = Constants.GetAppDataDirectory(Constants.AppDataPaths.PythonEnvironmentFolder);
        NDKPython p = await NDKPython.InitializeAsync(pythonDependencies, pythonVenv);

        if (NDKPython.DependenciesVersion != pythonDependencies)
            await settings.UpdateSettingsAsync(s => s with { PythonDependenciesVersion = NDKPython.DependenciesVersion });

        python = p;
        return p;
    }

    public async ValueTask DisposeAsync()
    {
        if (python is not null)
            await python.DisposeAsync();
    }
}
