using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NDiscoKit.Python;
public static class NDKPython
{
    private const string PythonVersion = "3.13";
    private static readonly string PythonVersionNumber = string.Concat(PythonVersion.Split('.').Take(2));

    public static Task<IPythonEnvironment> InitializeAsync(bool pipInstall = true)
    {
        Task<IPythonEnvironment> initialize = new(() => Initialize(pipInstall), TaskCreationOptions.LongRunning);
        initialize.Start();
        return initialize;
    }

    public static IPythonEnvironment Initialize(bool pipInstall = true)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                string home = Path.Join(Path.GetDirectoryName(Environment.ProcessPath), "Python");
                string venv = Path.Join(home, ".venv");
                string appDataPython = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", $"Python{PythonVersionNumber}");

                IPythonEnvironmentBuilder python = services.WithPython()
                    .WithHome(home)
                    .WithVirtualEnvironment(venv)
                    .FromFolder(appDataPython, PythonVersion);

                if (pipInstall)
                    python.WithPipInstaller();
            });

        IHost app = builder.Build();

        return app.Services.GetRequiredService<IPythonEnvironment>();
    }
}
