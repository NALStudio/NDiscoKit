using CSnakes.Runtime;
using NDiscoKit.Audio;
using NDiscoKit.Audio.AudioSources;
using NDiscoKit.AudioAnalysis.Models;
using NDiscoKit.AudioAnalysis.Processors;
using System.Diagnostics;

namespace NDiscoKit.AudioAnalysis;

public sealed class AudioAnalyzer : IAsyncDisposable
{
    private readonly AppAudioRecorder recorder;

    private readonly Action<AudioProcessorResult> onDataAvailable;

    private readonly AudioProcessor processor;
    private readonly AudioProcessorResult result;

    private readonly SilenceDetector silence;

    private AudioAnalyzer(IPythonEnvironment python, AppAudioRecorder recorder, Action<AudioProcessorResult> onDataAvailable)
    {
        this.onDataAvailable = onDataAvailable;

        processor = AudioProcessor.Create(fps: 100, inputFormat: AppAudioRecorder.RecordFormat, beatTracking: python.BeatTracking());
        result = new AudioProcessorResult();

        silence = new(TimeSpan.FromSeconds(5), AppAudioRecorder.RecordFormat);

        recorder.DataAvailable += Recorder_DataAvailable;
        this.recorder = recorder;
    }

    public static async Task<AudioAnalyzer> StartCaptureAsync(IPythonEnvironment python, AudioSourceProcess source, Action<AudioProcessorResult> onDataAvailable)
    {
        using Process sourceProcess = source.FindProcess();

        int recorderTries = 0;
        const int maxRecorderTries = 3;
        AppAudioRecorder? recorder = null;
        while (recorder is null)
        {
            if (recorderTries > 0)
                await Task.Delay(recorderTries * 500);

            try
            {
                recorder = await AppAudioRecorder.StartRecordAsync(sourceProcess.Id, includeProcessTree: source.CaptureEntireProcessTree);

                // Verify that the recorder actually started recording and didn't end with an error.
                try
                {
                    await recorder.WaitForRecordEnd().WaitAsync(TimeSpan.FromSeconds(1));
                }
                catch (TimeoutException)
                {
                }
            }
            catch when (recorderTries < maxRecorderTries)
            {
                recorderTries++;
            }
        }

        try
        {
            return new(python, recorder, onDataAvailable);
        }
        catch
        {
            await recorder.DisposeAsync();
            throw;
        }
    }

    public async Task<Exception?> WaitForExit()
    {
        try
        {
            await recorder.WaitForRecordEnd();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private void Recorder_DataAvailable(object? _, ReadOnlyMemory<byte> dataMemory)
    {
        ReadOnlySpan<byte> data = dataMemory.Span;

        silence.Update(data);

        bool reset = silence.IsSilence;
        processor.Process(data, in result, reset: reset);
        if (result.WasReset)
            silence.Reset();

        onDataAvailable.Invoke(result);
    }

    public async ValueTask DisposeAsync()
    {
        recorder.DataAvailable -= Recorder_DataAvailable;
        await recorder.DisposeAsync();

        processor.Dispose();
    }
}
