using DeepData.CLI.Utils;
using DeepData.Extensions;
using DeepData.Methods;
using DeepData.Settings;
using CommandArgs = DeepData.CLI.Models.CommandArgs;

namespace DeepData.CLI.Commands;

public class ExtractCommand(CommandArgs args) : BaseCommand(args)
{
    public override int Execute()
    {
        try
        {
            byte[] data;

            Console.WriteLine($"Source capacity: {FormatSize(GetCapacity())}");

            var progressBar = new ProgressBar();

            switch (Args.GetStegoMethod())
            {
                case Stego.Qim:
                    var qim = new Qim(Options);
                    
                    qim.SetProgress(progressBar);
                    data = qim.Extract(InputImage);
                    
                    break;
                case Stego.Lsb:
                    var lsb = new Lsb(Options);
                    
                    lsb.SetProgress(progressBar);
                    data = lsb.Extract(InputImage.ToBytes(out _));
                    
                    break;
                default:
                    throw new ArgumentException("Unsupported method");
            }

            File.WriteAllBytes(Args.OutputPath!, data);
            
            progressBar.Complete();
            
            Console.WriteLine($"Extracted data size: {FormatSize(data.Length)}");
            Console.WriteLine("Done successful");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
} 