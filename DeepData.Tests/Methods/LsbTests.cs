using DeepData.Extensions;
using DeepData.Methods;
using DeepData.Settings;
using DeepData.Settings.Records;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Test.Methods;

public class LsbTests
{
    private const string FileName = "test.png";
    private readonly Image<Rgba32> _image = Image.Load<Rgba32>(TestConstants.GetInputImagePath(FileName));

    [Fact]
    public void Lsb_Embed_Extract_NoSave()
    {
        var originalData = "Hello, DeepData.Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            Lsb = new LsbOptions
            {
                Strength = TestConstants.TestLsbStrength
            }
        };

        var steg = new Lsb(options);

        var embeddedImage = steg.Embed(_image.ToBytes(out _), originalData);
        var extractedData = steg.Extract(embeddedImage);

        Assert.Equal(originalData, extractedData);
    }

    [Fact]
    public void Lsb_Embed_Extract_Save()
    {
        var originalData = "Hdfklgjdflkgjdf;lkgj hello world"u8.ToArray();

        var options = new Options
        {
            Lsb = new LsbOptions
            {
                Strength = TestConstants.TestLsbStrength
            }
        };

        var steg = new Lsb(options);

        var embeddedImage = steg.Embed(_image.ToBytes(out var size), originalData);

        var outputPath = TestConstants.GetOutputImagePath(FileName, "LsbOut.png");
        embeddedImage.ToImage(size).Save(outputPath, new PngEncoder());

        using var imageTest = Image.Load<Rgba32>(outputPath);
        var extractedData = steg.Extract(imageTest.ToBytes(out _));

        Assert.Equal(originalData, extractedData);
    }
}