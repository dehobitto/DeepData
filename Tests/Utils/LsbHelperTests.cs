namespace DeepData.Test.Utils;

public class LsbHelper
{
    [Theory]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(255)]
    [InlineData(0)]
    public void ValidateLsbStrength_Should_Give_Exception(byte strength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(ValidateLsbStrength);
    }

    [Theory]
    [InlineData(130, 10, false)]
    [InlineData(125, 10, true)]
    [InlineData(200, 20, false)]
    [InlineData(210, 20, true)]
    [InlineData(100, 20, false)]
    public void CreateBitMask(byte val, int delta, bool expected)
    {
        var bit = QimHelper.QimExtractBit(val, delta);
        Assert.Equal(expected, bit);
    }
}