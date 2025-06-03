using DeepData.Utils.StegoSpecific;

namespace DeepData.Test.Utils.StegoSpecific;

public class QimHelperTests
{
    [Theory]
    [InlineData(100, true, 10)]
    [InlineData(150, false, 20)]
    [InlineData(255, true, 15)]
    [InlineData(0, false, 5)]
    public void QimEmbedBit_ShouldProduceValueWithinRange(byte orig, bool bit, int delta)
    {
        var embedded = QimHelper.QimEmbedBit(orig, bit, delta);
        Assert.InRange(embedded, (byte)0, (byte)255);
    }

    [Theory]
    [InlineData(130, 10, false)]
    [InlineData(125, 10, true)]
    [InlineData(200, 20, false)]
    [InlineData(210, 20, true)]
    [InlineData(100, 20, false)]
    public void QimExtractBit_ShouldExtractCorrectBit(byte val, int delta, bool expected)
    {
        var bit = QimHelper.QimExtractBit(val, delta);
        Assert.Equal(expected, bit);
    }

    [Theory]
    [InlineData(3, 2, true)]
    [InlineData(3, 1, false)]
    [InlineData(0, 1, false)]
    [InlineData(4, 2, false)]
    [InlineData(4, 3, true)]
    public void Vote_ShouldReturnCorrectResult(int activeChannels, int bitCount, bool expected)
    {
        var result = QimHelper.Vote(activeChannels, bitCount);
        Assert.Equal(expected, result);
    }
}