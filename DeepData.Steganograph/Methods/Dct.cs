using BitMiracle.LibJpeg.Classic;
using DeepData.Abstractions;
using DeepData.Extensions;
using DeepData.Settings;
using DeepData.Utils;
using DeepData.Utils.StegoSpecific;
using static DeepData.Utils.StegoSpecific.QimHelper;

namespace DeepData.Methods;

public class Dct(Options options) : StegoMethod<Stream, byte[]>(options)
{
    private int _processed;
    
    public override Stream Embed(Stream source, byte[] data)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;
        
        var bitWorker = BitWorker.CreateWithHeaderFromBytes(data);

        long requiredTotalBits = (long)data.Length * 8 + StegoConstants.HeaderBits;
        long availableCapacityBits = (long)GetCapacityBytes(source) * 8;

        if (availableCapacityBits < requiredTotalBits)
        {
            throw new ArgumentException("The source image does not fit the required data.");
        }

        ProcessComponents(decompressionResult, coefficientArrays, (blocks, componentInfo) =>
        {
            EmbedDataIntoBlocks(blocks, componentInfo, bitWorker);
        },
        compIndex =>
        {
            return compIndex switch
            {
                0 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Y),
                1 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cb),
                2 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cr),
                _ => true
            };
        });

        return JpegHelper.Compress(decompressionResult, coefficientArrays);
    }

    public override byte[] Extract(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;

        long potentialBitsToExtract = (long)GetCapacityBytes(source) * 8;
        
        if (potentialBitsToExtract < StegoConstants.HeaderBits)
        {
            throw new ArgumentException("File is corrupted");
        }

        var bitReader = new BitWorker((int)potentialBitsToExtract);

        ProcessComponents(decompressionResult, coefficientArrays,
        (blocks, componentInfo) =>
        {
            ExtractDataFromBlocks(blocks, componentInfo, bitReader);
        },
        compIndex => compIndex switch
        {
            0 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Y),
            1 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cb),
            2 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cr),
            _ => true
        });

        return bitReader.ReadBytesWithHeader();
    }

    public override bool WillFit(Stream source, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(source);

        long availableBits = (long)GetCapacityBytes(source) * 8;

        long requiredBits = StegoConstants.HeaderBits + (long)payload.Length * 8;

        return availableBits >= requiredBits;
    }

    public override int GetCapacityBytes(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        int capacityCount = 0;

        ProcessComponents(decompressionResult, decompressionResult.CoefArrays, (blocks, componentInfo) =>
        {
            ProcessBlocks(blocks, componentInfo, currentBlock =>
            {
                for (byte i = 1; i < Options.Jpeg.Blocks; i++)
                {
                    short coefficient = currentBlock[i];

                    if (IsCoefficientSkippable(coefficient))
                    {
                        continue;
                    }

                    capacityCount++;
                }

                return false;
            });
        },
        compIndex => compIndex switch
        {
            0 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Y),
            1 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cb),
            2 => !Options.Jpeg.Channels.HasFlag(JpegChannels.Cr),
            _ => true
        });

        source.Seek(0, SeekOrigin.Begin);

        return capacityCount / 8;
    }

    private void ProcessComponents(JpegDecompressResult decompressionResult,
        jvirt_array<JBLOCK>[] coefficientArrays,
        Action<JBLOCK[][], jpeg_component_info> action,
        Func<int, bool> skipCondition)
    {
        var channels = Options.Jpeg.Channels;
        byte componentIndex = 0;
        
        if (channels.HasFlag(JpegChannels.Y) && componentIndex < decompressionResult.NumComponents)
        {
            if (!skipCondition(componentIndex))
            {
                var componentInfo = decompressionResult.GetComponentInfo(componentIndex);
                var blocks = JpegHelper.GetBlocks(coefficientArrays[componentIndex], componentInfo);
                
                action(blocks, componentInfo);
            }

            componentIndex++;
        }
        
        if (channels.HasFlag(JpegChannels.Cb) && componentIndex < decompressionResult.NumComponents)
        {
            if (!skipCondition(componentIndex))
            {
                var componentInfo = decompressionResult.GetComponentInfo(componentIndex);
                var blocks = JpegHelper.GetBlocks(coefficientArrays[componentIndex], componentInfo);
                
                action(blocks, componentInfo);
            }

            componentIndex++;
        }
        
        if (channels.HasFlag(JpegChannels.Cr) && componentIndex < decompressionResult.NumComponents)
        {
            if (!skipCondition(componentIndex))
            {
                var componentInfo = decompressionResult.GetComponentInfo(componentIndex);
                var blocks = JpegHelper.GetBlocks(coefficientArrays[componentIndex], componentInfo);
                
                action(blocks, componentInfo);
            }
        }
    }

    private void ProcessBlocks(JBLOCK[][] blocks, 
        jpeg_component_info componentInfo, 
        Func<JBLOCK, bool> blockAction) 
    {
        long total = componentInfo.Height_in_blocks() * componentInfo.Width_in_blocks;
        
        for (int row = 0; row < componentInfo.Height_in_blocks(); row++)
        for (int col = 0; col < componentInfo.Width_in_blocks; col++)
        {
            var currentBlock = blocks[row][col];
            
            Progress?.Update(++_processed, total);
            
            if (blockAction(currentBlock))
            {
                return;
            }
        }
    }
    
    private void ExtractDataFromBlocks(JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, currentBlock =>
        {
            for (byte i = 1; i < Options.Jpeg.Blocks; i++)
            {
                short coefficient = currentBlock[i];

                if (IsCoefficientSkippable(coefficient))
                {
                    continue;
                }

                if (bitWorker.IsAtEnd())
                {
                    return true;
                }

                bool extractedBit = QimExtractBit(coefficient, Options.Jpeg.Delta);
                
                bitWorker.WriteBit(extractedBit);
            }

            return false;
        });
    }
    
    private void EmbedDataIntoBlocks(JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, currentBlock =>
        {
            for (byte i = 1; i < Options.Jpeg.Blocks; i++)
            {
                short coefficient = currentBlock[i];

                if (bitWorker.IsAtEnd())
                {
                    return true;
                }

                bool bit = bitWorker.PeekBit();

                if (IsCoefficientSkippableBitIncluding(coefficient, bit))
                {
                    continue;
                }

                currentBlock[i] = QimEmbedBit(coefficient, bitWorker.ReadBit(), Options.Jpeg.Delta);
            }

            return false;
        });
    }

    private bool IsCoefficientSkippableBitIncluding(short coefficient, bool bit)
    {
        return coefficient == 0 || QimEmbedBit(coefficient, bit, Options.Jpeg.Delta) == 0;
    }

    private bool IsCoefficientSkippable(short coefficient)
    {
        return coefficient == 0;
    }
}