using System.Reflection;
using BitMiracle.LibJpeg.Classic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = DeepData.Models.Size;

namespace DeepData.Extensions;

public static class Extensions
{
    public static byte[] ToBytes(this Image<Rgba32> image, out Size size)
    {
        size = new Size(image.Width, image.Height);
        var pixelCount = image.Width * image.Height;
        var buffer = new byte[pixelCount * 4];

        image.CopyPixelDataTo(buffer);
        return buffer;
    }

    public static Image<Rgba32> ToImage(this byte[] pixelBytes, Size size)
    {
        if (pixelBytes.Length != size.W * size.H * 4)
        {
            throw new ArgumentException("Pixel data size does not match width * height * 4.");
        }

        var image = Image.LoadPixelData<Rgba32>(pixelBytes, size.W, size.H);
        return image;
    }

    public static int Height_in_blocks(this jpeg_component_info component)
    {
        var heightField =
            typeof(jpeg_component_info).GetField("height_in_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        return (int)heightField!.GetValue(component)!;
    }
    
    public static string GetExtension(this string fileName)
    {
        string extension = fileName.Split(".").Last();
        
        return extension;
    }
}