using DeepData.Interfaces;

namespace DeepData.CLI.Utils;

public class ProgressBar(int width = 30) : IProgress
{
    private int _lastPercentage = -1;

    public void Update(int current, long total)
    {
        int percentage = (int)((float)current / total * 100);
        
        if (percentage == _lastPercentage)
        {
            return;
        }
        
        _lastPercentage = percentage;
        
        int progress = (int)((float)current / total * width);
        
        progress = Math.Min(progress, width); 
        percentage = Math.Min(percentage, 100);
        
        Console.Write($"\r{AsciiChars.Left}");
        Console.Write(new string(AsciiChars.ProgressChar, progress));
        Console.Write(new string(AsciiChars.Empty, width - progress));
        Console.Write($"{AsciiChars.Right} {percentage}%");
    }

    public void Complete()
    {
        Console.Write($"\r{AsciiChars.Left}");
        Console.Write(new string(AsciiChars.ProgressChar, width));
        Console.Write($"{AsciiChars.Right} 100%\n");
    }
}

public record AsciiChars
{
    public static readonly char Left = '[';
    public static readonly char Right = ']';
    public static readonly char ProgressChar = '=';
    public static readonly char Empty = ' ';
}