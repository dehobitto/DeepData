using DeepData.Methods;
using DeepData.Settings;
using DeepData.Settings.Records;

namespace DeepData.Test.Methods;

public class DctTests
{
    private const string FileName = "test.jpg";

    [Fact]
    public void DctJpegTests_Embed_Extract_NoSave()
    {
        var opt = new Options
        {
            Jpeg = new JpegOptions()
        };

        var steg = new Dct(opt);
        var fs = File.OpenRead(TestConstants.GetInputImagePath(FileName));
        var data = "Data hidden"u8.ToArray();

        var embededStream = steg.Embed(fs, data);

        var extracted = steg.Extract(embededStream);

        Assert.Equal(data, extracted);
    }

    [Fact]
    public void DctJpegTests_Embed_Extract_Save()
    {
        var outputPath = TestConstants.GetOutputImagePath(FileName, "DctJpeg.jpg");

        var opt = new Options
        {
            Jpeg = new JpegOptions()
        };

        var steg = new Dct(opt);
        var fs = File.OpenRead(TestConstants.GetInputImagePath(FileName));
        var data = "Data hidden"u8.ToArray();

        var embededStream = steg.Embed(fs, data);

        var outputFs = File.OpenWrite(outputPath);
        embededStream.CopyTo(outputFs);
        outputFs.Close();
        embededStream.Close();

        var fss = File.OpenRead(outputPath);

        var extracted = steg.Extract(fss);

        Assert.Equal(data, extracted);
    }
}