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

    public static byte ReadBitsForEmbedding(BitWorker dataBitWorker, byte strength)
    {
        byte bitsToEmbed = 0;
        for (byte bitNum = 0; bitNum < strength; bitNum++)
        {
            if (dataBitWorker.IsAtEnd())
            {
                break;
            }

            if (dataBitWorker.ReadBit())
            {
                bitsToEmbed |= (byte)(1 << (strength - 1 - bitNum));
            }
        }
        return bitsToEmbed;
    }

    public static byte ApplyBitsToSourceByte(byte sourceByte, byte bitsToEmbed, byte bitMask)
    {
        return (byte)((sourceByte & ~bitMask) | (bitsToEmbed & bitMask));
    }

    public static void ExtractAndWriteBits(byte sourceByte, byte bitMask, BitWorker extractedBitsWorker, byte strength)
    {
        byte maskedBits = (byte)(sourceByte & bitMask);
        extractedBitsWorker.WriteBitsFromByte(maskedBits, strength);
    }
}