using DeepData.Stego.Utils;

namespace DeepData.Stego;

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
    public const byte DefaultQimDelta = 16;
    public const byte DefaultLsbStrength = 1;
    public static readonly (int R, int G, int B) DefaultChannels = (1, 1, 1);
    public const int HeaderBits = 32;

    // Статический конструктор для создания папок
    static Constants()
    {
        DefaultImageOutputPath = Path.Combine(ProjectRoot, "Output");
        Directory.CreateDirectory(DefaultImageOutputPath);
    }
}
