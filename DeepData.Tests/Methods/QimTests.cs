using DeepData.Methods;
using DeepData.Settings;
using DeepData.Settings.Records;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Test.Methods;

public class QimTests
{
    private const string FileName = "test.png";
    private readonly Image<Rgba32> _image = Image.Load<Rgba32>(TestConstants.GetInputImagePath(FileName));

    [Fact]
    public void Qim_Embed_Extract_NoSave()
    {
        var originalData = "Hello, DeepData.Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            Qim = new QimOptions
            {
                Delta = TestConstants.TestQimDelta
            }
        };

        var steg = new Qim(options);

        var embeddedImage = steg.Embed(_image, originalData);
        var extractedData = steg.Extract(embeddedImage);

        Assert.Equal(originalData, extractedData);
    }

    [Fact]
    public void Qim_Embed_Extract_Save()
    {
        var originalData =
            "Hello, DeepData.Steganograph! 1233142 34d fg dfsg dr65 45w3 ht4e5  dfghdfghdhfghdfgklmdsjfgmpsvo9e4u yt93v8"u8
                .ToArray();

        var options = new Options
        {
            Qim = new QimOptions
            {
                Delta = TestConstants.TestQimDelta
            }
        };

        var steg = new Qim(options);

        var embeddedImage = steg.Embed(_image, originalData);

        var outputPath = TestConstants.GetOutputImagePath(FileName, "QimOut.png");
        embeddedImage.Save(outputPath, new PngEncoder());

        using var imageLoad = Image.Load<Rgba32>(outputPath);
        var extractedData = steg.Extract(imageLoad);

        Assert.Equal(originalData, extractedData);
    }
}