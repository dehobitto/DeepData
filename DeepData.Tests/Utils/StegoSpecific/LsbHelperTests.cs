using DeepData.Utils.StegoSpecific;

namespace DeepData.Test.Utils.StegoSpecific;

public class LsbHelperTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CreateBitMask(byte bitsPerByte)
    {
        var bitmask = LsbHelper.CreateBitMask(bitsPerByte);
        var expected = Math.Pow(2, bitsPerByte) - 1;
        Assert.Equal(expected, bitmask);
    }
}