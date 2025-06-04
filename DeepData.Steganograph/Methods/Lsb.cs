using DeepData.Abstractions;
using DeepData.Settings;
using DeepData.Utils;
using DeepData.Utils.StegoSpecific;

namespace DeepData.Methods;

using static LsbHelper;

public class Lsb(Options options) : StegoMethod<byte[], byte[]>(options)
{
    public override byte[] Embed(byte[] source, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(data);

        if (!WillFit(source, data))
        {
            throw new ArgumentException($"Data size ({data.Length} bytes) exceeds available capacity ({GetCapacityBytes(source) / 8} bytes) in source");
        }

        var dataBitWorker = BitWorker.CreateWithHeaderFromBytes(data);
        byte[] result = (byte[])source.Clone();

        byte strength = Options.Lsb.Strength;

        byte bitMask = CreateBitMask(strength);
        
        int totalBytes = source.Length;
        int currentByte = 0;

        for (var i = 0; i < source.Length; i++)
        {
            byte bitsToEmbed = ReadBitsForEmbedding(dataBitWorker, strength);
            
            result[i] = ApplyBitsToSourceByte(source[i], bitsToEmbed, bitMask);
            
            Progress?.Update(++currentByte, totalBytes);
        }

        return result;
    }

    public override byte[] Extract(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);

        int totalPossibleEmbeddedBits = source.Length * Options.Lsb.Strength;
        var extractedBitsWorker = new BitWorker(totalPossibleEmbeddedBits);

        if (extractedBitsWorker.ReadUInt32() <= totalPossibleEmbeddedBits)
        {
            throw new ArgumentException("Insufficient data in the source to read the hidden data header.");
        }
        
        byte strength = Options.Lsb.Strength;
        
        byte bitMask = CreateBitMask(strength);
        
        int totalBytes = source.Length;
        int currentByte = 0;

        foreach (byte b in source)
        {
            ExtractAndWriteBits(b, bitMask, extractedBitsWorker, strength);
            
            Progress?.Update(++currentByte, totalBytes);
        }

        extractedBitsWorker.ResetPosition();

        return extractedBitsWorker.ReadBytesWithHeader();
    }

    public override bool WillFit(byte[] source, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(payload);

        int requiredBytes = payload.Length + StegoConstants.HeaderBits / 8;
        int availableBytes = GetCapacityBytes(source);

        return availableBytes >= requiredBytes;
    }

    public override int GetCapacityBytes(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return source.Length * Options.Lsb.Strength / 8;
    }
}