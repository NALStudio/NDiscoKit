using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Audio.AudioSources;
public abstract class AudioSourceProcess
{
    public abstract bool CaptureEntireProcessTree { get; }

    /// <summary>
    /// Tries to find the given process. If process is not found <see langword="null"/> is returned.
    /// </summary>
    public abstract bool TryFindProcess([MaybeNullWhen(false)] out Process process);

    public Process FindProcess()
    {
        if (TryFindProcess(out Process? p))
            return p;
        else
            throw new InvalidOperationException("Process not found.");
    }

    public static AudioSourceProcess Spotify { get; } = new SpotifyAudioSourceProcess();
    public static AudioSourceProcess WindowsMediaPlayer { get; } = new WindowsMediaPlayerAudioSourceProcess();

    protected static void DisposeProcesses(Process[] processes, Process? except = null)
    {
        foreach (Process p in processes)
        {
            if (p != except)
                p.Dispose();
        }
    }
}
