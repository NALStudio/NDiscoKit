using NDiscoKit.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Services;

public interface IAudioRecordingService
{
    AudioSource? Source { get; }
    event EventHandler<AudioSource?>? SourceChanged;

    /// <summary>
    /// The recorded audio data in stereo 16 bit PCM.
    /// </summary>
    event EventHandler<ReadOnlyMemory<byte>>? DataAvailable;

    /// <summary>
    /// Returns <see langword="true"/> if the given audio source <paramref name="source"/> is supported, <see langword="false"/> otherwise.
    /// </summary>
    bool AudioSourceSupported(AudioSource source);

    /// <summary>
    /// <para>Start recording on the specified audio source.</para>
    /// <para>This function can be called again to switch the audio source without stopping first.</para>
    /// </summary>
    ValueTask StartRecordAsync(AudioSource source);

    /// <summary>
    /// <para>Stop recording on the current audio source or no-op if not currently recording.</para>
    /// </summary>
    ValueTask StopRecordAsync();

    bool TryFindProcess(AudioSource source, [MaybeNullWhen(false)] out Process process);
}