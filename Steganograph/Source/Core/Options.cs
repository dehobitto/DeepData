using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace DeepData.Core;

public record Options
{
    /// <summary>
    /// LsbStrength affects how much bits in bytes will be affected,
    /// it supposed to be 1 or 2, anything different will be noticable and useless
    ///
    /// QimDelta affects the strength of method (higher == more strong) but it gets noticable too
    ///
    /// Channels is used in QIM, so it will be duplicated it different channels, so the method is more time consuming but much stronger to filters
    /// </summary>
    public byte QimDelta { get; init; } = Constants.DefaultQimDelta;
    public byte LsbStrength { get; init; } = Constants.DefaultLsbStrength; 
    public (int R, int G, int B) Channels { get; init; } = Constants.DefaultChannels;
    public (int Y, int Cb, int Cr) ChannelsJpeg = Constants.DefaultChannelsJpeg;
    public string FileName { get; init; } = Constants.DefaultFileName;
    public ImageEncoder Encoder { get; init; } = new PngEncoder();
}