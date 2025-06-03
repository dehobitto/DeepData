using DeepData.CLI.Models;
using DeepData.Settings;
using DeepData.Settings.Records;

namespace DeepData.CLI.Utils;

public static class SettingsBuilder
{
    public static Options BuildOptions(CommandArgs args)
    {
        var options = new Options();
        var extension = Path.GetExtension(args.InputImagePath!).ToLowerInvariant();
        var isLossy = Constants.LossyFormats.Contains(extension);

        switch (args.GetStegoMethod())
        {
            case Stego.Qim:
                return BuildQimOptions(args.Settings, isLossy);
            case Stego.Lsb:
                return BuildLsbOptions(args.Settings);
            default:
                return options;
        }
    }

    private static Options BuildQimOptions(Dictionary<string, string> settings, bool isLossy)
    {
        var jpegChannels = StegoConstants.DefaultJpegChannels;
        var imageChannels = StegoConstants.DefaultQimChannels;
        var blocks = StegoConstants.DefaultJpegBlocksCount;
        var delta = StegoConstants.DefaultQimDelta;

        if (settings.TryGetValue("channels", out var channelsStr))
        {
            var channels = channelsStr.Split(',')
                .Select(c => c.Trim().ToUpper())
                .ToList();

            if (isLossy)
            {
                jpegChannels = channels.Aggregate(
                    JpegChannels.All,
                    (current, channel) => current & ~GetJpegChannel(channel)
                );
            }
            else
            {
                imageChannels = channels.Aggregate(
                    ImageChannels.All,
                    (current, channel) => current & ~GetImageChannel(channel)
                );
            }
        }

        if (settings.TryGetValue("blocks", out var blocksStr) && int.TryParse(blocksStr, out var blocksValue))
        {
            blocks = blocksValue;
        }

        if (settings.TryGetValue("delta", out var deltaStr) && byte.TryParse(deltaStr, out var deltaValue))
        {
            delta = deltaValue;
        }

        return new Options
        {
            Jpeg = new JpegOptions(jpegChannels, blocks),
            Qim = new QimOptions(delta, imageChannels)
        };
    }

    private static Options BuildLsbOptions(Dictionary<string, string> settings)
    {
        var channels = StegoConstants.DefaultQimChannels;
        var strength = StegoConstants.DefaultLsbStrength;

        if (settings.TryGetValue("channels", out var channelsStr))
        {
            channels = channelsStr.Split(',')
                .Select(c => c.Trim().ToUpper())
                .Aggregate(ImageChannels.All, (current, channel) => current & ~GetImageChannel(channel));
        }

        if (settings.TryGetValue("strength", out var strengthStr) && byte.TryParse(strengthStr, out var strengthValue))
        {
            strength = strengthValue;
        }

        return new Options
        {
            Qim = new QimOptions(StegoConstants.DefaultQimDelta, channels),
            Lsb = new LsbOptions(strength)
        };
    }

    private static JpegChannels GetJpegChannel(string channel)
    {
        return channel switch
        {
            "Y" => JpegChannels.Y,
            "CB" => JpegChannels.Cb,
            "CR" => JpegChannels.Cr,
            _ => throw new ArgumentException($"Unknown JPEG channel: {channel}")
        };
    }

    private static ImageChannels GetImageChannel(string channel)
    {
        return channel switch
        {
            "R" => ImageChannels.R,
            "G" => ImageChannels.G,
            "B" => ImageChannels.B,
            _ => throw new ArgumentException($"Unknown image channel: {channel}")
        };
    }
} 