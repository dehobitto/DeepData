namespace DeepData.Stego;

public static class Constants
{
    public const byte DefaultQimDelta = 16;
    public const byte DefaultLsbStrength = 1;
    public static readonly (int R, int G, int B) DefaultChannels = (1, 1, 1);
    public const int HeaderBits = 32;
    
    // its most likely to be the power of 2 the other way it wont work most of the times
    public const byte TestQimDelta = 128;
    public static (int R, int G, int B) TestChannels = (1, 1, 1);
    
    public const string TestImageInputPath = "../../../../Steganograph/Source/Data/Test/In/";
    public const string TestImageOutputPath = "../../../../Steganograph/Source/Data/Test/Out/";
    
    public const string DefaultImageOutputPath = "./Source/Data/Output/";
    public const byte TestLsbStrength = 2;

    public const string DefaultFileName = "result";
}