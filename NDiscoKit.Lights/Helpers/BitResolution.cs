namespace NDiscoKit.Lights.Helpers;
public static class BitResolution
{
    private static class ConversionConstants
    {
        public static readonly double DoubleToUInt8 = GetDoubleConversion(byte.MaxValue);
        public static readonly double DoubleToUInt16 = GetDoubleConversion(ushort.MaxValue);
        public static readonly double DoubleToUInt32 = GetDoubleConversion(uint.MaxValue);

        private static double GetDoubleConversion(ulong maxValue)
        {
            ulong incremented;
            checked
            {
                incremented = maxValue + 1;
            }

            double incrementedDouble = incremented;
            return Math.BitDecrement(incrementedDouble);
        }
    }

    /// <summary>
    /// Convert value from 0-1 to 0-255.
    /// </summary>
    /// <remarks>
    /// If <paramref name="value"/> is out of range, it will be clamped.
    /// </remarks>
    public static byte AsUInt8(double value) => (byte)(Math.Clamp(value, 0d, 1d) * ConversionConstants.DoubleToUInt8);

    /// <summary>
    /// Convert value from 0-1 to 0-65535
    /// </summary>
    /// <remarks>
    /// If <paramref name="value"/> is out of range, it will be clamped.
    /// </remarks>
    public static ushort AsUInt16(double value) => (ushort)(Math.Clamp(value, 0d, 1d) * ConversionConstants.DoubleToUInt16);

    /// <summary>
    /// Convert value from 0-1 to 0-4294967295
    /// </summary>
    /// <remarks>
    /// If <paramref name="value"/> is out of range, it will be clamped.
    /// </remarks>
    public static uint AsUInt32(double value) => (uint)(Math.Clamp(value, 0d, 1d) * ConversionConstants.DoubleToUInt32);
}
