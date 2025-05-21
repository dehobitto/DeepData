using BitMiracle.LibJpeg.Classic;
using DeepData.Core.Extensions;
using DeepData.Core.Models;
using DeepData.Core.Utils;

namespace DeepData.Core.Methods;

public class DctJpeg : StegoMethod<Stream, byte[]>
{
    public override Stream Embed(Stream source, byte[] data)
    {
        var decompressResult = JpegHelper.Decompress(source);
        
        var coefArrays = decompressResult.CoefArrays;
        BitWorker bw = BitWorker.CreateFromBytes(data);
        int maxCapacity = CalculateCapacity(decompressResult);

        if (maxCapacity < data.Length * 8)
        {
            throw new ArgumentException("The data cant fit here.");
        }

        int maxComponents = Math.Min(Constants.JpegChannelsCount, decompressResult.NumComponents);
        
        for (int k = 0; k < maxComponents; k++)
        {
            bool skip = k switch
            {
                0 => Options.ChannelsJpeg.Y == 0,
                1 => Options.ChannelsJpeg.Cb == 0,
                2 => Options.ChannelsJpeg.Cr == 0,
                _ => true
            };

            if (skip)
            {
                continue;
            }
            
            var compInfo = decompressResult.GetComponentInfo(k);
            var blocks = JpegHelper.GetBlocks(coefArrays[k], compInfo);

            EmbedDataInBlocks(blocks, compInfo, bw);
        }

        return JpegHelper.Compress(decompressResult, coefArrays);
    }

    private void EmbedDataInBlocks(JBLOCK[][] blocks, jpeg_component_info compInfo, BitWorker bw)
    {
        for (int row = 0; row < compInfo.Height_in_blocks(); row++)
        for (int col = 0; col < compInfo.Width_in_blocks; col++)
        {
            JBLOCK block = blocks[row][col];
            
            for (int i = 1; i < Constants.JpegBlocksCount; i++)
            {
                short coeff = block[i];

                if (coeff == 0)
                {
                    continue;
                }

                if (bw.IsEnded())
                {
                    return;
                }
                
                block[i] = QimHelper.QimEmbedBit(coeff, bw.ReadBit(), Options.QimDelta);
            }
        }
    }
    
    public override byte[] Extract(Stream source)
    {
        var decompressResult = JpegHelper.Decompress(source);
        var coefArrays = decompressResult.CoefArrays;
        int totalBits = CalculateCapacity(decompressResult);
        
        var bw = new BitWorker(totalBits);

        int maxComponents = Math.Min(Constants.JpegChannelsCount, decompressResult.NumComponents);
        
        for (int k = 0; k < maxComponents; k++)
        {
            bool skip = k switch
            {
                0 => Options.ChannelsJpeg.Y == 0,
                1 => Options.ChannelsJpeg.Cb == 0,
                2 => Options.ChannelsJpeg.Cr == 0,
                _ => true
            };

            if (skip)
            {
                continue;
            }
            
            var compInfo = decompressResult.GetComponentInfo(k);
            var blocks = JpegHelper.GetBlocks(coefArrays[k], compInfo);

            ExtractDataInBlocks(blocks, compInfo, bw);
        }
        
        if (totalBits < Constants.HeaderBits)
            throw new InvalidOperationException("Insufficient data for header");
        
        return bw.ReadBytes(totalBits / 8);
    }

    public override bool WillFit(Stream source, byte[] payload)
    {
        var decompress = JpegHelper.Decompress(source);
        int availableBits = CalculateCapacity(decompress);
    
        // Требуемые биты = заголовок + данные
        int requiredBits = Constants.HeaderBits + payload.Length * 8;
    
        return availableBits >= requiredBits;
    }

    private int CalculateCapacity(JpegDecompressResult decompress)
    {
        int capacity = 0;
        int maxComponents = Math.Min(Constants.JpegChannelsCount, decompress.NumComponents);

        for (int k = 0; k < maxComponents; k++)
        {
            bool include = k switch
            {
                0 => Options.ChannelsJpeg.Y == 1,
                1 => Options.ChannelsJpeg.Cb == 1,
                2 => Options.ChannelsJpeg.Cr == 1,
                _ => false
            };

            if (!include)
                continue;

            var compInfo = decompress.GetComponentInfo(k);
            var blocks = JpegHelper.GetBlocks(decompress.CoefArrays[k], compInfo);

            for (int row = 0; row < compInfo.Height_in_blocks(); row++)
            for (int col = 0; col < compInfo.Width_in_blocks; col++)
            {
                var block = blocks[row][col];
                for (int i = 1; i < Constants.JpegBlocksCount; i++) // AC only
                {
                    if (block[i] != 0)
                        capacity++;
                }
            }
        }

        return capacity;
    }



    private void ExtractDataInBlocks(JBLOCK[][] blocks, jpeg_component_info compInfo, BitWorker bw)
    {
        for (int row = 0; row < compInfo.Height_in_blocks(); row++)
        for (int col = 0; col < compInfo.Width_in_blocks; col++)
        {
            JBLOCK block = blocks[row][col];
            for (int i = 1; i < Constants.JpegBlocksCount; i++)
            {
                short coeff = block[i];
                if (coeff == 0) continue;
            
                if (bw.IsEnded()) 
                    return;
                
                // Правильное чтение бита и запись в BitWorker
                bool bit = QimHelper.QimExtractBit(coeff, Options.QimDelta);
                bw.Write(bit);
            }
        }
    }
}