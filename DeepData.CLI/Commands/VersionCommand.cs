namespace DeepData.CLI.Commands;

public class VersionCommand : ICommand
{
    public int Execute()
    {
        Console.WriteLine($"DeepData by Handro.\nv{Constants.Version}");
        
        return 0;
    }
}