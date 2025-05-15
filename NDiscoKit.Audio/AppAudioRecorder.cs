using NAudio.CoreAudioApi;
using NAudio.Wave;
using NDiscoKit.Audio.Internals;
using System.Diagnostics;

namespace NDiscoKit.Audio;

public sealed class AppAudioRecorder : IAsyncDisposable
{
    /// <summary>
    /// The format used to record app audio output.
    /// </summary>
    // ProcessWasapiCapture.WaveFormat is wrong, this is the ACTUAL format used.
    public static WaveFormat RecordFormat { get; } = new();

    public event EventHandler<ReadOnlyMemory<byte>>? DataAvailable;
    public event EventHandler<Exception?>? RecordingStopped;

    public bool IsRecording => !captureEnded.Task.IsCompleted;

    private ProcessWasapiCapture? capture;
    private readonly TaskCompletionSource captureEnded;
    private AppAudioRecorder(ProcessWasapiCapture capture)
    {
        this.capture = capture;
        captureEnded = new TaskCompletionSource();

        capture.DataAvailable += OnDataAvailable;
        capture.RecordingStopped += OnRecordingStopped;
    }

    /// <summary>
    /// <para>This task is very slow to complete (over a second).</para>
    /// <para>This function seems to fail around 30 % of the times, so consider using a try-catch and retry if necessary.</para>
    /// </summary>
    public static async Task<AppAudioRecorder> StartRecordAsync(int processId, bool includeProcessTree = false)
    {
        ProcessWasapiCapture capture = await ProcessWasapiCapture.CreateForProcessCaptureAsync(processId, includeProcessTree);
        AppAudioRecorder recorder = new(capture); // Create recorder before capture start so that we can subscribe to the events on time

        await Task.Delay(500); // Wait 500 ms to let the process capture initialize (otherwise the app crashes...)
        capture.StartRecording();
        await Task.Delay(500); // Wait 500 ms before returning the object to block quick start/stops of the recorder (as this will deadlock the app for some reason)

        return recorder;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs args)
    {
        // No need to check as the user can't register any event handlers
        // before we return the instance from StartRecordAsync
        // if (startFinished)
        DataAvailable?.Invoke(this, args.Buffer.AsMemory(0, args.BytesRecorded));
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs args)
    {
        // args.Exception is actually nullable even though it's not annotated as such
        Exception? err = (Exception?)args.Exception;

        RecordingStopped?.Invoke(this, err);

        if (err is null)
            captureEnded.SetResult();
        else
            captureEnded.SetException(err);
    }

    public Task WaitForRecordEnd() => captureEnded.Task;

    /// <summary>
    /// Even though method is asynchronous, it might block significantly.
    /// </summary>
    public async ValueTask StopRecordAsync()
    {
        if (capture is not null)
        {
            capture.DataAvailable -= OnDataAvailable;
            capture.Dispose();

            await WaitForRecordEnd();
            capture.RecordingStopped -= OnRecordingStopped;
            Debug.Assert(capture.CaptureState == CaptureState.Stopped);

            capture = null;
        }
    }

    /// <summary>
    /// <inheritdoc cref="StopRecord"/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopRecordAsync();
        GC.SuppressFinalize(this);
    }
}