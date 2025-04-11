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
        Task captureTask = StartCapture(python, cancelRecord.Token);

        Task<ConsoleKey> keyTask;
        while (true)
        {
            keyTask = Task.Run(() => Console.ReadKey().Key);
            await Task.WhenAny(captureTask, keyTask);

            if (captureTask.IsCompleted)
                break;

            if (keyTask.IsCompleted && keyTask.Result == ConsoleKey.Enter)
            {
                cancelRecord.Cancel();
                break;
            }
        }

        await captureTask;
    }

    private static async Task StartCapture(IPythonEnvironment python, CancellationToken cancelRecord)
    {
        await using AppAudioRecorder recorder = await AppAudioRecorder.StartRecordAsync(AudioSourceProcess.Spotify);

        using AudioProcessor processor = AudioProcessor.Create(fps: 100, inputFormat: AppAudioRecorder.RecordFormat, beatTracking: python.BeatTracking());
        SilenceDetector silence = new(TimeSpan.FromSeconds(5), AppAudioRecorder.RecordFormat);

        AudioProcessorResult result = new();
        recorder.DataAvailable += (_, dataMemory) =>
        {
            ReadOnlySpan<byte> data = dataMemory.Span;

            silence.Update(data);

            bool reset = silence.IsSilence;
            processor.Process(data, in result, reset: reset);
            if (reset)
            {
                silence.Reset();
                Console.WriteLine("Reset.");
            }

            Console.WriteLine($"{result.T1?.Value.BPM:0.00} {result.T2?.Value.BPM:0.00}");
        };
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
