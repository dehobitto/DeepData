namespace DeepData.Core.Utils.StegoSpecific;

public static class QimHelper
{
    /// <summary>
    /// BitMethods that are helpful
    /// </summary>

    public static byte QimEmbedBit(byte orig, bool bit, int delta)
    {
        double quantized = Math.Floor((double)orig / delta) * delta;
        double center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        return (byte)Math.Clamp(center, 0, 255);
    }

    public static bool QimExtractBit(byte val, int delta)
    {
        return (val % delta) >= (delta * 0.5);
    }
    
    public static bool Vote(int activeChannels, int bitCount)
    {
        return activeChannels > 0 && bitCount * 2 > activeChannels;
    }

    public static short QimEmbedBit(short coeff, bool bit, int delta)
    {
        // For negative coefficients, we need to handle the modulo differently
        int sign = Math.Sign(coeff);
        short absCoeff = (short)Math.Abs(coeff);
        
        double quantized = Math.Floor((double)absCoeff / delta) * delta;
        double center = quantized + (bit ? delta * 0.75 : delta * 0.25);
        
        // Preserve sign of original coefficient
        short result = (short)(sign * center);
        
        return result;
    }
    
    public static bool QimExtractBit(short coeff, int delta)
    {
        double absCoeff = Math.Abs(coeff);
        double mod = absCoeff - Math.Floor(absCoeff / delta) * delta;
        bool result = mod >= delta * 0.5;
        return result;
    }

}