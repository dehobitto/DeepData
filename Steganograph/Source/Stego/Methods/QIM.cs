using DeepData.Stego.Interfaces;
using DeepData.Stego.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Stego.Methods;

using static BitHelpers;

public class Qim : IStegoMethod<Image<Rgba32>, byte[]>
{
    public Image<Rgba32> Embed(Image<Rgba32> source, byte[] data, Options options)
    {
        byte delta = options.QimDelta;
        int totalBits = (Constants.HeaderBits + data.Length) * 8;
        BitWorker bw = new BitWorker(totalBits);
        var result = source.Clone();
        
        bw.Write(data);
        
        bw.Restart();
        
        var channelsToUse = options.Channels;
        // Using the channels from options
        for (int y = 0; y < result.Height && !bw.IsEnded(); y++)
        for (int x = 0; x < result.Width && !bw.IsEnded(); x++)
        {
            var px = result[x, y];
            var bit = bw.ReadBit();

            if (channelsToUse.R == 1)
            {
                px.R = QimEmbedBit(px.R, bit, delta); 
            }
            
            if (channelsToUse.G == 1)
            { 
                px.G = QimEmbedBit(px.G, bit, delta); 
            }
            
            if (channelsToUse.B == 1)
            {
                px.B = QimEmbedBit(px.B, bit, delta); 
            }
            
            result[x, y] = px;
        }
        
        return result;
    }

    public byte[] Extract(Image<Rgba32> source, Options options)
    {
        int delta = options.QimDelta;
        var channels = options.Channels;

        int totalPixels = source.Width * source.Height;
        BitWorker bw = new BitWorker(totalPixels);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source[x, y];
                int bitSum = 0;
                int activeChannels = 0;

                if (channels.R == 1)
                {
                    bitSum += QimExtractBit(pixel.R, delta) ? 1 : 0;
                    activeChannels++; 
                }
                
                if (channels.G == 1)
                {
                    bitSum += QimExtractBit(pixel.G, delta) ? 1 : 0;
                    activeChannels++; 
                }
                
                if (channels.B == 1)
                {
                    bitSum += QimExtractBit(pixel.B, delta) ? 1 : 0;
                    activeChannels++; 
                }

                bool finalBit = activeChannels > 0 && (bitSum > (activeChannels / 2.0));
                bw.Write(finalBit);
            }
        }
        
        bw.Restart();

        return bw.ReadBytes();
    }
}