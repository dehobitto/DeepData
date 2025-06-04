using DeepData.Settings.Records;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace DeepData.Settings;

public record Options
{
    public LsbOptions Lsb { get; init; } = new();
    public QimOptions Qim { get; init; } = new();
    public JpegOptions Jpeg { get; init; } = new();
}