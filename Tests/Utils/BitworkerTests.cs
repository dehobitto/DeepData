using DeepData.Core.Utils;

namespace DeepData.Test.Utils;

public class BitWorkerTests
{
    [Fact]
    public void FromBytes_ShouldWriteAndReadBackCorrectly()
    {
        byte[] data = { 0xAB, 0xCD, 0xEF };
        var bw = BitWorker.CreateFromBytes(data);

        var readBack = bw.ReadBytes(data.Length * 8);
        Assert.Equal(data, readBack);
    }

    [Fact]
    public void WriteBitsFromByte_ShouldWriteCorrectBits()
    {
        var bw = new BitWorker(8);
        bw.WriteNLastBits(0b10101010, 8);
        bw.Restart();

        byte result = bw.ReadNBytes(8);
        Assert.Equal(0b10101010, result);
    }

    [Fact]
    public void Write_And_ReadBit_ShouldBeSymmetric()
    {
        var bw = new BitWorker(3);
        bw.Write(true);
        bw.Write(false);
        bw.Write(true);
        bw.Restart();

        Assert.True(bw.ReadBit());
        Assert.False(bw.ReadBit());
        Assert.True(bw.ReadBit());
    }

    [Fact]
    public void Write_And_ReadBits_ShouldMatch()
    {
        var bw = new BitWorker(5);
        bw.WriteNLastBits(0b11011, 5);
        bw.Restart();

        var result = bw.ReadNBytes(5);
        Assert.Equal(0b11011, result);
    }

    [Fact]
    public void Restart_ShouldResetBitIndex()
    {
        var bw = new BitWorker(1);
        bw.Write(true);
        bw.Restart();
        Assert.False(bw.IsEnded());
    }

    [Fact]
    public void IsEnded_ShouldReturnTrueWhenOutOfBounds()
    {
        var bw = new BitWorker(1);
        bw.Write(true);
        Assert.True(bw.IsEnded());
    }

    [Fact]
    public void ReadBit_ShouldThrow_WhenOutOfBounds()
    {
        var bw = new BitWorker(1);
        bw.Write(true);
        Assert.Throws<IndexOutOfRangeException>(() => bw.ReadBit());
    }

    [Fact]
    public void Write_ShouldThrow_WhenOutOfBounds()
    {
        var bw = new BitWorker(1);
        bw.Write(true);
        Assert.Throws<IndexOutOfRangeException>(() => bw.Write(true));
    }
}