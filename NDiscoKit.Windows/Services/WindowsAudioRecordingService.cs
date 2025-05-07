using Microsoft.Extensions.Logging;
using NDiscoKit.Audio;
using NDiscoKit.Audio.AudioSources;
using NDiscoKit.Models;
using NDiscoKit.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace NDiscoKit.Windows.Services;

[SupportedOSPlatform("windows")]
internal class WindowsAudioRecordingService : IAudioRecordingService, IAsyncDisposable
{
    private class RecorderData
    {
        public required AppAudioRecorder Recorder { get; init; }
        public required Process Process { get; init; }
        public required AudioSource Source { get; init; }
    }

    private readonly ILogger<WindowsAudioRecordingService> logger;
    public WindowsAudioRecordingService(ILogger<WindowsAudioRecordingService> logger)
    {
        this.logger = logger;
    }

    private readonly Lock recorderLock = new();
    private RecorderData? recorder;

    public AudioSource? Source => recorder?.Source;
    public event EventHandler<AudioSource?>? SourceChanged;

    public event EventHandler<ReadOnlyMemory<byte>>? DataAvailable;

    public bool AudioSourceSupported(AudioSource source) => GetSourceProcess(source) is not null;

    // Why the fuck does this work when I run it on another thread????
    public ValueTask StartRecordAsync(AudioSource source) => new(Task.Run(() => StartRecordInternal(source)));
    private async Task StartRecordInternal(AudioSource source) // Task so that Task.Run can wrap it correctly
    {
        if (!TryFindProcess(source, out Process? audioSourceProcess, out AudioSourceProcess? audioSource))
            throw new InvalidOperationException("Process not found for source.");

        lock (recorderLock)
        {
            if (this.recorder is not null
                && this.recorder.Source == source
                && this.recorder.Process.Id == audioSourceProcess.Id) // Check id since the process object instances are unique
            {
                // The user activated record on the same process,
                // return early to avoid COMExceptions
                return;
            }
        }

        RecorderData? recorder = null;
        bool recorderAttached = false;
        try
        {
            int recorderTries = 0;
            const int maxRecorderTries = 3;
            while (recorder is null)
            {
                if (recorderTries > 0)
                    await Task.Delay(recorderTries * 500);

                try
                {
                    recorder = new RecorderData()
                    {
                        Recorder = await AppAudioRecorder.StartRecordAsync(audioSourceProcess.Id, includeProcessTree: audioSource.CaptureEntireProcessTree),
                        Process = audioSourceProcess,
                        Source = source
                    };

                    // Verify that the recorder actually started recording and didn't end with an error.
                    try
                    {
                        await recorder.Recorder.WaitForRecordEnd().WaitAsync(TimeSpan.FromSeconds(1));
                    }
                    catch (TimeoutException)
                    {
                    }
                }
                catch when (recorderTries < maxRecorderTries)
                {
                    recorderTries++;
                }
            }

            recorder.Process.EnableRaisingEvents = true;
            recorder.Process.Exited += Process_Exited;

            recorder.Recorder.DataAvailable += Recorder_DataAvailable;
            recorder.Recorder.RecordingStopped += Recorder_RecordingStopped;

            // Final check in case the recording crashed before event subscription
            if (recorder.Recorder.IsRecording)
            {
                await AttachRecorderAsync(recorder);
                recorderAttached = true;
            }
        }
        finally
        {
            if (recorder is not null)
            {
                if (!recorderAttached)
                    await DisposeRecorderAsync(recorder);
            }
            else
            {
                audioSourceProcess?.Dispose();
            }
        }
    }

    private async void Process_Exited(object? sender, EventArgs e)
    {
        try
        {
            Process? p = sender as Process;
            AppAudioRecorder? recorder = null;
            lock (recorderLock)
            {
                if (p is not null && p == this.recorder?.Process)
                    recorder = this.recorder.Recorder;
            }

            if (recorder is not null)
                await DetachRecorderAsync(recorder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to detach exited process.");
        }
    }

    public ValueTask StopRecordAsync() => new(Task.Run(StopRecordInternal));
    private async Task StopRecordInternal()
    {
        RecorderData? recorder = this.recorder;
        if (recorder is not null)
            await DetachRecorderAsync(recorder.Recorder);
    }

    private void Recorder_DataAvailable(object? _, ReadOnlyMemory<byte> e)
    {
        DataAvailable?.Invoke(this, e);
    }

    private async void Recorder_RecordingStopped(object? sender, Exception? e)
    {
        try
        {
            AppAudioRecorder? recorder = sender as AppAudioRecorder;
            if (recorder is not null)
                await DetachRecorderAsync(recorder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not dispose recorder.");
        }
    }

    private async ValueTask AttachRecorderAsync(RecorderData recorder)
    {
        RecorderData? oldRecorder = null;
        try
        {
            lock (recorderLock)
            {
                oldRecorder = this.recorder;
                this.recorder = recorder;
                SourceChanged?.Invoke(this, Source);
            }
        }
        finally
        {
            await DisposeRecorderAsync(oldRecorder);
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the recorder was detached, <see langword="false"/> if the current recorder didn't match the one provided as <paramref name="recorder"/>.
    /// </summary>
    /// <remarks>
    /// The <paramref name="recorder"/> will be disposed regardless of detach being successful.
    /// </remarks>
    private async ValueTask<bool> DetachRecorderAsync(AppAudioRecorder recorder, bool dispose = true)
    {
        RecorderData? oldRecorder = null;
        try
        {
            lock (recorderLock)
            {
                if (recorder == this.recorder?.Recorder)
                {
                    oldRecorder = this.recorder;
                    this.recorder = null;
                    SourceChanged?.Invoke(this, Source);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        finally
        {
            if (dispose)
            {
                if (oldRecorder is not null)
                    await DisposeRecorderAsync(oldRecorder);
                else
                    await DisposeAudioRecorderAsync(recorder);
            }
        }
    }

    private async ValueTask DisposeRecorderAsync(RecorderData? recorder)
    {
        if (recorder is null)
            return;

        await DisposeAudioRecorderAsync(recorder.Recorder);

        recorder.Process.Exited -= Process_Exited;
        recorder.Process.Dispose();
    }

    private async ValueTask DisposeAudioRecorderAsync(AppAudioRecorder recorder)
    {
        recorder.DataAvailable -= Recorder_DataAvailable;
        recorder.RecordingStopped -= Recorder_RecordingStopped;
        await recorder.DisposeAsync();
    }

    public bool TryFindProcess(AudioSource source, [MaybeNullWhen(false)] out Process process) => TryFindProcess(source, out process, out _);

    private static bool TryFindProcess(AudioSource source, [MaybeNullWhen(false)] out Process process, [MaybeNullWhen(false)] out AudioSourceProcess audioSource)
    {
        audioSource = GetSourceProcess(source);
        if (audioSource is null)
        {
            process = default;
            return false;
        }

        return audioSource.TryFindProcess(out process);
    }

    private static AudioSourceProcess? GetSourceProcess(AudioSource source)
    {
        return source switch
        {
            AudioSource.Spotify => AudioSourceProcess.Spotify,
            AudioSource.WindowsMediaPlayer => AudioSourceProcess.WindowsMediaPlayer,
            _ => null
        };
    }

    public async ValueTask DisposeAsync()
    {
        await StopRecordAsync();
    }
}
