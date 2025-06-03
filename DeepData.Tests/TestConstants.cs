namespace DeepData.Test;

public static class TestConstants
{
    static TestConstants()
    {
        ProjectRoot = DirectionWorker.GetProjectRoot(ProjectName);
    }
    
    private const string ProjectName = "DeepData.CLI";
    private static string InitSolutionRoot()
    {
        return DirectionWorker.FindSolutionDirectory();
    }

    public static readonly Lazy<string> SolutionRoot = new(InitSolutionRoot);
    public static string ProjectRoot;
    
    public const byte TestQimDelta = 128;
    public const byte TestLsbStrength = 2;

    private static readonly Lazy<string> _testsRoot = new(() =>
        Path.Combine(SolutionRoot.Value, "DeepData.Tests"));

    private static readonly Lazy<string> _dataRoot = new(() =>
        Path.Combine(_testsRoot.Value, "Data"));

    public static (int R, int G, int B) TestChannels = (1, 1, 1);
    public static string DataRoot => _dataRoot.Value;

    public static string InputPath => Path.Combine(DataRoot, "Input");
    public static string OutputPath => Path.Combine(DataRoot, "Output");

    public static string GetInputImagePath(string fileNameWithoutExt)
    {
        return Path.Combine(InputPath, fileNameWithoutExt);
    }

    public static string GetOutputImagePath(string fileNameWithoutExt, string suffix)
    {
        return Path.Combine(OutputPath, fileNameWithoutExt + suffix);
    }
}