using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace NDiscoKit.Python;
public sealed class NDKPython : IDisposable, IAsyncDisposable
{
    public const string PythonVersion = "3.13.3";
    public const int DependenciesVersion = 0;

    private readonly ServiceProvider serviceProvider;
    public IPythonEnvironment Python { get; }

    private NDKPython(ServiceProvider serviceProvider, IPythonEnvironment python)
    {
        this.serviceProvider = serviceProvider;
        Python = python;
    }

    /// <inheritdoc cref="Initialize"/>
    /// <remarks>
    /// This function will initialize outside of the main thread.
    /// </remarks>
    public static Task<NDKPython> InitializeAsync(int? dependenciesVersion = null)
    {
        Task<NDKPython> initialize = new(() => Initialize(dependenciesVersion), TaskCreationOptions.LongRunning);
        initialize.Start();
        return initialize;
    }

    /// <summary>
    /// <para>Initialize the Python environment.</para>
    /// <para>
    /// The <paramref name="dependeciesVersion"/> parameter controls whether the pip dependencies are loaded.
    /// If the value of <paramref name="dependeciesVersion"/> does not match <see cref="DependenciesVersion"/>, pip dependencies will be reinstalled.
    /// </para>
    /// </summary>
    public static NDKPython Initialize(int? dependeciesVersion = null)
    {
        bool pipInstall = dependeciesVersion != DependenciesVersion;

        ServiceCollection services = new();
        services.AddLogging();

        string home = Path.Join(Path.GetDirectoryName(Environment.ProcessPath), "Python");
        string venv = Path.Join(home, ".venv");

        IPythonEnvironmentBuilder python = services.WithPython()
            .WithHome(home)
            .WithVirtualEnvironment(venv)
            .FromNuGet(PythonVersion);

        if (pipInstall)
            python.WithPipInstaller();

        ServiceProvider provider = services.BuildServiceProvider();
        try
        {
            return new NDKPython(provider, python: provider.GetRequiredService<IPythonEnvironment>());
        }
        catch
        {
            provider.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await serviceProvider.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
