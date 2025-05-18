using DeepData.Stego;
using DeepData.Stego.Methods;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData.Test;

public class QimTests
{
    private const string FileName = "test1";
    private readonly Image<Rgba32> _image = Image.Load<Rgba32>(Constants.TestImageInputPath + FileName + ".png");
    [Fact]
    public void Qim_Embed_Extract_NoSave()
    {
        var steg = new Qim();
        byte[] originalData = "Hello, Steganograph 12345!"u8.ToArray();

        var options = new Options
        {
            QimDelta = Constants.TestQimDelta,
            Channels = Constants.TestChannels
        };

        var embeddedImage = steg.Embed(_image, originalData, options);
        var extractedData = steg.Extract(embeddedImage, options);

        Assert.Equal(originalData, extractedData);
    }
    
    [Fact]
    public void Qim_Embed_Extract_Save()
    {
        var steg = new Qim();
        
        byte[] originalData = "Hello, Steganograph! 1233142 34d fg dfsg dr65 45w3 ht4e5  dfghdfghdhfghdfgklmdsjfgmpsvo9e4u yt93v8"u8.ToArray();

        var options = new Options
        {
            QimDelta = Constants.TestQimDelta,
            Channels = Constants.TestChannels
        };
        
        var embeddedImage = steg.Embed(_image, originalData, options);
        embeddedImage.Save(Constants.TestImageOutputPath + FileName + "QimOut.png", new PngEncoder());
        
        using var imageLoad = Image.Load<Rgba32>(Constants.TestImageOutputPath + FileName + "QimOut.png");
        var extractedData = steg.Extract(imageLoad, options);
        
        Assert.Equal(originalData, extractedData);
    }
}