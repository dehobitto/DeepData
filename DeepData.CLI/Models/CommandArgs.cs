using DeepData.Settings;

namespace DeepData.CLI.Models;

public class CommandArgs
{
    private static readonly string[] SupportedImageFormats = Constants.SupportedImageFormats;
    private static readonly string[] LossyFormats = Constants.LossyFormats;

    public Command Command { get; }
    private Stego? Method { get; set; }
    public string? InputImagePath { get; private set; }
    public string? DataPath { get; private set; }
    public string? OutputPath { get; private set; }
    public Dictionary<string, string> Settings { get; } = new();

    public CommandArgs(string[] args)
    {
        ValidateInitialArguments(args);

        var currentArgIndex = 0;
        Command = ParseCommand(args[currentArgIndex++]);

        ParseRemainingArguments(args, ref currentArgIndex);
        ValidateParsedArguments();
        ValidateImageFormatAndMethod();
    }
    
    private static void ValidateInitialArguments(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("No arguments provided, use -h or --help");
        }
    }
    
    private static Command ParseCommand(string arg)
    {
        if (!Enum.TryParse<Command>(arg, true, out var command))
        {
            throw new ArgumentException($"Unknown command: {arg}");
        }
        return command;
    }
    
    private void ParseRemainingArguments(string[] args, ref int currentArgIndex)
    {
        while (currentArgIndex < args.Length)
        {
            var arg = args[currentArgIndex];
            
            if (arg.StartsWith("--"))
            {
                ParseOption(args, ref currentArgIndex);
            }
            else
            {
                ParsePositionalArgument(arg);
            }
            
            currentArgIndex++;
        }
    }
    
    private void ParseOption(string[] args, ref int currentArgIndex)
    {
        var arg = args[currentArgIndex];
        
        if (arg.StartsWith("--settings"))
        {
            ParseSettingsOption(arg, args, ref currentArgIndex);
        }
        else if (arg.StartsWith("--output"))
        {
            OutputPath = GetOptionValue(arg, args, ref currentArgIndex, "--output");
        }
        else if (arg == "--method")
        {
            ParseMethodOption(args, ref currentArgIndex);
        }
        else
        {
            throw new ArgumentException($"Unknown option: {arg}");
        }
        
    }
    
    private void ParseSettingsOption(string arg, string[] args, ref int currentArgIndex)
    {
        var settingsStr = GetOptionValue(arg, args, ref currentArgIndex, "--settings");
        ParseSettings(settingsStr);
    }
    
    private static string GetOptionValue(string arg, string[] args, ref int currentArgIndex, string optionName)
    {
        if (arg.Contains('='))
        {
            return arg.Split('=', 2)[1];
        }
        
        if (currentArgIndex + 1 < args.Length)
        {
            return args[++currentArgIndex];
        }
        else
        {
            throw new ArgumentException($"There is no value for option: {optionName}");
        }
    }
    
    private void ParseMethodOption(string[] args, ref int currentArgIndex)
    {
        var methodStr = args[++currentArgIndex];
        
        if (!Enum.TryParse<Stego>(methodStr, true, out var method))
        {
            throw new ArgumentException($"Unknown method: {methodStr}");
        }
        
        Method = method;
    }
    
    private void ParsePositionalArgument(string arg)
    {
        if (InputImagePath == null)
        {
            InputImagePath = arg;
        }
        else if (DataPath == null && Command == Command.Embed)
        {
            DataPath = arg;
        }
        else
        {
            throw new ArgumentException($"Unexpected argument: {arg}");
        }
    }
    
    private void ValidateParsedArguments()
    {
        if (InputImagePath == null)
        {
            throw new ArgumentException("You shall specify <image_path>.");
        }

        if (Command == Command.Embed && DataPath == null)
        {
            throw new ArgumentException("You shall specify <data_path> in order to use extract.");
        }

        if (Command == Command.Extract && OutputPath == null)
        {
            throw new ArgumentException("You shall specify <data_path> in order to use embed.");
        }
    }

    private void ValidateImageFormatAndMethod()
    { 
        var extension = Path.GetExtension(InputImagePath!).ToLowerInvariant();
        
        if (!SupportedImageFormats.Contains(extension))
        {
            throw new ArgumentException($"Unsupported image extension: {extension}. Supported: {string.Join(", ", SupportedImageFormats)}");
        }

        var method = GetStegoMethod();
        
        if (method == Stego.Lsb && LossyFormats.Contains(extension))
        {
            throw new ArgumentException($"Can't use LSB method with LossyFormats: ({string.Join(", ", LossyFormats)}). Use QIM method instead");
        }

        if (Settings.Count > 0 && Method == null)
        {
            throw new ArgumentException("You may want to specify method in order to specify settings.");
        }
    }

    private void ParseSettings(string settingsStr)
    {
        // Remove quotes if present
        settingsStr = settingsStr.Trim('"', '\'');
        
        var settings = settingsStr.Split(' ');
        
        foreach (var setting in settings)
        {
            if (string.IsNullOrWhiteSpace(setting)) continue;
            
            var parts = setting.Split('=', 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Wrong settings format: {setting}. 'key=value' expected.");
            }
            Settings[parts[0]] = parts[1];
        }
    }
    
    public Stego GetStegoMethod()
    {
        if (Method.HasValue)
        {
            return Method.Value;
        }

        var extension = Path.GetExtension(InputImagePath!).ToLowerInvariant();

        if (LossyFormats.Contains(extension))
        {
            return Stego.Dct;
        }
        else
        {
            return Stego.Lsb;
        }
    }
}