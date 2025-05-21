using DeepData.Core;
using DeepData.Core.Extensions;
using DeepData.Core.Methods;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Test.Methods;

public class LsbTests
{
    private const string FileName = "test1";
    private readonly Image<Rgba32> _image = Image.Load<Rgba32>(TestConstants.GetInputImagePath(FileName));

    [Fact]
    public void Lsb_Embed_Extract_NoSave()
    {
        byte[] originalData = "Hello, Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            LsbStrength = TestConstants.TestLsbStrength
        };

        var steg = new Lsb
        {
            Options = options
        };

        var embeddedImage = steg.Embed(_image.ToBytes(out _), originalData);
        var extractedData = steg.Extract(embeddedImage);

        Assert.Equal(originalData, extractedData);
    }

    [Fact]
    public void Lsb_Embed_Extract_Save()
    {
        byte[] originalData = "Hdfklgjdflkgjdf;lkgj hello world"u8.ToArray();

        var options = new Options
        {
            LsbStrength = TestConstants.TestLsbStrength
        };

        var steg = new Lsb
        {
            Options = options
        };

        var embeddedImage = steg.Embed(_image.ToBytes(out var size), originalData);

        var outputPath = TestConstants.GetOutputImagePath(FileName, "LsbOut");
        embeddedImage.ToImage(size).Save(outputPath, new PngEncoder());

        using var imageTest = Image.Load<Rgba32>(outputPath);
        var extractedData = steg.Extract(imageTest.ToBytes(out _));

        Assert.Equal(originalData, extractedData);
    }
}