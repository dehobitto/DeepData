using DeepData.Settings;
using DeepData.Settings.Records;

namespace DeepData.CLI.Utils;

public static class SettingsBuilder
{
    public static Options BuildOptions(CommandArgs args)
    {
        var options = new Options();

        switch (args.GetStegoMethod())
        {
            case Stego.Qim:
            {
                return BuildQimOptions(args.Settings);
            }
            case Stego.Lsb:
            {
                return BuildLsbOptions(args.Settings);
            }
            case Stego.Dct:
            {
                return BuildDctOptions(args.Settings);
            }
            default:
            {
                return options;
            }
        }
    }

    private static Options BuildDctOptions(Dictionary<string, string> settings)
    {
        var jpegChannels = StegoConstants.DefaultJpegChannels;
        var blocks = StegoConstants.DefaultJpegBlocksCount;
        var delta = StegoConstants.DefaultQimDelta;
        
        if (settings.TryGetValue("channels", out var channelsStr))
        {
            var channels = channelsStr.Split(',')
                .Select(c => c.Trim().ToUpper())
                .ToList();
            
            jpegChannels = JpegChannels.None;

            foreach (var channelName in channels)
            {
                jpegChannels |= GetJpegChannel(channelName);
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
            Jpeg = new JpegOptions(jpegChannels, blocks, delta)
        };
    }

    private static Options BuildQimOptions(Dictionary<string, string> settings)
    {
        var imageChannels = StegoConstants.DefaultQimChannels;
        var delta = StegoConstants.DefaultQimDelta;

        if (settings.TryGetValue("channels", out var channelsStr))
        {
            var channels = channelsStr.Split(',')
                .Select(c => c.Trim().ToUpper())
                .ToList();
            
            imageChannels = ImageChannels.None;

            foreach (var channelName in channels)
            {
                imageChannels |= GetImageChannel(channelName);
            }
        }

        if (settings.TryGetValue("delta", out var deltaStr) && byte.TryParse(deltaStr, out var deltaValue))
        {
            delta = deltaValue;
        }

        return new Options
        {
            Qim = new QimOptions(delta, imageChannels)
        };
    }

    private static Options BuildLsbOptions(Dictionary<string, string> settings)
    {
        var strength = StegoConstants.DefaultLsbStrength;

        if (settings.TryGetValue("strength", out var strengthStr) && byte.TryParse(strengthStr, out var strengthValue))
        {
            strength = strengthValue;
        }

        return new Options
        {
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