using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using NDiscoKit.Audio;
using NDiscoKit.Audio.AudioSources;
using NDiscoKit.Python;

namespace NDiscoKit.Testing;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Initializing Python...");
        IPythonEnvironment python = await NDKPython.InitializeAsync();

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
        IBeatTracking beatTracking = python.BeatTracking();

        using (PyObject beatTracker = beatTracking.CreateLegacyTempoTracker(fps: 100))
        {
            await using AppAudioRecorder recorder = await AppAudioRecorder.StartRecordAsync(AudioSourceProcess.Spotify);

            int sampleRate = AppAudioRecorder.RecordFormat.SampleRate;
            recorder.DataAvailable += (_, data) => Process(beatTracking, beatTracker, data, sampleRate: sampleRate);
            Console.WriteLine("Capture started.");

            await recorder.WaitForRecordEnd().WaitAsync(cancelRecord);
            if (cancelRecord.IsCancellationRequested)
                Console.WriteLine("Stopping capture...");

            await recorder.StopRecordAsync();
        }

        Console.WriteLine("Capture stopped.");
    }

    private static void Process(IBeatTracking beatTracking, PyObject beatTracker, ReadOnlyMemory<byte> data, int sampleRate)
    {
        // If we use Length == 0, it seemed to corrupt the heap... Oops...
        if (data.Length < 1)
            return;

        IPyBuffer buffer = beatTracking.ProcessTracker(beatTracker, data.ToArray(), sampleRate);
        ReadOnlySpan<double> output = buffer.AsDoubleReadOnlySpan();
        foreach (double x in output.ToArray())
            Console.WriteLine(x);
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
