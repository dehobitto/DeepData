namespace DeepData.Utils.StegoSpecific;

public static class QimHelper
{
    public static byte QimEmbedBit(byte orig, bool bit, int delta)
    {
        double quantized = Math.Floor((double)orig / delta) * delta;
        double center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        
        return (byte)Math.Clamp(center, 0, 255);
    }
    
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
    ///     Overload of <see cref="QimEmbedBit"/> for short, preserving its sign.
    /// </summary>
    public static short QimEmbedBit(short coeff, bool bit, int delta)
    {
        int sign = Math.Sign(coeff);
        short absCoeff = Math.Abs(coeff);

        double quantized = Math.Floor((double)absCoeff / delta) * delta;
        double center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        
        short result = (short)(sign * center);

        return result;
    }

    /// <summary>
    ///     Overload of <see cref="QimEmbedBit"/> for short.
    /// </summary>
    public static bool QimExtractBit(short coeff, int delta)
    {
        double absCoeff = Math.Abs(coeff);
        double mod = absCoeff - Math.Floor(absCoeff / delta) * delta;
        bool result = mod >= delta * 0.5;
        
        return result;
    }
}