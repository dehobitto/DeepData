using DeepData.Stego;
using DeepData.Stego.Extensions;
using DeepData.Stego.Methods;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Test;

public class LsbTests
{
    private const string FileName = "test1";
    private readonly Image<Rgba32> _image = Image.Load<Rgba32>(Constants.TestImageInputPath + FileName + ".png");
    [Fact]
    public void Lsb_Embed_Extract_NoSave()
    {
        var steg = new Lsb();
        byte[] originalData = "Hello, Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            LsbStrength = Constants.TestLsbStrength
        };

        var embeddedImage = steg.Embed(_image.ToBytes(out _), originalData, options);
        var extractedData = steg.Extract(embeddedImage, options);

        // Assert
        Assert.Equal(originalData, extractedData);
    }

    [Fact]
    public void Lsb_Embed_Extract_Save()
    {
        var steg = new Lsb();
        byte[] originalData = "Hello, Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            LsbStrength = Constants.TestLsbStrength
        };

        (int w, int h) size;
        var embeddedImage = steg.Embed(_image.ToBytes(out size), originalData, options);
        embeddedImage.ToImage(size).Save(Constants.TestImageOutputPath + FileName + "LsbOut.png", new PngEncoder());
        using var imageTest = Image.Load<Rgba32>(Constants.TestImageOutputPath + FileName + "LsbOut.png");
        var extractedData = steg.Extract(imageTest.ToBytes(out _), options);

        Assert.Equal(originalData, extractedData);
    }
}
