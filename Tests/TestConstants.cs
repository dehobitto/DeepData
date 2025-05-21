using DeepData.Core;

namespace DeepData.Test;

public static class TestConstants
{
    private static readonly Lazy<string> _testsRoot = new(() =>
        Path.Combine(Constants.SolutionRoot.Value, "Tests"));

    private static readonly Lazy<string> _dataRoot = new(() =>
        Path.Combine(_testsRoot.Value, "Data"));
    public static string DataRoot => _dataRoot.Value;

    public static string InputPath => Path.Combine(DataRoot, "Input");
    public static string OutputPath => Path.Combine(DataRoot, "Output");

    public static string GetInputImagePath(string fileNameWithoutExt)
        => Path.Combine(InputPath, fileNameWithoutExt + ".png");

    public static string GetOutputImagePath(string fileNameWithoutExt, string suffix)
        => Path.Combine(OutputPath, fileNameWithoutExt + suffix + ".png");

    public const byte TestQimDelta = 128;
    public static (int R, int G, int B) TestChannels = (1, 1, 1);
    public const byte TestLsbStrength = 2;
}