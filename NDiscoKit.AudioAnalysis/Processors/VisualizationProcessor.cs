namespace NDiscoKit.AudioAnalysis.Processors;

/*
public class VisualizationProcessor : AudioProcessor<double[]>
{
    private const int _kDecibelMin = -90;
    private const int _kDecibelMax = -30;

    public override event EventHandler<double[]>? OnData;

    /// <summary>
    /// Whether the values of <see cref="Output"/> will be clamped between 0 and 1.
    /// </summary>
    public bool Clamped { get; }

    private readonly double[] _output;

    public VisualizationProcessor(int outputSize = 40, bool clamped = true)
    {
        Clamped = clamped;
        _output = new double[outputSize];
    }

    public override void ProcessFFTMagnitude(in ReadOnlySpan<double> data)
    {

    }

    public override void ProcessFFTPower(in ReadOnlySpan<double> data)
    {
        Process(in data, computeDecibel: false);
    }

    private void Process(in ReadOnlySpan<double> data, bool computeDecibel)
    {
        Span<double> output = _output.AsSpan();

        // Reduce to output size
        ReduceResults(in output, SliceResult(in data));

        // Convert magnitude to 0-1 using the decibel scale
        Convert(in output, clamp: Clamped, computeDecibel: computeDecibel);

        OnData?.Invoke(this, _output);
    }

    private static ReadOnlySpan<double> SliceResult(in ReadOnlySpan<double> results)
    {
        // The low frequencies contain a lot of noise and the high frequencies are usually empty.
        // We slice to the middle 90 % instead.

        int rm = results.Length / 20;
        if (rm > 0)
            return results[rm..^rm];
        else
            return results;
    }

    private static void ReduceResults(scoped in Span<double> output, scoped in ReadOnlySpan<double> results)
    {
        static double Reduce(scoped in ReadOnlySpan<double> values)
        {
            // Reduce values by picking computing the average.
            // We cannot select the maximum value as we have large spikes in our data

            Debug.Assert(values.Length > 0);

            double max = double.NegativeInfinity;
            foreach (double v in values)
            {
                if (v > max)
                    max = v;
            }
            return max;
        }

        if (output.Length == 0)
            throw new ArgumentException("Output must not be empty.");
        if (output.Length >= results.Length)
            throw new ArgumentException("Output length must be less than results length", nameof(output));

        for (int i = 0; i < output.Length; i++)
        {
            int start = (int)(i * (long)results.Length / output.Length);
            int end = (int)((i + 1) * (long)results.Length / output.Length);
            output[i] = Reduce(results[start..end]);
        }
    }

    private static void Convert(scoped in Span<double> output, bool clamp, bool computeDecibel)
    {
        for (int i = 0; i < output.Length; i++)
        {
            double magnitudeOrDecibels = output[i];

            double decibels;
            if (computeDecibel)
                decibels = FFTHelpers.ComputeDecibel(magnitudeOrDecibels);
            else
                decibels = magnitudeOrDecibels;

            double value = (decibels - _kDecibelMin) / (_kDecibelMax - _kDecibelMin);
            if (clamp)
                value = Math.Clamp(value, 0d, 1d);

            output[i] = value;
        }
    }
}
*/