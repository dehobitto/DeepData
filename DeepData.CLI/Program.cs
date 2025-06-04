using DeepData.CLI.Commands;
using DeepData.CLI.Utils;

namespace DeepData.CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("deepdata -h for help");
                return 1;
            }
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
            Console.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static ICommand CreateCommand(CommandArgs args)
    {
        return args.Command switch
        {
            Command.Embed => new EmbedCommand(args),
            Command.Extract => new ExtractCommand(args),
            Command.Commands => new CapacityCommand(args),
            _ => throw new ArgumentException($"unknown command: {args.Command}")
        };
    }
}