namespace DeepData.Settings.Records;

public record JpegOptions
{
    public JpegOptions() {}

    public JpegOptions(JpegChannels channels, int blocks, int delta)
    {
        Channels = channels;

        if (blocks < 1 || blocks > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(blocks), "blocks count must be between 1 and 64.");
        }

        Blocks = blocks;
        Delta = delta;
    }

    public JpegChannels Channels { get; init; } = StegoConstants.DefaultJpegChannels;
    public int Blocks { get; init; } = StegoConstants.DefaultJpegBlocksCount;
    public int Delta { get; init; } = StegoConstants.DefaultQimDelta;
}