using Microsoft.Extensions.Logging;

namespace NDiscoKit.Services;
internal class DiscoService : IDisposable
{
    public class RunningChangedEventArgs : EventArgs
    {
        public RunningChangedEventArgs(bool isRunning, Exception? error)
        {
            IsRunning = isRunning;
            Error = error;
        }

        public bool IsRunning { get; }
        public Exception? Error { get; }
    }

    private readonly ILogger<DiscoService> logger;
    private readonly IPythonService python;
    private readonly DiscoLightService lightService;

    public DiscoService(ILogger<DiscoService> logger, IPythonService python, DiscoLightService lightService)
    {
        this.logger = logger;
        this.python = python;
        this.lightService = lightService;
    }

    private bool __running;
    public bool IsRunning => __running;

    public event EventHandler<RunningChangedEventArgs>? RunningChanged;

    private readonly Lock _backgroundTaskLock = new();
    private Task? _backgroundTask;
    private CancellationTokenSource? _backgroundTaskCancel;

    public void Start()
    {
        lock (_backgroundTaskLock)
        {
            _backgroundTaskCancel?.Cancel();
            _backgroundTaskCancel = new();
            _backgroundTask = BackgroundTaskWrapper(_backgroundTaskCancel);
        }
    }

    public void Stop()
    {
        lock (_backgroundTaskLock)
            _backgroundTaskCancel?.Cancel();
    }

    private void SetRunning(bool value, Exception? error = null)
    {
        bool originalValue = Interlocked.Exchange(ref __running, value);
        if (originalValue != value)
            RunningChanged?.Invoke(this, new RunningChangedEventArgs(isRunning: value, error: error));
    }

    private async Task BackgroundTaskWrapper(CancellationTokenSource backgroundCancelSource)
    {
        SetRunning(true);
        Exception? error = null;
        try
        {
            await BackgroundTask(backgroundCancelSource.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                error = ex;
                logger.LogError(ex, "DiscoService failed with error.");
            }
        }
        SetRunning(false, error);
    }

    private async Task BackgroundTask(CancellationToken cancellationToken)
    {
        await Task.Delay(-1, cancellationToken);
    }

    public void Dispose()
    {
        Stop();
    }
}
