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
            {
                Process p = mediaPlayers[0];

                bool suspended = false;
                if (p.Threads.Count > 0)
                {
                    ProcessThread t0 = p.Threads[0];
                    if (t0.ThreadState == System.Diagnostics.ThreadState.Wait)
                    {
                        try
                        {
                            suspended = t0.WaitReason == ThreadWaitReason.Suspended;
                        }
                        catch (InvalidOperationException) // Thread stopped waiting before we checked the reason
                        {
                        }
                    }
                }

                if (!suspended)
                    process = p;
            }
        }
        finally
        {
            DisposeProcesses(mediaPlayers, except: process);
        }

        return process is not null;
    }
}
