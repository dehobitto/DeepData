using DeepData.Core.Utils;

namespace DeepData.Core;

using static DirectionWorker;

public static class Constants
{
    // Ленивая инициализация: обход папок вверх делается только один раз
    public static readonly Lazy<string> SolutionRoot = new (InitSolutionRoot);

    /// <summary>
    /// ROOT конкретного проекта внутри решения
    /// </summary>
    public static string ProjectRoot => GetProjectRoot("Steganograph");

    /// <summary>
    /// Папка для выходных изображений
    /// </summary>
    public static string DefaultImageOutputPath { get; }

    public const string DefaultFileName = "result";
    
    public const byte DefaultQimDelta = 64;
    public const byte DefaultLsbStrength = 2;
    public static readonly (int R, int G, int B) DefaultChannels = (1, 1, 1);
    public static readonly (int Y, int Cb, int Cr) DefaultChannelsJpeg = (1, 1, 1);
    
    public const int HeaderBits = 32;

    // Статический конструктор для создания папок
    static Constants()
    {
        DefaultImageOutputPath = Path.Combine(ProjectRoot, "Output");
        Directory.CreateDirectory(DefaultImageOutputPath);
    }

    public static readonly int JpegChannelsCount = 3;
    public static readonly int JpegBlocksCount = 32;
    public static readonly int HeaderBytes = HeaderBits / 8;
}
