using NAudio.Wave;
using NDiscoKit.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Services;

public abstract class AudioRecordingService
{
    protected abstract int SampleRate { get; }

    private WaveFormat? _outputFormat;
    public WaveFormat OutputFormat => _outputFormat ??= WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: SampleRate, channels: 1);

    public virtual AudioSource? Source { get; }
    public abstract event EventHandler<AudioSource?>? SourceChanged;

    /// <summary>
    /// The recorded audio data in mono 32 bit float values.
    /// </summary>
    public abstract event EventHandler<ReadOnlyMemory<float>>? DataAvailable;

    /// <summary>
    /// <para>Start recording on the specified audio source.</para>
    /// <para>This function can be called again to switch the audio source without stopping first.</para>
    /// </summary>
    public abstract ValueTask StartRecordAsync(AudioSource source);

    /// <summary>
    /// <para>Stop recording on the current audio source or no-op if not currently recording.</para>
    /// </summary>
    public abstract ValueTask StopRecordAsync();

    public abstract bool TryFindProcess(AudioSource source, [MaybeNullWhen(false)] out Process process);
}