using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NDiscoKit.Audio;
using NDiscoKit.Audio.AudioSources;
using NDiscoKit.Models;
using NDiscoKit.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace NDiscoKit.Windows.Services;

[SupportedOSPlatform("windows")]
internal class WindowsAudioRecordingService : AudioRecordingService, IAsyncDisposable
{
    private class RecordData
    {
        public required ProcessWasapiCapture Capture { get; init; }
        public required Process Process { get; init; }
        public required AudioSource Source { get; init; }
    }

    private readonly ILogger<WindowsAudioRecordingService> logger;
    public WindowsAudioRecordingService(ILogger<WindowsAudioRecordingService> logger)
    {
        this.logger = logger;
    }

    private float[]? _convertBuffer;

    private readonly Lock recordLock = new();
    private RecordData? record;

    protected override int SampleRate { get; } = new WaveFormat().SampleRate;

    public override AudioSource? Source => record?.Source;
    public override event EventHandler<AudioSource?>? SourceChanged;

    public override event EventHandler<ReadOnlyMemory<float>>? DataAvailable;

    public override ValueTask StartRecordAsync(AudioSource source) => new(Task.Run(() => StartRecordInternal(source)));
    private async Task StartRecordInternal(AudioSource source) // Task so that Task.Run can wrap it correctly
    {
        StopRecord();

        if (!TryFindProcess(source, out Process? audioSourceProcess, out AudioSourceProcess? audioSource))
            throw new InvalidOperationException("Process not found for source.");

        RecordData? record = null;
        try
        {
            record = new()
            {
                Capture = await ProcessWasapiCapture.CreateForProcessCaptureAsync(audioSourceProcess.Id, includeProcessTree: audioSource.CaptureEntireProcessTree),
                Process = audioSourceProcess,
                Source = source,
            };

            SubscribeRecord(record);

            record.Capture.StartRecording();

            AttachRecord(record, out RecordData? oldRecord);
            if (oldRecord is not null)
            {
                logger.LogError("Another record was already running. The old recording was overridden.");
                DisposeRecord(record);
            }
        }
        finally
        {
            if (record is null || record != this.record)
            {
                if (record is not null)
                    DisposeRecord(record);
                else
                    audioSourceProcess.Dispose();
            }
        }
    }

    public override ValueTask StopRecordAsync()
    {
        StopRecord();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns the stopped record data if available.
    /// </summary>
    private RecordData? StopRecord()
    {
        DetachRecord(out RecordData? oldRecord);
        if (oldRecord is not null)
            DisposeRecord(oldRecord);
        return oldRecord;
    }

    private void AttachRecord(RecordData? value, out RecordData? oldValue)
    {
        lock (recordLock)
        {
            oldValue = record;
            record = value;
        }

        if (value?.Source != oldValue?.Source)
            SourceChanged?.Invoke(this, value?.Source);
    }

    private void DetachRecord(out RecordData? oldValue)
    {
        lock (recordLock)
        {
            oldValue = record;
            record = null;
        }

        if (oldValue is not null)
            SourceChanged?.Invoke(this, null);
    }

    private void Capture_DataAvailable(object? _, WaveInEventArgs e)
    {
        ReadOnlySpan<byte> source = e.Buffer;

        // Ensure correct convert buffer size
        int convertBufferLength = source.Length / 2;
        if (_convertBuffer?.Length != convertBufferLength)
        {
            logger.LogInformation("Resizing convert buffer...");
            _convertBuffer = new float[convertBufferLength];
        }

        Span<float> convert = _convertBuffer.AsSpan();

        // Convert from 16 bit stereo to 32 bit mono
        int length = e.BytesRecorded;
        Debug.Assert(length % 2 == 0); // length is even (left and right channels are symmetrical in length)
        int sourceIndex = 0;
        int bufferIndex = 0;
        while (sourceIndex < length)
        {
            short left = source[sourceIndex++];
            short right = source[sourceIndex++];

            _convertBuffer[bufferIndex++] = (left + right) / (float)(2 * short.MaxValue);
        }

        // Invoke update
        DataAvailable?.Invoke(this, _convertBuffer.AsMemory(0, bufferIndex));
    }

    private void Capture_RecordingStopped(object? sender, StoppedEventArgs? e)
    {
        try
        {
            ProcessWasapiCapture? capture = sender as ProcessWasapiCapture;

            RecordData? record = StopRecord();
            if (capture is null || capture != record?.Capture)
            {
                logger.LogError("An unknown capture has stopped resulting in a full recording stop.");
                if (capture is not null)
                    DisposeCapture(capture);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not dispose recorder.");
        }
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        try
        {
            Process? p = sender as Process;

            RecordData? record = StopRecord();
            if (p is null || p.Id != record?.Process.Id)
            {
                logger.LogError("An unknown process has exited resulting in a full recording stop.");
                if (p is not null)
                    DisposeProcess(p);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to detach exited process.");
        }
    }

    private void SubscribeRecord(RecordData record)
    {
        ArgumentNullException.ThrowIfNull(record);

        record.Process.EnableRaisingEvents = true;
        record.Process.Exited += Process_Exited;

        record.Capture.DataAvailable += Capture_DataAvailable;
        record.Capture.RecordingStopped += Capture_RecordingStopped;
    }

    private void DisposeRecord(RecordData record)
    {
        ArgumentNullException.ThrowIfNull(record);

        DisposeCapture(record.Capture);
        DisposeProcess(record.Process);
    }

    private void DisposeCapture(ProcessWasapiCapture capture)
    {
        capture.DataAvailable -= Capture_DataAvailable;
        capture.RecordingStopped -= Capture_RecordingStopped;
        capture.Dispose();
    }

    private void DisposeProcess(Process process)
    {
        process.Exited -= Process_Exited;
        process.Dispose();
    }

    public override bool TryFindProcess(AudioSource source, [MaybeNullWhen(false)] out Process process) => TryFindProcess(source, out process, out _);

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
