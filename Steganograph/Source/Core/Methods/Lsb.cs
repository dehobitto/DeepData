using DeepData.Core.Models;
using DeepData.Core.Utils;

namespace DeepData.Core.Methods;

using static LsbHelper;

public class Lsb : StegoMethod<byte[], byte[]>
{
    public override byte[] Embed(byte[] source, byte[] data)
    {
        ValidateLsbStrength(Options.LsbStrength);

        if (!WillFit(source, data))
        {
            throw new ArgumentException("The source cannot fit the data.");
        }

        byte bitMask = CreateBitMask(Options.LsbStrength);
        var bw = BitWorker.CreateFromBytes(data);
        var result = (byte[])source.Clone();

        for (int i = 0; i < source.Length && !bw.IsEnded(); i++)
        {
            byte newBits = bw.ReadNBytes(Options.LsbStrength);
            result[i] = (byte)((source[i] & ~bitMask) | (newBits & bitMask));
        }

        return result;
    }

    public override byte[] Extract(byte[] source)
    {
        ValidateLsbStrength(Options.LsbStrength);

        byte bitMask = CreateBitMask(Options.LsbStrength);
        int totalBits = source.Length * Options.LsbStrength;

        var bw = new BitWorker(totalBits);

        foreach (var b in source)
        {
            byte masked = (byte)(b & bitMask);
            bw.WriteNLastBits(masked, Options.LsbStrength);
        }

        bw.Restart();

        int embeddedDataLength = bw.ReadNLastBits(Constants.HeaderBits);
        if (embeddedDataLength < 0 || embeddedDataLength > source.Length)
        {
            throw new InvalidOperationException("Wrong length");
        }

        return bw.ReadBytes(totalBits);
    }

    public override bool WillFit(byte[] source, byte[] payload)
    {
        return source.Length - Constants.HeaderBytes >= payload.Length;
    }
}