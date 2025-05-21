using DeepData.Core.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = DeepData.Core.Models.Size;

namespace DeepData.Test;

public class ExtensionsTests
{
    [Fact]
    public void ToBytes_ReturnsCorrectByteArray_AndSetsSize()
    {
        // Arrange
        int width = 2;
        int height = 2;
        using var image = new Image<Rgba32>(width, height);
        
        // Заполним пиксели для проверки
        image[0, 0] = new Rgba32(10, 20, 30, 40);
        image[1, 0] = new Rgba32(50, 60, 70, 80);
        image[0, 1] = new Rgba32(90, 100, 110, 120);
        image[1, 1] = new Rgba32(130, 140, 150, 160);

        // Act
        var bytes = image.ToBytes(out Size size);

        // Assert
        Assert.NotNull(bytes);
        Assert.Equal(width * height * 4, bytes.Length);
        Assert.Equal(width, size.W);
        Assert.Equal(height, size.H);

        // Проверим содержимое байт
        Assert.Equal(10, bytes[0]);  // R
        Assert.Equal(20, bytes[1]);  // G
        Assert.Equal(30, bytes[2]);  // B
        Assert.Equal(40, bytes[3]);  // A
        Assert.Equal(50, bytes[4]);  // следующий пиксель R
    }

    [Fact]
    public void ToImage_ReturnsCorrectImage_FromValidByteArray()
    {
        // Arrange
        int width = 2;
        int height = 2;
        var size = new Size(width, height);

        byte[] pixels = new byte[]
        {
            10, 20, 30, 40,
            50, 60, 70, 80,
            90, 100, 110, 120,
            130, 140, 150, 160
        };

        // Act
        var image = pixels.ToImage(size);

        // Assert
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
        // Arrange
        var size = new Size(width: 2, height:2);
        byte[] wrongSizePixels = new byte[3]; // меньше чем нужно

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => wrongSizePixels.ToImage(size));
        Assert.Contains("Pixel data size does not match", ex.Message);
    }
}