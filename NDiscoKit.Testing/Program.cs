using CSnakes.Runtime;
using NDiscoKit.Audio;
using NDiscoKit.Audio.AudioSources;
using NDiscoKit.Python;

namespace NDiscoKit.Testing;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Initializing Python...");
        IPythonEnvironment python = await NDKPython.InitializeAsync(pipInstall: false);

        CancellationTokenSource cancelRecord = new();

        Console.WriteLine("Starting capture...");
        Task captureTask = new(async () => await StartCapture(python, cancelRecord.Token), TaskCreationOptions.LongRunning);
        captureTask.Start();

        if (Console.ReadKey().Key == ConsoleKey.Enter)
            cancelRecord.Cancel();

        await captureTask;
    }

    private static async Task StartCapture(IPythonEnvironment python, CancellationToken cancelRecord)
    {
        await using AppAudioRecorder recorder = await AppAudioRecorder.StartRecordAsync(AudioSourceProcess.Spotify);

        using AudioProcessor processor = AudioProcessor.CreateBeats(fps: 100, inputFormat: AppAudioRecorder.RecordFormat, beatTracking: python.BeatTracking());

        recorder.DataAvailable += (_, data) => processor.Process(data);
        Console.WriteLine("Capture started.");

        try
        {
            await recorder.WaitForRecordEnd().WaitAsync(cancelRecord);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Stopping capture...");
            await recorder.StopRecordAsync();
        }

        Console.WriteLine("Capture stopped.");
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
