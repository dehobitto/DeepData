using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace DeepData
{
    public class Steganograph
    {
        private static BitWorker bitWorker;
        private static string _signature = "2L3mlD6e8jfrVNUZX";

        public Steganograph(Bitmap bmp)
        {
            bitWorker = new BitWorker(bmp);
        }
        
        public void Hide(String data, String fileName)
        {
            if (CheckSignature())
            {
                Console.WriteLine("File is already hidden");
                return;
            }
            
            SetSignature();
            
            byte[] dataLengthBytes = BitConverter.GetBytes(Encoding.UTF8.GetBytes(data).Length);
            BitArray lenToHide = new BitArray(dataLengthBytes);

            bitWorker.PlaceBits(new BitArray(Int32.MaxValue), (_signature.Length * 8 / 6) + 1);
            bitWorker.PlaceBits(new BitArray(0), (_signature.Length * 8 / 6) + 1);
            bitWorker.PlaceBits(lenToHide, (_signature.Length * 8 / 6) + 1, -1);
            
            BitArray bitsToHide = new BitArray(Encoding.UTF8.GetBytes(data));
            bitWorker.PlaceBits(bitsToHide, (_signature.Length * 8 + 32) / 6 + 1);
            var bmpModified = bitWorker.bmp;
            
            bmpModified.Save(fileName + ".bmp", ImageFormat.Bmp);
            Console.WriteLine($"Data Hidden in Image. {bitsToHide.Length} bits written. LenToHide {data.Length}");
        }
        public string Decode()
        {
            if (!CheckSignature())
            {
                Console.WriteLine("There is no signature, so no file");
                return null;
            }
            
            BitArray bitsOfLen = bitWorker.GetBits(32, (_signature.Length * 8 / 6) + 1, -1);
            byte[] byteLenArray = new byte[4];   
            bitsOfLen.CopyTo(byteLenArray, 0);
            int len = BitConverter.ToInt32(byteLenArray, 0);
            
            Console.WriteLine("Decode len: " + len);
            BitArray bits = bitWorker.GetBits(len * 8, (_signature.Length * 8 + 32) / 6 + 1);
            byte[] byteArray = new byte[bits.Length / 8];   
            bits.CopyTo(byteArray, 0);
            return Encoding.UTF8.GetString(byteArray);
        }
        
        private void SetSignature()
        {
            BitArray bitsToHide = new BitArray(Encoding.UTF8.GetBytes(_signature));
            bitWorker.PlaceBits(bitsToHide);
        }
        
        private bool CheckSignature()
        {
            BitArray bits = bitWorker.GetBits(_signature.Length * 8);
            byte[] byteArray = new byte[bits.Length / 8];   
            bits.CopyTo(byteArray, 0);
            return Encoding.UTF8.GetString(byteArray).Equals(_signature);
        }
    }

    internal class BitWorker
    {
        internal Bitmap bmp { get; }
        public BitWorker(Bitmap bmp)
        {
            this.bmp = bmp;
        }

        internal void PlaceBits(BitArray bits, int startPixel = 0, int direction = 1)
        {
            int bitsLeft = bits.Length;
            bool finished = false; // Флаг, сигнализирующий, что данные закончились

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    if (startPixel > 0)
                    {
                        startPixel--;
                        continue;
                    }
                    
                    Color pixel = bmp.GetPixel(j, i);

                    // Получаем текущие 8-битовые представления каналов
                    BitArray[] channels = { 
                        new BitArray(new byte[] { pixel.R }),
                        new BitArray(new byte[] { pixel.G }),
                        new BitArray(new byte[] { pixel.B })
                    };

                    // Обходим 3 канала, в каждом по 2 младших бита (итого 6 бит на пиксель)
                    for (int k = 0; k < 3; k++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            if (bitsLeft > 0)
                            {
                                if (direction == -1)
                                {
                                    bitsLeft--;
                                    channels[k][z] = bits[bitsLeft];
                                }
                                else
                                {
                                    channels[k][z] = bits[bits.Length - bitsLeft];
                                    bitsLeft--;
                                }
                            }
                            else
                            {
                                channels[k][z] = false;
                                finished = true;
                            }
                        }
                    }

                    // Собираем новые значения каналов из обновлённых BitArray
                    byte rByte = GetByteFromBitArray(channels[0]);
                    byte gByte = GetByteFromBitArray(channels[1]);
                    byte bByte = GetByteFromBitArray(channels[2]);

                    Color newColor = Color.FromArgb(rByte, gByte, bByte);
                    bmp.SetPixel(j, i, newColor);

                    // Если данные закончились – завершаем обработку (выходим после обработки текущего пикселя)
                    if (finished && bitsLeft == 0)
                        return;
                }
            }
        }

        internal BitArray GetBits(int bitsAmount, int startPixel = 0, int direction = 1)
        {
            BitArray bits = new BitArray(bitsAmount);
            int bitsLeft = bitsAmount;
            bool finished = false;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    if (startPixel > 0)
                    {
                        startPixel--;
                        continue;
                    }
                    
                    Color pixel = bmp.GetPixel(j, i);

                    // Получаем 8-битовые представления каналов
                    BitArray[] channels = { 
                        new BitArray(new byte[] { pixel.R }),
                        new BitArray(new byte[] { pixel.G }),
                        new BitArray(new byte[] { pixel.B })
                    };

                    // Извлекаем по 2 младших бита из каждого канала
                    for (int k = 0; k < 3; k++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            if (bitsLeft > 0)
                            {
                                if (direction == -1)
                                {
                                    bitsLeft--;
                                    bits[bitsLeft] = channels[k][z];
                                }
                                else
                                {
                                    bits[bitsAmount - bitsLeft] = channels[k][z];
                                    bitsLeft--;
                                }
                            }
                            else
                            {
                                finished = true;
                            }
                        }
                    }
                    if (finished && bitsLeft == 0)
                        return bits;
                }
            }
            
            return bits;
        }
        
        private static byte GetByteFromBitArray(BitArray bitArray)
        {
            byte[] byteArray = new byte[1];
            bitArray.CopyTo(byteArray, 0);
            return byteArray[0];
        }
    }
}