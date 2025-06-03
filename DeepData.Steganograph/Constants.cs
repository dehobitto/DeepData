using DeepData.Settings;

namespace DeepData;

public static class StegoConstants
{
    public const int HeaderBits = 32;

    public const byte DefaultLsbStrength = 2;

    public const byte DefaultQimDelta = 4;
    public const ImageChannels DefaultQimChannels = ImageChannels.All;

    public const int DefaultJpegBlocksCount = 64;
    public const JpegChannels DefaultJpegChannels = JpegChannels.All;
}