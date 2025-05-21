using DeepData.Core.Models;
using DeepData.Core.Utils;
using DeepData.Core.Utils.StegoSpecific;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Core.Methods;

using static QimHelper;

public class Qim : StegoMethod<Image<Rgba32>, byte[]>
{
    public override Image<Rgba32> Embed(Image<Rgba32> source, byte[] data)
    {
        if (!WillFit(source, data))
        {
            throw new ArgumentException("The source image does not fit the required data.");
        }
        
        BitWorker bw = BitWorker.CreateWithHeaderFromBytes(data);

        var result = source.Clone();
        var delta = Options.QimDelta;
        var channels = Options.Channels;

        for (int y = 0; y < result.Height && !bw.IsAtEnd(); y++)
        for (int x = 0; x < result.Width && !bw.IsAtEnd(); x++)
        {
            var px = result[x, y];

            bool bit = bw.ReadBit();

            if (channels.R == 1)
            {
                px.R = QimEmbedBit(px.R, bit, delta);
            }

            if (channels.G == 1)
            {
                px.G = QimEmbedBit(px.G, bit, delta);
            }

            if (channels.B == 1)
            {
                px.B = QimEmbedBit(px.B, bit, delta);
            }

            result[x, y] = px;
        }

        return result;
    }

    public override byte[] Extract(Image<Rgba32> source)
    {
        int totalBits = source.Width * source.Height;
    
        BitWorker bw = new BitWorker(totalBits);

        int delta = Options.QimDelta;

        for (int y = 0; y < source.Height; y++)
        for (int x = 0; x < source.Width; x++)
        {
            var pixel = source[x, y];

            var channelValues = new[]
            {
                (Options.Channels.R == 1, pixel.R),
                (Options.Channels.G == 1, pixel.G),
                (Options.Channels.B == 1, pixel.B)
            };

            int bitCount = 0;
            int activeChannels = 0;

            foreach (var (enabled, value) in channelValues)
            {
                if (!enabled)
                {
                    continue;
                }

                activeChannels++;

                if (QimExtractBit(value, delta))
                {
                    bitCount++;
                }
            }

            bool finalBit = Vote(activeChannels, bitCount);
            bw.WriteBit(finalBit);
        }

        return bw.ReadBytesWithHeader();
    }

    public override bool WillFit(Image<Rgba32> source, byte[] payload)
    {
        int maxBytes = (source.Width * source.Height - Constants.HeaderBits) / 8;
        
        return maxBytes >= payload.Length;
    }

    public override int GetCapacity(Image<Rgba32> source)
    {
        return source.Width * source.Height * (Options.Channels.R + Options.Channels.G + Options.Channels.B);
    }
}