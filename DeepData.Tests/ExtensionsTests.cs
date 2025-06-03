using DeepData.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = DeepData.Models.Size;

namespace DeepData.Test;

public class ExtensionsTests
{
    [Fact]
    public void ToBytes_ReturnsCorrectByteArray_AndSetsSize()
    {
        var width = 2;
        var height = 2;
        using var image = new Image<Rgba32>(width, height);

        image[0, 0] = new Rgba32(10, 20, 30, 40);
        image[1, 0] = new Rgba32(50, 60, 70, 80);
        image[0, 1] = new Rgba32(90, 100, 110, 120);
        image[1, 1] = new Rgba32(130, 140, 150, 160);

        var bytes = image.ToBytes(out var size);

        Assert.NotNull(bytes);
        Assert.Equal(width * height * 4, bytes.Length);
        Assert.Equal(width, size.W);
        Assert.Equal(height, size.H);

        Assert.Equal(10, bytes[0]);
        Assert.Equal(20, bytes[1]);
        Assert.Equal(30, bytes[2]);
        Assert.Equal(40, bytes[3]);
        Assert.Equal(50, bytes[4]);
    }

    [Fact]
    public void ToImage_ReturnsCorrectImage_FromValidByteArray()
    {
        var width = 2;
        var height = 2;
        var size = new Size(width, height);

        var pixels = new byte[]
        {
            10, 20, 30, 40,
            50, 60, 70, 80,
            90, 100, 110, 120,
            130, 140, 150, 160
        };

        var image = pixels.ToImage(size);

        Assert.NotNull(image);
        Assert.Equal(width, image.Width);
        Assert.Equal(height, image.Height);

        Assert.Equal(new Rgba32(10, 20, 30, 40), image[0, 0]);
        Assert.Equal(new Rgba32(50, 60, 70, 80), image[1, 0]);
        Assert.Equal(new Rgba32(90, 100, 110, 120), image[0, 1]);
        Assert.Equal(new Rgba32(130, 140, 150, 160), image[1, 1]);
    }

    [Fact]
    public void ToImage_ThrowsException_WhenByteArraySizeMismatch()
    {
        var size = new Size(2, 2);
        var wrongSizePixels = new byte[3];

        var ex = Assert.Throws<ArgumentException>(() => wrongSizePixels.ToImage(size));
        Assert.Contains("Pixel data size does not match", ex.Message);
    }
}