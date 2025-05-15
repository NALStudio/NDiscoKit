using FftSharp;
using FftSharp.Windows;
using NDiscoKit.AudioAnalysis.Dsp;
using NDiscoKit.AudioAnalysis.Models;
using System.Diagnostics;
using System.Numerics;

namespace NDiscoKit.AudioAnalysis.Processors;
public class AudioVisualizationProcessor
{
    private readonly FixedSizeBuffer<float> _rotBuffer;
    private readonly Complex[] _fftBuffer;
    private readonly double[] _magnitudeBuffer;
    private readonly double[] _outputBuffer;

    private readonly double[] _signalWindow;

    private const int _kDecibelMin = -90;
    private const int _kDecibelMax = -30;

    public int InputSize => _fftBuffer.Length;
    public int OutputSize => Output.Length;

    /// <summary>
    /// Whether the values of <see cref="Output"/> will be clamped between 0 and 1.
    /// </summary>
    public bool Clamped { get; }

    /// <summary>
    /// Values from 0 to 1 determining the strength of the given frequency range.
    /// </summary>
    public double[] Output { get; }

    public AudioVisualizationProcessor(int inputSize = 16384, int outputSize = 40, bool clamped = true)
    {
        if (!BitOperations.IsPow2(inputSize))
            throw new ArgumentException("Input size must be a power of 2.", nameof(inputSize));

        _rotBuffer = new FixedSizeBuffer<float>(inputSize);
        _fftBuffer = new Complex[inputSize];
        _magnitudeBuffer = new double[FFTHelpers.PositiveLength(_fftBuffer)];
        _outputBuffer = new double[outputSize];

        _signalWindow = new Hanning().Create(inputSize);

        Clamped = clamped;
        Output = new double[outputSize];
    }

    public void Process(in ReadOnlySpan<float> samples)
    {
        // Push data into buffer
        _rotBuffer.Push(in samples);

        // Prepare signal for FFT
        ReadOnlySpan<double> signalWindow = _signalWindow;
        ReadOnlySpan<float> rotBuffer = _rotBuffer.AsSpan();
        Span<Complex> fftBuffer = _fftBuffer.AsSpan();
        for (int i = 0; i < fftBuffer.Length; i++) // Can't use SIMD due to cast to Complex
            fftBuffer[i] = rotBuffer[i] * signalWindow[i];

        // Run FFT
        FFT.Forward(fftBuffer);

        // Compute magnitude
        FFTHelpers.Magnitude(_magnitudeBuffer, fftBuffer);

        // Reduce to output size
        Span<double> outputBuffer = _outputBuffer.AsSpan();
        ReduceResults(in outputBuffer, _magnitudeBuffer);

        // Convert magnitude to 0-1 using the decibel scale
        Convert(in outputBuffer, Clamped);

        outputBuffer.CopyTo(Output);
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

    private static void Convert(scoped in Span<double> output, bool clamp)
    {
        for (int i = 0; i < output.Length; i++)
        {
            double magnitude = output[i];
            double decibels = FFTHelpers.ComputeDecibel(magnitude);
            double value = (decibels - _kDecibelMin) / (_kDecibelMax - _kDecibelMin);
            if (clamp)
                value = Math.Clamp(value, 0d, 1d);
            output[i] = value;
        }
    }
}
