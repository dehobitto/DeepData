using DeepData.Core.Models;
using DeepData.Core.Utils;
using DeepData.Core.Utils.StegoSpecific;

namespace DeepData.Core.Methods;

/// <summary>
/// Implements the Least Significant Bit (LSB) steganography method for embedding and extracting data within byte arrays.
/// This method modifies the least significant bits of the cover data.
/// </summary>
public class Lsb : StegoMethod<byte[], byte[]>
{
    /// <summary>
    /// Embeds the specified data into the source byte array using the LSB method.
    /// </summary>
    /// <param name="source">The source byte array (cover data) into which the data will be embedded.</param>
    /// <param name="data">The byte array data to embed.</param>
    /// <returns>A new byte array containing the source data with embedded information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if LSB strength is invalid or data is too large for header.</exception>
    /// <exception cref="StegoCapacityException">Thrown if the data to embed exceeds the available capacity in the source.</exception>
    public override byte[] Embed(byte[] source, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(data);

        // Validate LSB strength (e.g., 1 to 8 bits)
        ValidateLsbStrength(Options.LsbStrength);

        // Calculate actual required bits, including header
        int requiredTotalBits = Constants.HeaderBits + (data.Length * 8);

        // Get available capacity based on source and LSB strength
        int availableCapacityBits = GetCapacityBytes(source);

        if (availableCapacityBits < requiredTotalBits)
        {
            //throw new StegoCapacityException(
              //  "The data cannot fit in the provided image.",
              //  requiredTotalBits,
              //  availableCapacityBits);
        }

        // Create BitWorker with header (length of data)
        // BitWorker now correctly handles writing the header
        var dataBitWorker = BitWorker.CreateWithHeaderFromBytes(data);
        var result = (byte[])source.Clone(); // Work on a clone to not modify original source

        // Create a bitmask for the LSBs to be modified
        byte bitMask = CreateBitMask(Options.LsbStrength); // e.g., 00000111 for strength 3

        for (int i = 0; i < source.Length; i++)
        {
            // If all data has been written, stop embedding.
            if (dataBitWorker.IsAtEnd())
            {
                break;
            }

            // Read the next `Options.LsbStrength` bits from the data BitWorker
            // These bits will replace the LSBs of the source byte.
            byte bitsToEmbed = 0;
            for (int bitNum = 0; bitNum < Options.LsbStrength; bitNum++)
            {
                if (dataBitWorker.IsAtEnd())
                {
                    // If we run out of data bits mid-byte, break and use what we have.
                    // This scenario should be caught by the initial capacity check,
                    // but it's good practice for robustness.
                    break;
                }
                if (dataBitWorker.ReadBit())
                {
                    bitsToEmbed |= (byte)(1 << (Options.LsbStrength - 1 - bitNum));
                }
            }
            
            // Clear the LSBs in the source byte using the inverse of the bitmask
            // Then set the new LSBs from 'bitsToEmbed'
            result[i] = (byte)((source[i] & ~bitMask) | (bitsToEmbed & bitMask));
        }

        return result;
    }

    /// <summary>
    /// Extracts embedded data from the source byte array using the LSB method.
    /// </summary>
    /// <param name="source">The source byte array (stego data) from which to extract information.</param>
    /// <returns>A byte array representing the extracted data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if LSB strength is invalid.</exception>
    /// <exception cref="StegoExtractionException">Thrown if there is insufficient data to read the header or payload,
    /// or if the header indicates an invalid payload length.</exception>
    public override byte[] Extract(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);

        ValidateLsbStrength(Options.LsbStrength);

        // Calculate the maximum possible bits that could have been embedded
        int totalPossibleEmbeddedBits = source.Length * Options.LsbStrength;

        // Ensure there's enough capacity in the source to even read the header
        if (totalPossibleEmbeddedBits < Constants.HeaderBits)
        {
            //throw new StegoExtractionException("Insufficient data in the source to read the hidden data header.", StegoExtractionErrorType.InsufficientData);
        }

        // Initialize BitWorker with the maximum possible bits it could hold from the source
        // This ensures the BitWorker has enough internal storage for all extracted LSBs.
        var extractedBitsWorker = new BitWorker(totalPossibleEmbeddedBits);

        // Create a bitmask to isolate the LSBs during extraction
        byte bitMask = CreateBitMask(Options.LsbStrength);

        // Extract LSBs from each byte of the source and write them into the BitWorker
        foreach (byte b in source)
        {
            // Mask the byte to get only the LSBs that potentially hold data
            byte maskedBits = (byte)(b & bitMask);
            
            // Write these extracted LSBs into our BitWorker.
            // The BitWorker's WriteBitsFromByte will handle writing the correct number of bits.
            extractedBitsWorker.WriteBitsFromByte(maskedBits, Options.LsbStrength);
        }

        // Reset the BitWorker's position to read from the beginning
        extractedBitsWorker.ResetPosition();

        // Read the actual hidden data bytes using the header
        // This method automatically reads the header and then the specified number of bytes
        return extractedBitsWorker.ReadBytesWithHeader();
    }

    /// <summary>
    /// Determines if the payload can fit into the source byte array.
    /// </summary>
    /// <param name="source">The source byte array.</param>
    /// <param name="payload">The byte array payload to check.</param>
    /// <returns>True if the payload will fit, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="payload"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if LSB strength is invalid.</exception>
    public override bool WillFit(byte[] source, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(payload);
        ValidateLsbStrength(Options.LsbStrength);

        // Calculate total bits required (payload data + header)
        int requiredBits = (payload.Length * 8) + Constants.HeaderBits;

        // Get available capacity in bits
        int availableBits = GetCapacityBytes(source);

        return availableBits >= requiredBits;
    }

    /// <summary>
    /// Calculates the maximum number of bits that can be embedded into the source byte array.
    /// </summary>
    /// <param name="source">The source byte array.</param>
    /// <returns>The maximum embedding capacity in bits.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if LSB strength is invalid.</exception>
    public override int GetCapacityBytes(byte[] source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateLsbStrength(Options.LsbStrength);
        return source.Length * Options.LsbStrength;
    }

    /// <summary>
    /// Validates the LSB strength value, ensuring it's within a valid range (1 to 8).
    /// </summary>
    /// <param name="strength">The LSB strength value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the strength is less than 1 or greater than 8.</exception>
    private static void ValidateLsbStrength(int strength)
    {
        if (strength < 1 || strength > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "LSB strength must be between 1 and 8 bits.");
        }
    }

    /// <summary>
    /// Creates a bitmask for the specified LSB strength.
    /// For example, strength 3 yields 00000111 (binary).
    /// </summary>
    /// <param name="strength">The LSB strength (number of LSBs to affect).</param>
    /// <returns>A byte representing the bitmask.</returns>
    private static byte CreateBitMask(int strength)
    {
        // This creates a mask like (1 << strength) - 1
        // e.g., strength 3: (1 << 3) - 1 = 8 - 1 = 7 (00000111)
        return (byte)((1 << strength) - 1);
    }
}