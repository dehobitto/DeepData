using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = DeepData.Stego.Models.Size;

namespace DeepData.Stego.Extensions;

public static class Extensions
{
    /// <summary>
    /// Few extensions so it's easier to convert things, nothing to explain here
    /// </summary>
    
    public static byte[] ToBytes(this Image<Rgba32> image, out Size size)
    {
        size = new Size((image.Width, image.Height)); // we save size here because we will need it later to save the image
        var pixelCount = image.Width * image.Height;
        var buffer = new byte[pixelCount * 4];
            
        image.CopyPixelDataTo(buffer);
        return buffer;             
    }
    
    public static Image<Rgba32> ToImage(this byte[] pixelBytes, Size size)
    {
        if (pixelBytes.Length != size.w * size.h * 4)
        {
            throw new ArgumentException("Pixel data size does not match width * height * 4.");
        }
 
        var image = Image.LoadPixelData<Rgba32>(pixelBytes, size.w, size.h);
        return image;
    }
}