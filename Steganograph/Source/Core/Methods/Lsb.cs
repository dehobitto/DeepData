using DeepData.Stego.Interfaces;
using DeepData.Stego.Utils;

namespace DeepData.Stego.Methods;

using static LsbHelper;

public class Lsb(Options options) : IStegoMethod<byte[], byte[]>
{
    public byte[] Embed(byte[] source, byte[] data)
    {
        ValidateLsbStrength(options.LsbStrength);

        byte bitMask = CreateBitMask(options.LsbStrength);
        var bw = BitWorker.FromBytes(data);
        var result = (byte[])source.Clone();

        for (int i = 0; i < source.Length && !bw.IsEnded(); i++)
        {
            byte newBits = bw.ReadBits(options.LsbStrength);
            result[i] = (byte)((source[i] & ~bitMask) | (newBits & bitMask));
        }

        return result;
    }

    public byte[] Extract(byte[] source)
    {
        ValidateLsbStrength(options.LsbStrength);

        byte bitMask = CreateBitMask(options.LsbStrength);
        int totalBits = source.Length * options.LsbStrength;

        var bw = new BitWorker(totalBits);

        foreach (var b in source)
        {
            byte masked = (byte)(b & bitMask);
            bw.WriteBitsFromByte(masked, options.LsbStrength);
        }

        bw.Restart();

        int embeddedDataLength = bw.ReadInt(Constants.HeaderBits);
        if (embeddedDataLength < 0 || embeddedDataLength > source.Length)
        {
            throw new InvalidOperationException("Wrong length");
        }

        return bw.ReadBytes();
    }
}