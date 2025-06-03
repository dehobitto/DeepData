using DeepData.Abstractions;
using DeepData.Interfaces;
using DeepData.Models;
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

        var requiredTotalBits = StegoConstants.HeaderBits + data.Length * 8;
        var availableCapacityBits = GetCapacityBytes(source);

        if (availableCapacityBits < requiredTotalBits)
        {
            throw new ArgumentException("Length of source bytes exceeds available capacity");
        }

        var dataBitWorker = BitWorker.CreateWithHeaderFromBytes(data);
        var result = (byte[])source.Clone();

        var bitMask = CreateBitMask(Options.Lsb.Strength);
        var totalBytes = source.Length;
        var currentByte = 0;

        for (var i = 0; i < source.Length; i++)
        {
            byte bitsToEmbed = 0;
            
            for (var bitNum = 0; bitNum < Options.Lsb.Strength; bitNum++)
            {
                if (dataBitWorker.IsAtEnd())
                {
                    break;
                }

                if (dataBitWorker.ReadBit())
                {
                    bitsToEmbed |= (byte)(1 << (Options.Lsb.Strength - 1 - bitNum));
                }
            }

            result[i] = (byte)((source[i] & ~bitMask) | (bitsToEmbed & bitMask));
            Progress?.Update(++currentByte, totalBytes);
        }

        return result;
    }

    public override byte[] Extract(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var totalPossibleEmbeddedBits = source.Length * Options.Lsb.Strength;

        if (totalPossibleEmbeddedBits < StegoConstants.HeaderBits)
        {
            throw new ArgumentException("Insufficient data in the source to read the hidden data header.");
        }

        var extractedBitsWorker = new BitWorker(totalPossibleEmbeddedBits);
        var bitMask = CreateBitMask(Options.Lsb.Strength);
        var totalBytes = source.Length;
        var currentByte = 0;

        foreach (var b in source)
        {
            var maskedBits = (byte)(b & bitMask);
            extractedBitsWorker.WriteBitsFromByte(maskedBits, Options.Lsb.Strength);
            Progress?.Update(++currentByte, totalBytes);
        }

        extractedBitsWorker.ResetPosition();

        return extractedBitsWorker.ReadBytesWithHeader();
    }

    public override bool WillFit(byte[] source, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(payload);

        var requiredBits = payload.Length * 8 + StegoConstants.HeaderBits;
        var availableBits = GetCapacityBytes(source);

        return availableBits >= requiredBits;
    }

    public override int GetCapacityBytes(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Length * Options.Lsb.Strength;
    }
}