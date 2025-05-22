namespace DeepData.Core.Utils;

/// <summary>
/// A utility class for reading and writing data at the bit level.
/// Supports sequential bit manipulation and manages an internal bit index.
/// </summary>
public class BitWorker
{
    private readonly bool[] _bits;
    private int _currentBitIndex;

    /// <summary>
    /// Gets the total capacity of the BitWorker in bits.
    /// </summary>
    public int CapacityBits => _bits.Length;

    /// <summary>
    /// Gets the current read/write position in bits.
    /// </summary>
    public int CurrentPositionBits => _currentBitIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitWorker"/> class with a specified bit size.
    /// </summary>
    /// <param name="sizeInBits">The total number of bits this worker can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sizeInBits"/> is negative.</exception>
    public BitWorker(int sizeInBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeInBits);
        _bits = new bool[sizeInBits];
        _currentBitIndex = 0;
    }

    /// <summary>
    /// Creates a new <see cref="BitWorker"/> instance from a byte array,
    /// including a header to store the length of the data.
    /// </summary>
    /// <param name="data">The byte array to be stored.</param>
    /// <returns>A <see cref="BitWorker"/> initialized with the data and its length header.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the data length exceeds the maximum representable value for the header.</exception>
    public static BitWorker CreateWithHeaderFromBytes(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Constants.HeaderBits should define how many bits are used for the length header.
        // For example, if it's 32, then MaxDataLengthInBytes = (2^32 - 1).
        // Check if data.Length can be safely represented by a uint (if HeaderBits is 32)
        // Or if data.Length is small enough to fit within HeaderBits if it's less than 32.
        if (Constants.HeaderBits > 0 && (data.Length * 8L + Constants.HeaderBits > int.MaxValue))
        {
            // This checks for potential overflow if total bits exceed int.MaxValue
            // Or if the length itself is too large for the header to store.
            throw new ArgumentOutOfRangeException(nameof(data), "The size of the data exceeds the capacity for internal storage or header representation.");
        }
        
        var bitWorker = new BitWorker(Constants.HeaderBits + data.Length * 8);

        // Write the length of the data (in bytes) into the header.
        // Assuming Constants.HeaderBits is 32 for a uint header.
        bitWorker.WriteUInt32((uint)data.Length, Constants.HeaderBits); 
        bitWorker.WriteBytes(data); // Write the actual data bytes

        bitWorker.ResetPosition(); // Reset to start for reading
        return bitWorker;
    }

    /// <summary>
    /// Writes a single boolean bit to the current position.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to write beyond the allocated capacity.</exception>
    public void WriteBit(bool value)
    {
        ThrowIfPositionAtEnd();
        _bits[_currentBitIndex++] = value;
    }

    /// <summary>
    /// Writes a specified number of bits from the end of a byte value.
    /// </summary>
    /// <param name="value">The byte value.</param>
    /// <param name="numberOfBits">The number of least significant bits to write (1-8).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numberOfBits"/> is not between 1 and 8.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to write beyond the allocated capacity.</exception>
    public void WriteBitsFromByte(byte value, int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 8) throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only write up to 8 bits from a byte.");

        for (int i = numberOfBits - 1; i >= 0; i--)
        {
            WriteBit(((value >> i) & 1) == 1);
        }
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The unsigned integer to write.</param>
    /// <param name="numberOfBits">The number of bits to write (1-32). Defaults to 32.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numberOfBits"/> is not between 1 and 32.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to write beyond the allocated capacity.</exception>
    public void WriteUInt32(uint value, int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 32) throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only write up to 32 bits for a UInt32.");

        for (int i = numberOfBits - 1; i >= 0; i--)
        {
            WriteBit((value & (1U << i)) != 0);
        }
    }

    /// <summary>
    /// Writes a byte value.
    /// </summary>
    /// <param name="value">The byte to write.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to write beyond the allocated capacity.</exception>
    public void WriteByte(byte value)
    {
        for (int i = 7; i >= 0; i--)
        {
            WriteBit(((value >> i) & 1) == 1);
        }
    }

    /// <summary>
    /// Writes an array of bytes.
    /// </summary>
    /// <param name="bytes">The byte array to write.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is null.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to write beyond the allocated capacity.</exception>
    public void WriteBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        foreach (byte b in bytes)
        {
            WriteByte(b);
        }
    }

    /// <summary>
    /// Reads a single boolean bit from the current position.
    /// </summary>
    /// <returns>The boolean value read.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to read beyond the allocated capacity.</exception>
    public bool ReadBit()
    {
        ThrowIfPositionAtEnd();
        return _bits[_currentBitIndex++];
    }

    public bool PeekBit()
    {
        ThrowIfPositionAtEnd();
        return _bits[_currentBitIndex];
    }

    /// <summary>
    /// Reads a specified number of bits and converts them to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="numberOfBits">The number of bits to read (1-32).</param>
    /// <returns>The unsigned integer value read.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numberOfBits"/> is not between 1 and 32.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to read beyond the allocated capacity.</exception>
    public uint ReadUInt32(int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 32) throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only read up to 32 bits for a UInt32.");

        uint result = 0;
        for (int i = numberOfBits - 1; i >= 0; i--)
        {
            if (ReadBit())
            {
                result |= (1U << i);
            }
        }
        return result;
    }

    /// <summary>
    /// Reads a byte value.
    /// </summary>
    /// <returns>The byte value read.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if trying to read beyond the allocated capacity.</exception>
    public byte ReadByte()
    {
        byte result = 0;
        for (int i = 0; i < 8; i++) // Read 8 bits
        {
            if (ReadBit())
            {
                result |= (byte)(1 << (7 - i));
            }
        }
        return result;
    }

    /// <summary>
    /// Reads a byte array, expecting a header at the beginning to determine the length.
    /// The read position is reset to the start before reading the header.
    /// </summary>
    /// <returns>The extracted byte array.</returns>
    /// <exception cref="InvalidDataException">Thrown if the header indicates a length that would exceed available bits.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if there are not enough bits to read the header or the payload.</exception>
    public byte[] ReadBytesWithHeader()
    {
        ResetPosition(); // Ensure we read the header from the start

        // Read the length from the header. Assuming Constants.HeaderBits is 32 for a uint header.
        uint lengthInBytes = ReadUInt32(Constants.HeaderBits);

        // Check if reading the entire payload would exceed the BitWorker's capacity.
        // This is crucial to prevent IndexOutOfRangeException later.
        if (_currentBitIndex + (long)lengthInBytes * 8 > CapacityBits)
        {
            throw new InvalidDataException($"Not enough bits available to read the payload indicated by the header. Header says {lengthInBytes} bytes, but only {CapacityBits - _currentBitIndex} bits remain.");
        }

        byte[] bytes = new byte[lengthInBytes];
        for (int i = 0; i < lengthInBytes; i++)
        {
            bytes[i] = ReadByte();
        }
        return bytes;
    }

    /// <summary>
    /// Resets the internal read/write position to the beginning of the bit array.
    /// </summary>
    public void ResetPosition()
    {
        _currentBitIndex = 0;
    }

    /// <summary>
    /// Determines if the current read/write position has reached the end of the allocated bits.
    /// </summary>
    /// <returns>True if at or beyond the end, false otherwise.</returns>
    public bool IsAtEnd()
    {
        return _currentBitIndex >= _bits.Length;
    }

    /// <summary>
    /// Throws an <see cref="IndexOutOfRangeException"/> if the current position is at or beyond the end.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Thrown if at the end of capacity.</exception>
    private void ThrowIfPositionAtEnd()
    {
        if (IsAtEnd())
        {
            throw new IndexOutOfRangeException("Cannot read or write: current position is at or beyond the end of the allocated bits.");
        }
    }
}