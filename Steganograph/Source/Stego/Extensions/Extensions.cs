using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Stego.Extensions;

public static class Extensions
{
    /// <summary>
    /// Few extensions so its easier to convert things, nothing to explain here
    /// </summary>
    
    public static byte[] ToBytes(this Image<Rgba32> image, out (int w, int h) size)
    {
        (size.w, size.h) = (image.Width, image.Height); // we save size here because we will need it later to save the image
        var pixelCount = image.Width * image.Height;
        var buffer = new byte[pixelCount * 4];
            
        image.CopyPixelDataTo(buffer);
        return buffer;             
    }
    
    public static Image<Rgba32> ToImage(this byte[] pixelBytes, (int w, int h) size)
    {
        if (pixelBytes.Length != size.w * size.h * 4)
        {
            throw new ArgumentException("Pixel data size does not match width * height * 4.");
        }
 
        var image = Image.LoadPixelData<Rgba32>(pixelBytes, size.w, size.h);
        return image;
    }
    
    public static bool[] IntToBits(int value)
    {
        bool[] bits = new bool[32];
        for (int i = 0; i < 32; i++)
        {
            bits[i] = ((value >> (31 - i)) & 1) == 1;
        }
        return bits;
    }
}