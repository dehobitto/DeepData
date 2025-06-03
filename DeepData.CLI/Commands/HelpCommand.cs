namespace DeepData.CLI.Commands;

public class HelpCommand : ICommand
{
    public int Execute()
    {
        Console.WriteLine(Constants.HelpText);
        
        return 0;
    }
} 