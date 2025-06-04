using DeepData.Abstractions;
using DeepData.Settings;
using DeepData.Utils;
using DeepData.Utils.StegoSpecific;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Methods;

using static QimHelper;

public class Qim(Options options) : StegoMethod<Image<Rgba32>, byte[]>(options)
{
    public override Image<Rgba32> Embed(Image<Rgba32> source, byte[] data)
    {
        if (!WillFit(source, data))
        {
            throw new ArgumentException("The source image does not fit the required data.");
        }

        var bitWorker = BitWorker.CreateWithHeaderFromBytes(data);

        Image<Rgba32> result = source.Clone();
        
        byte delta = Options.Qim.Delta;
        var channels = Options.Qim.Channels;
        
        int totalPixels = source.Width * source.Height;
        int currentPixel = 0;

        for (var y = 0; y < result.Height && !bitWorker.IsAtEnd(); y++)
        for (var x = 0; x < result.Width && !bitWorker.IsAtEnd(); x++)
        {
            var px = result[x, y];

            var bit = bitWorker.ReadBit();

            if (channels.HasFlag(ImageChannels.R))
            {
                px.R = QimEmbedBit(px.R, bit, delta);
            }

            if (channels.HasFlag(ImageChannels.G))
            {
                px.G = QimEmbedBit(px.G, bit, delta);
            }

            if (channels.HasFlag(ImageChannels.B))
            {
                px.B = QimEmbedBit(px.B, bit, delta);
            }

            result[x, y] = px;
            
            Progress?.Update(++currentPixel, totalPixels);
        }

        return result;
    }

    public override byte[] Extract(Image<Rgba32> source)
    {
        var totalBits = source.Width * source.Height;
        var totalPixels = source.Width * source.Height;
        var currentPixel = 0;

        var bw = new BitWorker(totalBits);

        int delta = Options.Qim.Delta;
        var channels = Options.Qim.Channels;

        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var pixel = source[x, y];

            var channelValues = new[]
            {
                (channels.HasFlag(ImageChannels.R), pixel.R),
                (channels.HasFlag(ImageChannels.G), pixel.G),
                (channels.HasFlag(ImageChannels.B), pixel.B)
            };

            var bitCount = 0;
            var activeChannels = 0;

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

            var finalBit = Vote(activeChannels, bitCount);
            
            bw.WriteBit(finalBit);
            Progress?.Update(++currentPixel, totalPixels);
        }

        return bw.ReadBytesWithHeader();
    }

    public override bool WillFit(Image<Rgba32> source, byte[] payload)
    {
        return GetCapacityBytes(source) >= payload.Length;
    }

    public override int GetCapacityBytes(Image<Rgba32> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Width * source.Height / 8;
    }
}