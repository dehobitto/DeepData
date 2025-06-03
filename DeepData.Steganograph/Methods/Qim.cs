using DeepData.Interfaces;
using DeepData.Models;
using DeepData.Settings;
using DeepData.Utils;
using DeepData.Utils.StegoSpecific;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Methods;

using static QimHelper;

public class Qim(Options options) : StegoMethod<Image<Rgba32>, byte[]>(options)
{
    private IProgress? _progress;

    public void SetProgress(IProgress progress)
    {
        _progress = progress;
    }

    public override Image<Rgba32> Embed(Image<Rgba32> source, byte[] data)
    {
        if (!WillFit(source, data))
        {
            throw new ArgumentException("The source image does not fit the required data.");
        }

        var bw = BitWorker.CreateWithHeaderFromBytes(data);

        var result = source.Clone();
        var delta = Options.Qim.Delta;
        var channels = Options.Qim.Channels;
        var totalPixels = source.Width * source.Height;
        var currentPixel = 0;

        for (var y = 0; y < result.Height && !bw.IsAtEnd(); y++)
        for (var x = 0; x < result.Width && !bw.IsAtEnd(); x++)
        {
            var px = result[x, y];

            var bit = bw.ReadBit();

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
            _progress?.Update(++currentPixel, totalPixels);
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
            _progress?.Update(++currentPixel, totalPixels);
        }

        return bw.ReadBytesWithHeader();
    }

    public override bool WillFit(Image<Rgba32> source, byte[] payload)
    {
        return GetCapacityBytes(source) >= payload.Length;
    }

    public override int GetCapacityBytes(Image<Rgba32> source)
    {
        return source.Width * source.Height / 8;
    }
}