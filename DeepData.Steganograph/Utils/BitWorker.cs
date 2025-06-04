namespace DeepData.Utils;

public class BitWorker
{
    private readonly bool[] _bits;

    /// <summary>
    ///     Creates a new bitWorker with a specific bit size.
    /// </summary>
    /// <param name="sizeInBits">The total number of bits to store.</param>
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
    public void WriteBitsFromByte(byte value, int numberOfBits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        
        if (numberOfBits > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only write up to 8 bits from a byte.");
        }

        for (int i = numberOfBits - 1; i >= 0; i--)
        {
            WriteBit(((value >> i) & 1) == 1);
        }
    }
    
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
    
    public void WriteByte(byte value)
    {
        for (var i = 7; i >= 0; i--)
        {
            WriteBit(((value >> i) & 1) == 1);
        }
    }
    
    public void WriteBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        foreach (var b in bytes)
        {
            WriteByte(b);
        }
    }
    
    public bool ReadBit()
    {
        ThrowIfPositionAtEnd();
        
        return _bits[CurrentPositionBits++];
    }
    
    public bool PeekBit()
    {
        ThrowIfPositionAtEnd();
        
        return _bits[CurrentPositionBits];
    }
    
    public uint ReadUInt32(int numberOfBits = 32)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfBits);
        
        if (numberOfBits > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfBits), "Can only read up to 32 bits for a UInt32.");
        }

        var result = 0u;

        for (var i = numberOfBits - 1; i >= 0; i--)
        {
            if (ReadBit())
            {
                result |= 1U << i;
            }
        }

        return result;
    }
    
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
    public byte[] ReadBytesWithHeader()
    {
        ResetPosition();

        var lengthInBytes = ReadUInt32(StegoConstants.HeaderBits);

        if (CurrentPositionBits + (long)lengthInBytes * 8 > CapacityBits)
        {
            throw new InvalidDataException(
                $"Not enough bits left for the payload. Header says {lengthInBytes} bytes, but only {CapacityBits - CurrentPositionBits} bits remain.");
        }

        byte[] bytes = new byte[lengthInBytes];
        
        for (var i = 0; i < lengthInBytes; i++)
        {
            bytes[i] = ReadByte();
        }
        
        return bytes;
    }
    
    public void ResetPosition()
    {
        CurrentPositionBits = 0;
    }
    
    public bool IsAtEnd()
    {
        return CurrentPositionBits >= _bits.Length;
    }
    
    private void ThrowIfPositionAtEnd()
    {
        if (IsAtEnd())
        {
            throw new IndexOutOfRangeException("Cannot read or write: bitworker is at the end of the data");
        }
    }
}