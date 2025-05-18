namespace DeepData.Stego.Utils;


public class BitWorker(int size)
{
    private readonly bool[] _bits = new bool[size];
    private int BitIndex { get; set; }

    public void Write(bool value)
    {
        if (IsEnded())
        {
            throw new IndexOutOfRangeException("No more space to write bits.");
        }
        
        _bits[BitIndex++] = value;
    }

    private void Write(byte b)
    {
        for (int i = 7; i >= 0; i--)
        {
            Write(((b >> i) & 1) == 1);
        }
    }

    private void Write(int value, int bitCount)
    {
        for (int i = bitCount - 1; i >= 0; i--)
        {
            bool bit = ((value >> i) & 1) == 1;
            Write(bit);
        }
    }
    
    public void Write(byte[] bytes)
    {
        Write(bytes.Length, Constants.HeaderBits);
        Array.ForEach(bytes, Write);
    }

    public bool ReadBit()
    {
        if (IsEnded())
        {
            throw new IndexOutOfRangeException("No more bits to read.");
        }
        
        return _bits[BitIndex++];
    }

    public byte ReadByte()
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

    public int ReadInt(int bitCount)
    {
        if (bitCount < 0 || bitCount > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 0 and 32.");
        }
        
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
        if (IsEnded())
        {
            throw new IndexOutOfRangeException("No more bits to read.");
        }
        int length = ReadInt(Constants.HeaderBits);
        byte[] bytes = new byte[length];
        
        for (int i = 0; i < length; i++)
        {
            bytes[i] = ReadByte();
        }
        
        return bytes;
    }
    
    public bool[] ToArray() => _bits;
    public void Restart() => BitIndex = 0;
    public bool IsEnded() => BitIndex >= _bits.Length;
}