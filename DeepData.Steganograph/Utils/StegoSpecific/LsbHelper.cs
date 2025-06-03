namespace DeepData.Utils.StegoSpecific;

/// <summary>
///     Helper class for Least Significant Bit (LSB) manipulation.
/// </summary>
public static class LsbHelper
{
    /// <summary>
    ///     Creates bit mask used is LSB methods
    /// </summary>
    /// <param name="bitsPerByte">Amount of bits to mask from the end</param>
    /// <returns></returns>
    public static byte CreateBitMask(byte bitsPerByte)
    {
        return (byte)((1 << bitsPerByte) - 1);
    }
}