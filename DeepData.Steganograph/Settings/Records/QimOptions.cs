namespace DeepData.Settings.Records;

public record QimOptions
{
    public QimOptions(){}

    public QimOptions(byte delta, ImageChannels channels)
    {
        Channels = channels;

        if (delta < 1 || delta > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "delta must be between 1 and 128.");
        }
        
        Delta = delta;
    }
    public byte Delta { get; init; } = StegoConstants.DefaultQimDelta;
    public ImageChannels Channels { get; init; } = StegoConstants.DefaultQimChannels;
}