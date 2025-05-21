namespace DeepData.Core.Utils.StegoSpecific;

public static class LsbHelper
{
    public static void ValidateLsbStrength(byte strength)
    {
        if (strength is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "LsbStrength must be in range [1, 8]");
        }
    }
    
    public static byte CreateBitMask(byte bitsPerByte)
    {
        return (byte)((1 << bitsPerByte) - 1);
    }
}