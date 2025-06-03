using BitMiracle.LibJpeg.Classic;
using DeepData.Extensions;
using DeepData.Models;
using DeepData.Settings;
using DeepData.Utils;
using DeepData.Utils.StegoSpecific;
using static DeepData.Utils.StegoSpecific.QimHelper;

namespace DeepData.Methods;

public class Dct(Options options) : StegoMethod<Stream, byte[]>(options)
{
    public override Stream Embed(Stream source, byte[] data)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;
        var bitWriter = BitWorker.CreateWithHeaderFromBytes(data);

        var requiredTotalBits = (long)data.Length * 8 + StegoConstants.HeaderBits;
        var availableCapacityBits = (long)GetCapacityBytes(source) * 8;

        if (availableCapacityBits < requiredTotalBits)
        {
            throw new ArgumentException("The source image does not fit the required data.");
        }

        ProcessComponents(decompressionResult, coefficientArrays,
        (blocks, componentInfo) =>
        {
            EmbedDataIntoBlocks(blocks, componentInfo, bitWriter);
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

    private void EmbedDataIntoBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, currentBlock =>
        {
            for (var i = 1; i < Options.Jpeg.Blocks; i++)
            {
                var coefficient = currentBlock[i];

                if (bitWorker.IsAtEnd())
                {
                    return true;
                }

                var bit = bitWorker.PeekBit();

                if (IsCoefficientSkippableForEmbedding(coefficient, bit))
                {
                    continue;
                }

                currentBlock[i] = QimEmbedBit(coefficient, bitWorker.ReadBit(), Options.Qim.Delta);
            }

            return false;
        });
    }

    public override byte[] Extract(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;

        var potentialBitsToExtract = (long)GetCapacityBytes(source) * 8;
        
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

    public override bool WillFit(
        Stream source,
        byte[] payload)
    {
        var availableBits = (long)GetCapacityBytes(source) * 8;

        var requiredBits = StegoConstants.HeaderBits + (long)payload.Length * 8;

        return availableBits >= requiredBits;
    }

    public override int GetCapacityBytes(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var capacityCount = 0;

        ProcessComponents(decompressionResult, decompressionResult.CoefArrays, 
        (blocks, componentInfo) =>
        {
            ProcessBlocks(blocks, componentInfo, currentBlock =>
            {
                for (var i = 1; i < Options.Jpeg.Blocks; i++)
                {
                    var coefficient = currentBlock[i];

                    if (IsCoefficientSkippableForCapacity(coefficient)) continue;

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

    private void ExtractDataFromBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, currentBlock =>
        {
            for (var i = 1; i < Options.Jpeg.Blocks; i++)
            {
                var coefficient = currentBlock[i];

                if (IsCoefficientSkippableForExtraction(coefficient))
                {
                    continue;
                }

                if (bitWorker.IsAtEnd())
                {
                    return true;
                }

                var extractedBit = QimExtractBit(coefficient, Options.Qim.Delta);
                bitWorker.WriteBit(extractedBit);
            }

            return false;
        });
    }

    private void ProcessComponents(
        JpegDecompressResult decompressionResult,
        jvirt_array<JBLOCK>[] coefficientArrays,
        Action<JBLOCK[][], jpeg_component_info> action,
        Func<int, bool> skipCondition)
    {
        var channels = Options.Jpeg.Channels;
        var componentIndex = 0;
        
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

    private void ProcessBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        Func<JBLOCK, bool> blockAction)
    {
        for (var row = 0; row < componentInfo.Height_in_blocks(); row++)
        for (var col = 0; col < componentInfo.Width_in_blocks; col++)
        {
            var currentBlock = blocks[row][col];
            if (blockAction(currentBlock))
            {
                return;
            }
        }
    }

    private bool IsCoefficientSkippableForEmbedding(short coefficient, bool bit)
    {
        return coefficient == 0 || QimEmbedBit(coefficient, bit, Options.Qim.Delta) == 0;
    }

    private bool IsCoefficientSkippableForExtraction(short coefficient)
    {
        return coefficient == 0;
    }

    private bool IsCoefficientSkippableForCapacity(short coefficient)
    {
        return coefficient == 0;
    }
}