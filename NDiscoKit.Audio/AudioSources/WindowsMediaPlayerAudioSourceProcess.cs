using System.Diagnostics;

namespace NDiscoKit.Audio.AudioSources;
public class WindowsMediaPlayerAudioSourceProcess : AudioSourceProcess
{
    public override bool CaptureEntireProcessTree => false;

    public override ValueTask<Process?> TryFindProcess()
    {
        Process? player = Process.GetProcessesByName("Microsoft.Media.Player.exe").SingleOrDefault();
        return ValueTask.FromResult(player);
    }
}
