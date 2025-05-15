using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NDiscoKit.AudioAnalysis.Dsp;
internal static class FFTHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PositiveLength(int spectrumLength) => (spectrumLength / 2) + 1;
    public static int PositiveLength(scoped in ReadOnlySpan<Complex> spectrum) => PositiveLength(spectrum.Length);

    /// <summary>
    /// Calculate power spectrum density (PSD) in RMS units.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <paramref name="positiveOnly"/> is <see langword="true"/>, <paramref name="magnitudes"/> length must equal the value provided by <see cref="PositiveLength"/>.
    /// </para>
    /// <para>
    /// Otherwise <paramref name="magnitudes"/> length must be equal to <paramref name="spectrum"/> length.
    /// </para>
    /// </remarks>
    public static void Magnitude(in Span<double> magnitudes, in ReadOnlySpan<Complex> spectrum, bool positiveOnly = true)
    {
        // Non-allocating version of:
        // https://github.com/swharden/FftSharp/blob/3f5158f7ab146c8fb651028e8dce67407b3ded81/src/FftSharp/FFT.cs#L226

        int length = positiveOnly ? PositiveLength(spectrum.Length) : spectrum.Length;
        if (magnitudes.Length != length)
            throw new ArgumentException("Invalid magnitudes length.", nameof(magnitudes));

        // PositiveLength should always be above 0 since span length cannot be less than 0
        Debug.Assert(magnitudes.Length > 0);

        magnitudes[0] = spectrum[0].Magnitude / spectrum.Length;
        for (int i = 1; i < magnitudes.Length; i++)
            magnitudes[i] = 2 * spectrum[i].Magnitude / spectrum.Length;
    }

    /// <summary>
    /// Calculate power spectrum density (PSD) in dB units.
    /// </summary>
    /// <para>
    /// If <paramref name="positiveOnly"/> is <see langword="true"/>, <paramref name="magnitudes"/> length must equal the value provided by <see cref="PositiveLength"/>.
    /// </para>
    /// <para>
    /// Otherwise <paramref name="magnitudes"/> length must be equal to <paramref name="spectrum"/> length.
    /// </para>
    /// </remarks>
    public static void Power(in Span<double> powers, in ReadOnlySpan<Complex> spectrum, bool positiveOnly = true)
    {
        Magnitude(in powers, in spectrum);

        for (int i = 0; i < powers.Length; i++)
            powers[i] = ComputeDecibel(powers[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ComputeDecibel(double magnitude) => 20 * Math.Log10(magnitude);
}
