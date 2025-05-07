using NDiscoKit.Audio.AudioSources;
using NDiscoKit.AudioAnalysis.Models;
using NDiscoKit.Python;

namespace NDiscoKit.Testing;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Initializing Python...");
        NDKPython python = await NDKPython.InitializeAsync(dependenciesVersion: NDKPython.DependenciesVersion); // ignore pip install

        Console.WriteLine("Starting capture...");
        AudioAnalyzer analyzer = await AudioAnalyzer.StartCaptureAsync(python.Python, AudioSourceProcess.Spotify, onDataAvailable: PrintOutput);

        Task<Exception?> captureTask = analyzer.WaitForExit();
        Task<ConsoleKey> keyTask;
        while (true)
        {
            keyTask = Task.Run(() => Console.ReadKey().Key);
            await Task.WhenAny(captureTask, keyTask);

            if (captureTask.IsCompleted)
                break;

            if (keyTask.IsCompleted && keyTask.Result == ConsoleKey.Enter)
            {
                await analyzer.DisposeAsync();
                break;
            }
        }

        Exception? error = await captureTask;
        if (error is not null)
            throw error;
    }

    private static void PrintOutput(AudioProcessorResult result)
    {
        if (result.WasReset)
            Console.WriteLine("RESET");
        Console.WriteLine($"{result.T1?.Value.BPM:0.00} {result.T2?.Value.BPM:0.00}");
    }

    /* Didn't work....
    private static async Task<bool> TestRecordToSeeIfIsSpotifyAudioOutput(int processId)
    {
        ProcessWasapiCapture capture = await ProcessWasapiCapture.CreateForProcessCaptureAsync(processId, false);

        bool bytesRecordedZero = false;

        capture.DataAvailable += (s, a) =>
        {
            if (a.BytesRecorded == 0)
                bytesRecordedZero = true;
        };

        capture.StartRecording();
        await Task.Delay(2000); // Record for 2 seconds
        capture.Dispose(); // Dispose stops the recording

        return !bytesRecordedZero;
    }
    */
}
