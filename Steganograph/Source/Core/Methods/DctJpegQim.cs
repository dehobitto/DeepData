using BitMiracle.LibJpeg.Classic;
using DeepData.Core.Extensions;
using DeepData.Core.Models;
using DeepData.Core.Utils;
using DeepData.Core.Utils.StegoSpecific;
using static DeepData.Core.Utils.StegoSpecific.QimHelper;

namespace DeepData.Core.Methods;

/// <summary>
/// Implements a steganography method for embedding and extracting data within JPEG images using the Discrete Cosine Transform (DCT) coefficients.
/// </summary>
public class DctJpeg : StegoMethod<Stream, byte[]>
{
    /// <summary>
    /// Embeds the specified data into the source JPEG stream.
    /// </summary>
    /// <param name="source">The source JPEG stream.</param>
    /// <param name="data">The byte array data to embed.</param>
    /// <returns>A new stream containing the JPEG image with embedded data.</returns>
    /// <exception cref="ArgumentException">Thrown if the data to embed exceeds the available capacity in the source image.</exception>
    public override Stream Embed(
        Stream source,
        byte[] data)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;
        BitWorker bitWriter = BitWorker.CreateWithHeaderFromBytes(data);
        int maximumCapacity = GetCapacityBytes(source);

        if (maximumCapacity < data.Length)
        {
            throw new ArgumentException("The data cannot fit in the provided image.");
        }

        ProcessComponents(decompressionResult, coefficientArrays, (blocks, componentInfo) =>
        {
            EmbedDataIntoBlocks(blocks, componentInfo, bitWriter);
        },
        (compIndex) => compIndex switch
        {
            0 => Options.ChannelsJpeg.Y == 0,
            1 => Options.ChannelsJpeg.Cb == 0,
            2 => Options.ChannelsJpeg.Cr == 0,
            _ => true
        });

        return JpegHelper.Compress(decompressionResult, coefficientArrays);
    }

    /// <summary>
    /// Embeds bits from the BitWorker into the DCT coefficients of the provided blocks.
    /// </summary>
    /// <param name="blocks">The 2D array of JBLOCKs representing image blocks.</param>
    /// <param name="componentInfo">The JPEG component information.</param>
    /// <param name="bitWorker">The BitWorker containing the data to embed.</param>
    private void EmbedDataIntoBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, (currentBlock) =>
        {
            for (int i = 0; i < Options.JpegBlocksCount; i++) // Iterate through AC coefficients (skip DC at index 0)
            {
                short coefficient = currentBlock[i];
                
                if (bitWorker.IsAtEnd()) // Stop embedding if all data is written
                {
                    return true; // Indicate that processing should stop
                }
                
                bool bit = bitWorker.PeekBit();

                if (IsCoefficientSkippableForEmbedding(coefficient, bit))
                {
                    continue;
                }

                currentBlock[i] = QimEmbedBit(coefficient, bitWorker.ReadBit(), Options.QimDelta);
            }
            return false; // Indicate that processing should continue
        });
    }
    
    /// <summary>
    /// Extracts embedded data from the source JPEG stream.
    /// </summary>
    /// <param name="source">The source JPEG stream containing embedded data.</param>
    /// <returns>A byte array representing the extracted data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is insufficient data to read the header.</exception>
    public override byte[] Extract(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        var coefficientArrays = decompressionResult.CoefArrays;
        int totalBitsToExtract = GetCapacityBytes(source) * 8; // Assuming this factor is part of the original logic

        var bitReader = new BitWorker(totalBitsToExtract);
        
        ProcessComponents(decompressionResult, coefficientArrays, (blocks, componentInfo) =>
        {
            ExtractDataFromBlocks(blocks, componentInfo, bitReader);
        },
        (compIndex) => compIndex switch
        {
            0 => Options.ChannelsJpeg.Y == 0,
            1 => Options.ChannelsJpeg.Cb == 0,
            2 => Options.ChannelsJpeg.Cr == 0,
            _ => true
        });
        
        if (totalBitsToExtract < Constants.HeaderBits)
            throw new InvalidOperationException("Insufficient data for header.");
        
        return bitReader.ReadBytesWithHeader();
    }

    /// <summary>
    /// Determines if the payload can fit into the source JPEG image.
    /// </summary>
    /// <param name="source">The source JPEG stream.</param>
    /// <param name="payload">The byte array payload to check.</param>
    /// <returns>True if the payload will fit, false otherwise.</returns>
    public override bool WillFit(
        Stream source,
        byte[] payload)
    {
        int availableBits = GetCapacityBytes(source);
    
        int requiredBits = Constants.HeaderBits + payload.Length * 8;
    
        return availableBits >= requiredBits;
    }

    /// <summary>
    /// Calculates the maximum number of bits that can be embedded into the source JPEG stream.
    /// </summary>
    /// <param name="source">The source JPEG stream.</param>
    /// <returns>The maximum embedding capacity in bits.</returns>
    public override int GetCapacityBytes(Stream source)
    {
        var decompressionResult = JpegHelper.Decompress(source);
        int capacityCount = 0;

        ProcessComponents(decompressionResult, decompressionResult.CoefArrays, (blocks, componentInfo) =>
        {
            ProcessBlocks(blocks, componentInfo, (currentBlock) =>
            {
                for (int i = 0; i < Options.JpegBlocksCount; i++)
                {
                    short coefficient = currentBlock[i];

                    if (IsCoefficientSkippableForCapacity(coefficient))
                    {
                        continue;
                    }

                    capacityCount++;
                }
                return false;
            });
        },
        (compIndex) => compIndex switch
        {
            0 => Options.ChannelsJpeg.Y == 0,
            1 => Options.ChannelsJpeg.Cb == 0,
            2 => Options.ChannelsJpeg.Cr == 0,
            _ => true
        });

        source.Seek(0, SeekOrigin.Begin);

        return capacityCount / 8;
    }

    /// <summary>
    /// Extracts bits from the DCT coefficients of the provided blocks and writes them to the BitWorker.
    /// </summary>
    /// <param name="blocks">The 2D array of JBLOCKs representing image blocks.</param>
    /// <param name="componentInfo">The JPEG component information.</param>
    /// <param name="bitWorker">The BitWorker to write the extracted bits to.</param>
    private void ExtractDataFromBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        BitWorker bitWorker)
    {
        ProcessBlocks(blocks, componentInfo, (currentBlock) =>
        {
            for (int i = 0; i < Options.JpegBlocksCount; i++) // Iterate through AC coefficients (skip DC at index 0)
            {
                short coefficient = currentBlock[i];
                
                if (IsCoefficientSkippableForExtraction(coefficient))
                {
                    continue;
                }

                if (bitWorker.IsAtEnd()) // Stop extracting if the target capacity is reached
                {
                    return true; // Indicate that processing should stop
                }

                bool extractedBit = QimExtractBit(coefficient, Options.QimDelta);
                bitWorker.WriteBit(extractedBit);
            }
            return false; // Indicate that processing should continue
        });
    }

    /// <summary>
    /// Processes JPEG components, applying a given action to each relevant component's blocks.
    /// </summary>
    /// <param name="decompressionResult">The result of the JPEG decompression.</param>
    /// <param name="coefficientArrays">The arrays of DCT coefficients.</param>
    /// <param name="action">The action to perform on the blocks and component info.</param>
    /// <param name="skipCondition">A function to determine if a component should be skipped.</param>
    private void ProcessComponents(
        JpegDecompressResult decompressionResult,
        jvirt_array<JBLOCK>[] coefficientArrays,
        Action<JBLOCK[][], jpeg_component_info> action,
        Func<int, bool> skipCondition)
    {
        int numberOfComponentsToProcess = Math.Min(Constants.JpegChannelsCount, decompressionResult.NumComponents);
        
        for (int componentIndex = 0; componentIndex < numberOfComponentsToProcess; componentIndex++)
        {
            if (skipCondition(componentIndex))
            {
                continue;
            }
            
            var componentInfo = decompressionResult.GetComponentInfo(componentIndex);
            var blocks = JpegHelper.GetBlocks(coefficientArrays[componentIndex], componentInfo);
            action(blocks, componentInfo);
        }
    }

    /// <summary>
    /// Processes blocks within a component, applying a given action to each block.
    /// </summary>
    /// <param name="blocks">The 2D array of JBLOCKs.</param>
    /// <param name="componentInfo">The JPEG component information.</param>
    /// <param name="blockAction">The action to perform on each JBLOCK. Returns true if processing should stop.</param>
    private void ProcessBlocks(
        JBLOCK[][] blocks,
        jpeg_component_info componentInfo,
        Func<JBLOCK, bool> blockAction)
    {
        for (int row = 0; row < componentInfo.Height_in_blocks(); row++)
        for (int col = 0; col < componentInfo.Width_in_blocks; col++)
        {
            JBLOCK currentBlock = blocks[row][col];
            if (blockAction(currentBlock))
            {
                return; // Stop processing blocks if the action indicated
            }
        }
    }

    /// <summary>
    /// Determines if a coefficient should be skipped during embedding.
    /// </summary>
    /// <param name="coefficient">The DCT coefficient.</param>
    /// <returns>True if the coefficient should be skipped, false otherwise.</returns>
    private bool IsCoefficientSkippableForEmbedding(short coefficient, bool bit)
    {
        return coefficient == 0 || QimEmbedBit(coefficient, bit, Options.QimDelta) == 0;
    }

    /// <summary>
    /// Determines if a coefficient should be skipped during extraction.
    /// </summary>
    /// <param name="coefficient">The DCT coefficient.</param>
    /// <returns>True if the coefficient should be skipped, false otherwise.</returns>
    private bool IsCoefficientSkippableForExtraction(short coefficient)
    {
        return coefficient == 0;
    }
    // TODO "< 2" is SUS maybe like embedBit if == 0 then skip

    /// <summary>
    /// Determines if a coefficient should be skipped during capacity calculation.
    /// </summary>
    /// <param name="coefficient">The DCT coefficient.</param>
    /// <returns>True if the coefficient should be skipped, false otherwise.</returns>
    private bool IsCoefficientSkippableForCapacity(short coefficient)
    {
        return coefficient == 0;
    }
}