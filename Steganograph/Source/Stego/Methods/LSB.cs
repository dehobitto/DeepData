using DeepData.Stego.Interfaces;

namespace DeepData.Stego.Methods;

public class Lsb : IStegoMethod<byte[], byte[]>
{
    /// <summary>
    /// We are using LSB method. In options we can choose LSB-1 or LSB-2. It works correctly only for those 2 numbers.
    /// And its the same for LsbStrength 1 or 2, because of bit masks and etc.
    /// </summary>
    public byte[] Embed(byte[] source, byte[] data, Options options)
    {
        byte bitsPerByte = options.LsbStrength;
        // Storing bitMask, and doing weird stuff with bitsPerByte, but in the end we either get 1 if 1 and 3 if 2
        // The translation is bitsPerByte * 2 - 1
        byte bitMask = (byte)((1 << bitsPerByte) - 1);
        // 1 byte = 8 bits sooo...
        int totalDataBits = (sizeof(int) + data.Length) * 8;
        int maxAvailableBits = source.Length * bitsPerByte;
        
        if (totalDataBits > maxAvailableBits)
        {
            throw new InvalidOperationException("Not enough space in source to embed data.");
        }
        
        // An array of bits where the all bits to hide will be storred
        bool[] bits = new bool[totalDataBits];
        // An variable so its earier to navigate through this loops, shows how much of bits we already used
        int bitIndex = 0;
        
        // we are using the first 32 bits (4 bytes) or the full size of int to store the length of hidden data further to use
        for (int i = 31; i >= 0; i--)
        {
            // shift data to the right and take last bit
            bits[bitIndex++] = ((data.Length >> i) & 1) == 1;
        }
        
        // Traverse each byte
        for (int i = 0; i < data.Length; i++)
        {
            // Each bit
            for (int b = 7; b >= 0; b--)
            {
                bits[bitIndex++] = ((data[i] >> b) & 1) == 1;
            }
        }
        
        // cloning the source for security reasons
        byte[] result = (byte[])source.Clone();
        // zero it here, because now we need to go from the start and place all gathered bits
        bitIndex = 0;
        
        // and now just mixing the 1 or 2 bits of result(origin) with the bits
        for (int i = 0; bitIndex < bits.Length; i++)
        {
            byte newBits = 0;

            for (int b = bitsPerByte - 1; b >= 0 && bitIndex < bits.Length; b--)
            {
                if (bits[bitIndex++])
                {
                    newBits |= (byte)(1 << b);
                }
            }
            // n : [1, 2] є N == bitMask
            // So (source[i] & ~bitMask) -> we are cleaning the last n bits so ~n is like 11111100 if n is 2
            // (newBits & bitMask) -> its like the opposite & bitMask we will get 00000011 if n is 2
            // and the | symbol is basically an add operation
            result[i] = (byte)((source[i] & ~bitMask) | (newBits & bitMask));
        }
        
        return result;
    }
    

    public byte[] Extract(byte[] source, Options options)
    {
        byte bitsUsedPerByte = options.LsbStrength;
        // explained above
        byte bitMask = (byte)((1 << bitsUsedPerByte) - 1);

        // variable to store the future length we get from first 32 bits
        int embeddedDataLength = 0;
        int embeddedLengthBits = 32;
        int bitIndex = 0; // its like the bitIndex

        // Now we reading 32 first bits and get data
        for (int i = 0; bitIndex < embeddedLengthBits; i++)
        {
            // get current bits to work with 1 or 2
            byte currentByte = (byte)(source[i] & bitMask);

            for (int bitPos = bitsUsedPerByte - 1; bitPos >= 0 && bitIndex < embeddedLengthBits; bitPos--)
            {
                int bit = (currentByte >> bitPos) & 1;
                // get and add to the length the bit we got
                embeddedDataLength = (embeddedDataLength << 1) | bit;
                bitIndex++;
            }
        }
        
        // sanity checks
        if (embeddedDataLength < 0 || embeddedDataLength > source.Length)
        {
            throw new InvalidOperationException("Wrong length");
        }
        
        // All bits we need to collect
        int totalDataBits = embeddedDataLength * 8;
        // buffer array for extracted things
        byte[] extractedData = new byte[embeddedDataLength];
        
        // so we start after the first 32 bits
        int sourceIndex = bitIndex / bitsUsedPerByte;
        // zero it for optimisation purpose
        bitIndex = 0;

        // read data
        while (bitIndex < totalDataBits)
        {
            // get bits to work with
            byte currentByteBits = (byte)(source[sourceIndex] & bitMask); // mask
            
            for (int bitPos = bitsUsedPerByte - 1; bitPos >= 0 && bitIndex < totalDataBits; bitPos--)
            {
                int bit = (currentByteBits >> bitPos) & 1; // we get next bit

                int byteIndex = bitIndex / 8; 
                int bitInByteIndex = 7 - (bitIndex % 8);
                extractedData[byteIndex] |= (byte)(bit << bitInByteIndex);

                bitIndex++;
            }
            sourceIndex++;
        }

        return extractedData;
    }
}
