using DeepData.CLI.Utils;
using DeepData.Extensions;
using DeepData.Methods;
using DeepData.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using CommandArgs = DeepData.CLI.Models.CommandArgs;

namespace DeepData.CLI.Commands;

public abstract class BaseCommand(CommandArgs args) : ICommand
{
    protected readonly CommandArgs Args = args;
    protected readonly Options Options = SettingsBuilder.BuildOptions(args);
    protected readonly Image<Rgba32> InputImage = Image.Load<Rgba32>(args.InputImagePath!);

    public abstract int Execute();

    protected string FormatSize(int bytes)
    {
        return $"{bytes / 1024.0 / 1024.0:F2} MB";
    }

    protected int GetCapacity()
    {
        switch (Args.GetStegoMethod())
        {
            case Stego.Qim:
            {
                var qim = new Qim(Options);
                return qim.GetCapacityBytes(InputImage);
            }
            case Stego.Lsb:
            {
                var lsb = new Lsb(Options);
                var bytes = InputImage.ToBytes(out _);
                return lsb.GetCapacityBytes(bytes);
            }
            case Stego.Dct:
            {
                var dct = new Dct(Options);
                
                using var jpegStream = new MemoryStream();
                InputImage.Save(jpegStream, new JpegEncoder());
                jpegStream.Position = 0;
                
                return dct.GetCapacityBytes(jpegStream);
            }
            default: 
            {
                throw new ArgumentException("Unsupported method.");
            }
        }
    }
} 