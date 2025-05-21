namespace DeepData.Core.Utils;

public class BitWorker
{
    private readonly bool[] _bits;
    private int BitIndex { get; set; }

    public BitWorker(int size)
    {
        _bits = new bool[size];
    }

    public static BitWorker CreateFromBytes(byte[] data)
    {
        // Проверка максимальной длины
        if (data.Length > int.MaxValue / 8 - Constants.HeaderBits)
            throw new ArgumentException("Data too large");
    
        var bw = new BitWorker(Constants.HeaderBits + data.Length * 8);
    
        // Записываем длину как 32 бита (uint)
        bw.WriteUInt32((uint)data.Length);
    
        foreach (byte b in data)
            bw.WriteByte(b);
    
        bw.Restart();
        return bw;
    }

    public void Write(bool value)
    {
        EnsureNotEnded();
        _bits[BitIndex++] = value;
    }

    public void WriteWithLength(byte[] bytes)
    {
        WriteInt(bytes.Length, Constants.HeaderBits);
        
        foreach (var b in bytes)
        {
            WriteByte(b);
        }
    }
    
    public void Write(byte[] bytes)
    {
        foreach (var b in bytes)
        {
            WriteByte(b);
        }
    }

    public void WriteNLastBits(byte value, int n)
    {
        for (int i = n - 1; i >= 0; i--)
        {
            Write(((value >> i) & 1) == 1);
        }
    }

    public bool ReadBit()
    {
        EnsureNotEnded();
        return _bits[BitIndex++];
    }

    public byte ReadNBytes(int n)
    {
        byte result = 0;
        
        for (int i = n - 1; i >= 0 && !IsEnded(); i--)
        {
            if (ReadBit())
            {
                result |= (byte)(1 << i);
            }
        }
        
        return result;
    }
    
    public uint ReadUInt32(int bitCount = 32)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitCount);
        if (bitCount > 32) 
            throw new ArgumentException("Max 32 bits for UInt32");

        uint result = 0;
        for (int i = bitCount - 1; i >= 0; i--)
        {
            if (ReadBit())
                result |= (1U << i);
        }
        return result;
    }

    public int ReadNLastBits(int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitCount);
        
        long result = 0;
    
        for (int i = bitCount - 1; i >= 0; i--)
        {
            if (ReadBit())
            {
                result |= (1L << i);
            }
        }
        
        return (int)result;
    }
    
    public void WriteUInt32(uint value, int bitCount = 32)
    {
        for (int i = bitCount - 1; i >= 0; i--)
        {
            Write((value & (1U << i)) != 0);
        }
    }

    public byte[] ReadBytes(int maxLen)
    {
        Restart();
        
        uint length = ReadUInt32(Constants.HeaderBits);

        int requiredBits = (int)(length * 8);
        if (BitIndex + requiredBits > _bits.Length)
            throw new InvalidDataException("Not enough bits for payload");
    
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = ReadByte();
        }
        return bytes;
    }

    public void Restart()
    {
        BitIndex = 0;
    }

    public bool IsEnded()
    {
        return BitIndex >= _bits.Length;
    }

    private void WriteByte(byte value)
    {
        for (int i = 7; i >= 0; i--)
        {
            Write(((value >> i) & 1) == 1);
        }
    }

    private void WriteInt(int value, int bitCount)
    {
        for (int i = bitCount - 1; i >= 0; i--)
        {
            Write(((value >> i) & 1) == 1);
        }
    }

    private byte ReadByte()
    {
        byte result = 0;
        
        for (int i = 0; i < 8; i++)
        {
            if (ReadBit())
            {
                result |= (byte)(1 << (7 - i));
            }
        }
        
        return result;
    }
    
    private void EnsureNotEnded()
    {
        if (IsEnded())
        {
            throw new IndexOutOfRangeException("No more bits to read or write.");
        }
    }
}