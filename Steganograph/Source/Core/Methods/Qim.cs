using DeepData.Stego.Interfaces;
using DeepData.Stego.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Stego.Methods;

using static QimHelper;

public class Qim(Options options) : IStegoMethod<Image<Rgba32>, byte[]>
{
    public Image<Rgba32> Embed(Image<Rgba32> source, byte[] data)
    {
        BitWorker bw = BitWorker.FromBytes(data);

        var result = source.Clone();
        var delta = options.QimDelta;
        var channels = options.Channels;

        for (int y = 0; y < result.Height && !bw.IsEnded(); y++)
        for (int x = 0; x < result.Width && !bw.IsEnded(); x++)
        {
            var px = result[x, y];

            bool bit = bw.ReadBit();

            if (channels.R == 1) px.R = QimEmbedBit(px.R, bit, delta);
            if (channels.G == 1) px.G = QimEmbedBit(px.G, bit, delta);
            if (channels.B == 1) px.B = QimEmbedBit(px.B, bit, delta);

            result[x, y] = px;
        }

        return result;
    }

    public byte[] Extract(Image<Rgba32> source)
    {
        int totalPixels = source.Width * source.Height;
        int channelsCount = options.Channels.R + options.Channels.G + options.Channels.B;
        int totalBits = totalPixels * channelsCount;
    
        BitWorker bw = new BitWorker(totalBits);

        int delta = options.QimDelta;

        for (int y = 0; y < source.Height; y++)
        for (int x = 0; x < source.Width; x++)
        {
            var pixel = source[x, y];

            var channelValues = new[]
            {
                (options.Channels.R == 1, pixel.R),
                (options.Channels.G == 1, pixel.G),
                (options.Channels.B == 1, pixel.B)
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
            bw.Write(finalBit);
        }

        return bw.ReadBytes();
    }

}