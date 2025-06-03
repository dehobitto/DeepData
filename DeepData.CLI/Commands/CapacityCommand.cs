using DeepData.CLI.Models;

namespace DeepData.CLI.Commands;

public class CapacityCommand(CommandArgs args) : BaseCommand(args)
{
    public override int Execute()
    {
        try
        {
            Console.WriteLine($"Source capacity: {FormatSize(GetCapacity())}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
} 