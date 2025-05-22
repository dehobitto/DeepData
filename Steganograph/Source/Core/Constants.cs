using DeepData.Core.Utils;

namespace DeepData.Core;

using static DirectionWorker;

public static class Constants
{
    private const string ProjectName = "Steganograph";

    public static readonly Lazy<string> SolutionRoot = new (InitSolutionRoot);
    public static string ProjectRoot { get; }
    public static string DefaultImageOutputPath { get; }

    public const string DefaultFileName = "result";
    public const byte DefaultQimDelta = 4;
    public const byte DefaultLsbStrength = 2;
    public static readonly (int R, int G, int B) DefaultChannels = (1, 1, 1);
    public static readonly (int Y, int Cb, int Cr) DefaultChannelsJpeg = (1, 1, 1);

    public const int HeaderBits = 32;

    static Constants()
    {
        ProjectRoot = GetProjectRoot(ProjectName);
        DefaultImageOutputPath = Path.Combine(ProjectRoot, "Output");
        Directory.CreateDirectory(DefaultImageOutputPath);
    }

    public static readonly int JpegChannelsCount = 3;
    public static readonly int JpegBlocksCount = 64;
    public static readonly int HeaderBytes = HeaderBits / 8;
}