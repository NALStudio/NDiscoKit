using FftSharp.Windows;
using NDiscoKit.AudioAnalysis.Dsp;
using NDiscoKit.AudioAnalysis.Models;
using System.Collections.Immutable;
using System.Numerics;

namespace NDiscoKit.Services;
internal class AudioFFTService : IDisposable
{
    public readonly ref struct Result
    {
        public required ReadOnlySpan<Complex> FFT { get; init; }
        public required ReadOnlySpan<double> FFTMagnitudes { get; init; }
        public required ReadOnlySpan<double> FFTPowers { get; init; }
    }

    public const int FftSize = 16384; // Must be a power of 2
    private static readonly ImmutableArray<double> _fftWindow = new Blackman().Create(FftSize).ToImmutableArray();

    private readonly AudioRecordingService recording;

    private readonly FixedSizeBuffer<float> _fftSamples;
    private readonly Complex[] _fftBuffer;
    private readonly double[] _magBuffer;
    private readonly double[] _pwrBuffer;

    public event Action<Result>? DataAvailable;

    public AudioFFTService(AudioRecordingService recording)
    {
        this.recording = recording;
        recording.DataAvailable += Recording_DataAvailable;

        _fftSamples = new FixedSizeBuffer<float>(FftSize);
        _fftBuffer = new Complex[FftSize];
        _magBuffer = new double[FFTHelpers.PositiveLength(FftSize)];
        _pwrBuffer = new double[_magBuffer.Length];
    }

    private void Recording_DataAvailable(object? sender, ReadOnlyMemory<float> e)
    {
        ReadOnlySpan<float> data = e.Span;

        // Compute FFT
        _fftSamples.Push(in data);
        RunFFT();
    }

    private void RunFFT()
    {
        ReadOnlySpan<float> fftSamples = _fftSamples.AsSpan();
        Span<Complex> fftBuffer = _fftBuffer.AsSpan();

        ReadOnlySpan<double> fftWindow = _fftWindow.AsSpan();
        for (int i = 0; i < fftBuffer.Length; i++) // Can't use SIMD due to cast to Complex
            fftBuffer[i] = fftSamples[i] * fftWindow[i];

        FftSharp.FFT.Forward(fftBuffer);

        Span<double> magBuffer = _magBuffer.AsSpan();
        Span<double> pwrBuffer = _pwrBuffer.AsSpan();

        FFTHelpers.Magnitude(in magBuffer, fftBuffer);
        FFTHelpers.MagnitudeToPower(in pwrBuffer, magBuffer);
    }

    public void Dispose()
    {
        recording.DataAvailable -= Recording_DataAvailable;
    }
}
