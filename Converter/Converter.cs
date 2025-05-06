using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Converter;

public class ImgToByte
{
    public static byte[] ToBytes(string path, out (int w, int h) size) //, 
    {
        using var image = Image.Load<Rgba32>(path);
        {
            (size.w, size.h) = (image.Width, image.Height);
            var pixelCount = image.Width * image.Height;
            // RGBA in each pixel
            var buffer = new byte[pixelCount * 4];
            
            image.CopyPixelDataTo(buffer);
            return buffer;                  
        }
    }
    
    public static Image<Rgba32> ToImage(byte[] pixelBytes, (int w, int h) size)
    {
        if (pixelBytes.Length != size.w * size.h * 4)
        {
            throw new ArgumentException("Pixel data size does not match width * height * 4.");
        }

        var image = Image.LoadPixelData<Rgba32>(pixelBytes, size.w, size.h);
        return image;
    }
}