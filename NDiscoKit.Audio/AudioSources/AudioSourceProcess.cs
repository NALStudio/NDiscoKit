using System.Diagnostics;

namespace NDiscoKit.Audio.AudioSources;
public abstract class AudioSourceProcess
{
    public abstract bool CaptureEntireProcessTree { get; }

    /// <summary>
    /// Tries to find the given process. If process is not found <see langword="null"/> is returned.
    /// </summary>
    public abstract ValueTask<Process?> TryFindProcess();

    public static AudioSourceProcess Spotify { get; } = new SpotifyAudioSourceProcess();
    public static AudioSourceProcess WindowsMediaPlayer { get; } = new WindowsMediaPlayerAudioSourceProcess();
}
