namespace Steganograph;

public interface IStego
{
   // bytes from data -> source
   byte[] Embed(byte[] source, byte[] data, Options options); 
   
   // get from dataSource
   byte[] Extract(byte[] dataSource, Options options);
}

public record Options
{
   public int QimDelta { get; init; } = 16;
}

public class LsbStego : IStego
{
   public byte[] Embed(byte[] source, byte[] data, Options options)
   {
      if (source == null) throw new ArgumentNullException(nameof(source));
      if (data == null) throw new ArgumentNullException(nameof(data));

      // Total bits needed: 32 bits for length + 8 * data.Length
      int totalDataBits = sizeof(int) * 8 + data.Length * 8;
      
      if (totalDataBits > source.Length)
         throw new ArgumentException("Source is too small to hold the data.");

      // Copy source so we don't modify original
      var output = new byte[source.Length];
      Buffer.BlockCopy(source, 0, output, 0, source.Length);

      // Prepare bit stream: length prefix then data
      var bits = new bool[totalDataBits];
      // Write length prefix (big-endian)
      int length = data.Length;
      for (int i = 0; i < 32; i++)
      {
         bits[i] = ((length >> (31 - i)) & 1) == 1;
      }
      // Write data bits
      for (int i = 0; i < data.Length * 8; i++)
      {
         int byteIndex = i / 8;
         int bitIndex = 7 - (i % 8);
         bits[32 + i] = ((data[byteIndex] >> bitIndex) & 1) == 1;
      }

      // Embed bits into LSB of each byte
      for (int i = 0; i < totalDataBits; i++)
      {
         output[i] = (byte)((output[i] & 0xFE) | (bits[i] ? 1 : 0));
      }

      return output;
   }

   public byte[] Extract(byte[] dataSource, Options options)
   {
      if (dataSource == null) 
         throw new ArgumentNullException(nameof(dataSource));
      // маємо хоча б 32 байти, по одному для кожного біта довжини
      if (dataSource.Length < 32)
         throw new ArgumentException("Data source is too small to contain embedded length.");

      // 1) Зчитуємо 32 LSB → length
      int length = 0;
      for (int i = 0; i < 32; i++)
      {
         int bit = dataSource[i] & 1;
         length = (length << 1) | bit;
      }

      if (length < 0)
         throw new InvalidOperationException("Invalid embedded length.");

      // 2) Обчислюємо, скільки байтів-записів (по 1 біт у кожному) нам треба
      int dataBits  = length * 8;
      int totalBits = 32 + dataBits;

      // Тепер порівнюємо «скільки байтів нам реально доступно»
      if (totalBits > dataSource.Length)
         throw new ArgumentException("Data source does not contain full embedded payload.");

      // 3) Витягуємо біти й збираємо назад у байти
      var extracted = new byte[length];
      for (int i = 0; i < dataBits; i++)
      {
         int srcIdx   = 32 + i;              // байт, у якому лежить потрібний біт
         int outByte  = i / 8;               // у який байт результату пишемо
         int outBit   = 7 - (i % 8);         // у який біт байта пишемо (MSB first)

         int bit = dataSource[srcIdx] & 1;
         extracted[outByte] |= (byte)(bit << outBit);
      }

      return extracted;
   }


}