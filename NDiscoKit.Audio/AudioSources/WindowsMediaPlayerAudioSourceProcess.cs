using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Audio.AudioSources;
public class WindowsMediaPlayerAudioSourceProcess : AudioSourceProcess
{
    public override bool CaptureEntireProcessTree => false;

    public override bool TryFindProcess([MaybeNullWhen(false)] out Process process)
    {
        Process[] mediaPlayers = Process.GetProcessesByName("Microsoft.Media.Player");

        process = null;
        try
        {
            if (mediaPlayers.Length == 1)
                process = mediaPlayers[0];
        }
        finally
        {
            DisposeProcesses(mediaPlayers, except: process);
        }

        return process is not null;
    }
}
