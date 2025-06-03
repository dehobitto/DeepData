using DeepData.CLI.Commands;
using DeepData.CLI.Models;

namespace DeepData.CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args[0] is "-h" or "--help")
            {
                return new HelpCommand().Execute();
            }
            
            if (args[0] is "-v" or "--version")
            {
                return new VersionCommand().Execute();
            }

            var commandArgs = new CommandArgs(args);
            var command = CreateCommand(commandArgs);
            
            return command.Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static ICommand CreateCommand(CommandArgs args)
    {
        return args.Command switch
        {
            Command.Embed => new EmbedCommand(args),
            Command.Extract => new ExtractCommand(args),
            Command.Capacity => new CapacityCommand(args),
            _ => throw new ArgumentException($"Unknown command: {args.Command}")
        };
    }
}