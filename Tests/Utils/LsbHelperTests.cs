using DeepData.Core.Utils;
using DeepData.Core.Utils.StegoSpecific;

namespace DeepData.Test.Utils;

public class LsbHelperTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public static void ValidateLsbStrength_ShouldGiveException(byte strength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LsbHelper.ValidateLsbStrength(strength));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CreateBitMask(byte bitsPerByte)
    {
        byte bitmask = LsbHelper.CreateBitMask(bitsPerByte);
        double expected = Math.Pow(2, bitsPerByte) - 1;
        Assert.Equal(expected, bitmask);
    }
}