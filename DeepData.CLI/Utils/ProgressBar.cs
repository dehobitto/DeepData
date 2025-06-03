using DeepData.Interfaces;

namespace DeepData.CLI.Utils;

public class ProgressBar(int width = 30) : IProgress
{
    private int _lastPercentage = -1;

    public void Update(int current, int total)
    {
        var percentage = (int)((float)current / total * 100);
        if (percentage == _lastPercentage) return;
        
        _lastPercentage = percentage;
        var progress = (int)((float)current / total * width);
        
        Console.Write("\r[");
        Console.Write(new string('=', progress));
        Console.Write(new string(' ', width - progress));
        Console.Write($"] {percentage}%");
    }

    public void Complete()
    {
        Console.Write("\r[");
        Console.Write(new string('=', width));
        Console.WriteLine($"] 100%");
    }
} 