using System.Diagnostics;

namespace NDiscoKit.Audio.AudioSources;
public class SpotifyAudioSourceProcess : AudioSourceProcess
{
    public override bool CaptureEntireProcessTree => true;

    public override ValueTask<Process?> TryFindProcess()
    {
        Process[] spotifyProcesses = Process.GetProcessesByName("spotify");

        // When Spotify is open, it has a window which means that MainWindowTitle is not empty.
        // It is impossible to detect which Spotify process is playing audio at any given time
        // so we just record the Spotify window process and all of its subprocesses together.
        Process? spotify = spotifyProcesses.FirstOrDefault(static p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
        return ValueTask.FromResult(spotify);
    }
}
