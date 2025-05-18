namespace DeepData.Stego.Utils;

public static class BitHelpers
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
        double mod = val % delta;
        return mod >= (delta * 0.5);
    }
}