namespace DeepData.Utils;

public class BitWorker
{
    private readonly bool[] _bits;

    /// <summary>
    ///     Creates a new bitWorker with a specific bit size.
    /// </summary>
    /// <param name="sizeInBits">The total number of bits to store.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="sizeInBits" /> is negative.</exception>
    public BitWorker(int sizeInBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeInBits);
        _bits = new bool[sizeInBits];
        CurrentPositionBits = 0;
    }

    public int CapacityBits => _bits.Length;
    public int CurrentPositionBits { get; private set; }

    /// <summary>
    ///     Creates a <see cref="BitWorker" /> from a byte array, adding a length header first.
    /// </summary>
    /// <param name="data">The bytes to store.</param>
    /// <returns>A <see cref="BitWorker" /> ready with data and its length.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="data" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the data is too big for the header or internal storage.</exception>
    public static BitWorker CreateWithHeaderFromBytes(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (StegoConstants.HeaderBits > 0 && data.Length * 8L + StegoConstants.HeaderBits > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data size exceeds storage or header capacity.");
        }

        var bitWorker = new BitWorker(StegoConstants.HeaderBits + data.Length * 8);

        bitWorker.WriteUInt32((uint)data.Length, StegoConstants.HeaderBits);
        bitWorker.WriteBytes(data);

        bitWorker.ResetPosition();
        return bitWorker;
    }

    /// <summary>
    ///     Writes a single bit at the current position.
    /// </summary>
    /// <param name="value">The bit (true/false) to write.</param>
    /// <exception cref="IndexOutOfRangeException">If trying to write past the end.</exception>
    public void WriteBit(bool value)
    {
        ThrowIfPositionAtEnd();
        _bits[CurrentPositionBits++] = value;
    }

    /// <summary>
    ///     Writes a specific number of least significant bits from a byte.
    /// </summary>
    /// <param name="value">The byte source.</param>
    /// <param name="numberOfBits">How many bits to write (1-8).</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="numberOfBits" /> is out of range (1-8).</exception>
    /// <exception cref="IndexOutOfRangeException">If trying to write past the end.</exception>
    public void WriteBitsFromByte(byte value, int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only write up to 8 bits from a byte.");
        }

        for (var i = numberOfBits - 1; i >= 0; i--) WriteBit(((value >> i) & 1) == 1);
    }

    /// <summary>
    ///     Writes an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value">The unsigned integer to write.</param>
    /// <param name="numberOfBits">How many bits to write (1-32).</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="numberOfBits" /> is out of range (1-32).</exception>
    /// <exception cref="IndexOutOfRangeException">If trying to write past the end.</exception>
    public void WriteUInt32(uint value, int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only write up to 32 bits for a UInt32.");
        }

        for (var i = numberOfBits - 1; i >= 0; i--)
        {
            WriteBit((value & (1U << i)) != 0);
        }
    }

    /// <summary>
    ///     Writes an entire byte.
    /// </summary>
    /// <param name="value">The byte to write.</param>
    /// <exception cref="IndexOutOfRangeException">If trying to write past the end.</exception>
    public void WriteByte(byte value)
    {
        for (var i = 7; i >= 0; i--)
        {
            WriteBit(((value >> i) & 1) == 1);
        }
    }

    /// <summary>
    ///     Writes an array of bytes.
    /// </summary>
    /// <param name="bytes">The byte array to write.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="bytes" /> is null.</exception>
    /// <exception cref="IndexOutOfRangeException">If trying to write past the end.</exception>
    public void WriteBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        foreach (var b in bytes) WriteByte(b);
    }

    /// <summary>
    ///     Reads a single boolean bit from the current position.
    /// </summary>
    /// <returns>The bit read.</returns>
    /// <exception cref="IndexOutOfRangeException">If trying to read past the end.</exception>
    public bool ReadBit()
    {
        ThrowIfPositionAtEnd();
        return _bits[CurrentPositionBits++];
    }

    /// <summary>
    ///     Peeks at the next bit without advancing the position.
    /// </summary>
    /// <returns>The next bit.</returns>
    /// <exception cref="IndexOutOfRangeException">If trying to peek past the end.</exception>
    public bool PeekBit()
    {
        ThrowIfPositionAtEnd();
        return _bits[CurrentPositionBits];
    }

    /// <summary>
    ///     Reads a specific number of bits and converts them to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="numberOfBits">How many bits to read (1-32).</param>
    /// <returns>The unsigned integer read.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="numberOfBits" /> is out of range (1-32).</exception>
    /// <exception cref="IndexOutOfRangeException">If trying to read past the end.</exception>
    public uint ReadUInt32(int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        if (numberOfBits > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only read up to 32 bits for a UInt32.");
        }

        uint result = 0;

        for (var i = numberOfBits - 1; i >= 0; i--)
        {
            if (ReadBit())
            {
                result |= 1U << i;
            }
        }

        return result;
    }

    /// <summary>
    ///     Reads an entire byte.
    /// </summary>
    /// <returns>The byte read.</returns>
    /// <exception cref="IndexOutOfRangeException">If trying to read past the end.</exception>
    public byte ReadByte()
    {
        byte result = 0;

        for (var i = 0; i < 8; i++)
        {
            if (ReadBit())
            {
                result |= (byte)(1 << (7 - i));
            }
        }

        return result;
    }

    /// <summary>
    ///     Reads a byte array, using an initial header to find its length.
    ///     Resets the read position to the start first.
    /// </summary>
    /// <returns>The extracted byte array.</returns>
    /// <exception cref="InvalidDataException">If the header shows a length that's too big for the remaining bits.</exception>
    /// <exception cref="IndexOutOfRangeException">If not enough bits to read header or data.</exception>
    public byte[] ReadBytesWithHeader()
    {
        ResetPosition();

        var lengthInBytes = ReadUInt32(StegoConstants.HeaderBits);

        if (CurrentPositionBits + (long)lengthInBytes * 8 > CapacityBits)
        {
            throw new InvalidDataException(
                $"Not enough bits left for the payload. Header says {lengthInBytes} bytes, but only {CapacityBits - CurrentPositionBits} bits remain.");
        }

        var bytes = new byte[lengthInBytes];
        
        for (var i = 0; i < lengthInBytes; i++)
        {
            bytes[i] = ReadByte();
        }
        
        return bytes;
    }

    /// <summary>
    ///     Resets the read/write position to the very beginning.
    /// </summary>
    public void ResetPosition()
    {
        CurrentPositionBits = 0;
    }

    /// <summary>
    ///     Checks if the current position is at or past the end.
    /// </summary>
    /// <returns>True if at the end, false otherwise.</returns>
    public bool IsAtEnd()
    {
        return CurrentPositionBits >= _bits.Length;
    }

    /// <summary>
    ///     Throws an <see cref="IndexOutOfRangeException" /> if at the end of capacity.
    /// </summary>
    private void ThrowIfPositionAtEnd()
    {
        if (IsAtEnd())
        {
            throw new IndexOutOfRangeException("Cannot read or write: bitworker is at the end of the data");
        }
    }
}