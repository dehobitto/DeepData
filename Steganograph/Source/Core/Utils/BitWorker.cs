namespace DeepData.Stego.Utils;

public class BitWorker(int size)
{
    private readonly bool[] _bits = new bool[size];
    private int BitIndex { get; set; }

    public static BitWorker FromBytes(byte[] data)
    {
        var bw = new BitWorker(Constants.HeaderBits + data.Length * 8);
        
        bw.Write(data);
        bw.Restart();
        
        return bw;
    }

    public void Write(bool value)
    {
        EnsureNotEnded();
        _bits[BitIndex++] = value;
    }

    public void Write(byte[] bytes)
    {
        WriteInt(bytes.Length, Constants.HeaderBits);
        
        foreach (var b in bytes)
        {
            WriteByte(b);
        }
    }

    public void WriteBitsFromByte(byte value, int bitCount)
    {
        for (int i = bitCount - 1; i >= 0; i--)
        {
            Write(((value >> i) & 1) == 1);
        }
    }

    public bool ReadBit()
    {
        EnsureNotEnded();
        return _bits[BitIndex++];
    }

    public byte ReadBits(int bitCount)
    {
        byte result = 0;
        
        for (int i = bitCount - 1; i >= 0 && !IsEnded(); i--)
        {
            if (ReadBit())
            {
                result |= (byte)(1 << i);
            }
        }
        
        return result;
    }

    public int ReadInt(int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, 32);

        int result = 0;
        
        for (int i = bitCount - 1; i >= 0; i--)
        {
            if (ReadBit())
            {
                result |= (1 << i);
            }
        }
        
        return result;
    }

    public byte[] ReadBytes()
    {
        EnsureNotEnded();
        Restart();

        int length = ReadInt(Constants.HeaderBits);
        
        if (length < 0)
        {
            throw new InvalidOperationException("Invalid length detected");
        }

        var bytes = new byte[length];
        
        for (int i = 0; i < length; i++)
        {
            bytes[i] = ReadByte();
        }
        
        return bytes;
    }

    public void Restart() => BitIndex = 0;
    public bool IsEnded() => BitIndex >= _bits.Length;

    // Private helpers
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
