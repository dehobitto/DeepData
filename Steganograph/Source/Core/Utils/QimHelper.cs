namespace DeepData.Stego.Utils;

public static class QimHelper
{
    /// <summary>
    /// BitMethods that are helpful
    /// </summary>

    public static byte QimEmbedBit(byte orig, bool bit, int delta)
    {
        double quantized = Math.Floor((double)orig / delta) * delta;
        double center = quantized + (bit ? delta * 0.75 : delta * 0.25); // emperial selected numbers can be another ones tho 0.6 an 0.4 and etc
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
}