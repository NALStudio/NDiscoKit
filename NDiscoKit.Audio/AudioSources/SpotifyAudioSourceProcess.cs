using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Audio.AudioSources;
public class SpotifyAudioSourceProcess : AudioSourceProcess
{
    public override bool CaptureEntireProcessTree => true;

    public override bool TryFindProcess([MaybeNullWhen(false)] out Process process)
    {
        Process[] spotifyProcesses = Process.GetProcessesByName("spotify");

        process = null;
        try
        {
            // When Spotify is open, it has a window which means that MainWindowTitle is not empty.
            // It is impossible to detect which Spotify process is playing audio at any given time
            // so we just record the Spotify window process and all of its subprocesses together.
            process = spotifyProcesses.FirstOrDefault(static p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
        }
        finally
        {
            DisposeProcesses(spotifyProcesses, except: process);
        }

        return process is not null;
    }
}
