namespace DeepData.Utils.StegoSpecific;

/// <summary>
///     Helps with Quantization Index Modulation (QIM).
///     Use it to embed or extract single bits from numbers.
/// </summary>
public static class QimHelper
{
    /// <summary>
    ///     Embeds a bit into a byte using QIM.
    /// </summary>
    /// <param name="orig">The original byte value.</param>
    /// <param name="bit">The bit (true/false) to embed.</param>
    /// <param name="delta">The QIM delta value.</param>
    /// <returns>The modified byte with the embedded bit.</returns>
    public static byte QimEmbedBit(byte orig, bool bit, int delta)
    {
        var quantized = Math.Floor((double)orig / delta) * delta;
        var center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        return (byte)Math.Clamp(center, 0, 255);
    }

    /// <summary>
    ///     Extracts a bit from a byte using QIM.
    /// </summary>
    /// <param name="val">The byte value containing the embedded bit.</param>
    /// <param name="delta">The QIM delta value used during embedding.</param>
    /// <returns>The extracted bit (true/false).</returns>
    public static bool QimExtractBit(byte val, int delta)
    {
        return val % delta >= delta / 2;
    }

    /// <summary>
    ///     Determines a bit's value based on votes from multiple channels.
    /// </summary>
    /// <param name="activeChannels">The total number of channels used for voting.</param>
    /// <param name="bitCount">The count of 'true' bits (votes).</param>
    /// <returns>True if more than half of the active channels voted true, otherwise false.</returns>
    public static bool Vote(int activeChannels, int bitCount)
    {
        return activeChannels > 0 && bitCount * 2 > activeChannels;
    }

    /// <summary>
    ///     Embeds a bit into a short (like a DCT coefficient) using QIM, preserving its sign.
    /// </summary>
    /// <param name="coeff">The original short coefficient.</param>
    /// <param name="bit">The bit (true/false) to embed.</param>
    /// <param name="delta">The QIM delta value.</param>
    /// <returns>The modified short coefficient with the embedded bit.</returns>
    public static short QimEmbedBit(short coeff, bool bit, int delta)
    {
        // For negative coefficients, we need to handle the modulo differently
        var sign = Math.Sign(coeff);
        var absCoeff = Math.Abs(coeff);

        var quantized = Math.Floor((double)absCoeff / delta) * delta;
        var center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        
        var result = (short)(sign * center);

        return result;
    }

    /// <summary>
    ///     Extracts a bit from a short (like a DCT coefficient) using QIM.
    /// </summary>
    /// <param name="coeff">The short coefficient containing the embedded bit.</param>
    /// <param name="delta">The QIM delta value used during embedding.</param>
    /// <returns>The extracted bit (true/false).</returns>
    public static bool QimExtractBit(short coeff, int delta)
    {
        double absCoeff = Math.Abs(coeff);
        var mod = absCoeff - Math.Floor(absCoeff / delta) * delta;
        var result = mod >= delta * 0.5;
        return result;
    }
}