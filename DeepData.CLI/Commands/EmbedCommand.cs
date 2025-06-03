using DeepData.CLI.Utils;
using DeepData.Extensions;
using DeepData.Methods;
using DeepData.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using CommandArgs = DeepData.CLI.Models.CommandArgs;

namespace DeepData.CLI.Commands;

public class EmbedCommand(CommandArgs args) : BaseCommand(args)
{
    public override int Execute()
    {
        try
        {
            var data = File.ReadAllBytes(Args.DataPath!);
            var outputPath = Args.OutputPath ?? $"output_{Path.GetFileName(Args.InputImagePath)}";

            Console.WriteLine($"Source capacity: {FormatSize(GetCapacity())}");
            Console.WriteLine($"Data size: {FormatSize(data.Length)}");

            var progressBar = new ProgressBar();

            switch (Args.GetStegoMethod())
            {
                case Stego.Qim:
                    var qim = new Qim(Options);
                    
                    qim.SetProgress(progressBar);
                    
                    var qimResult = qim.Embed(InputImage, data);
                    qimResult.Save(outputPath, GetEncoder(outputPath));
                    
                    break;
                case Stego.Lsb:
                    var lsb = new Lsb(Options);
                    
                    lsb.SetProgress(progressBar);
                    
                    byte[] lsbResult = lsb.Embed(InputImage.ToBytes(out var size), data);
                    lsbResult.ToImage(size).Save(outputPath, GetEncoder(outputPath));
                    
                    break;
            }

            progressBar.Complete();
            
            Console.WriteLine($"Done successful. Output: {outputPath}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static IImageEncoder GetEncoder(string path)
    {
        return Path.GetExtension(path).ToLower() switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder(),
            ".png" => new PngEncoder(),
            ".bmp" => new BmpEncoder(),
            _ => throw new ArgumentException($"Unsupported output format: {Path.GetExtension(path)}")
        };
    }
} 