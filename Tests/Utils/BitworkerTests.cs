using DeepData.Core;
using DeepData.Core.Utils;

namespace DeepData.Test.Utils;

public class BitWorkerTests
{
    [Fact]
    public void Constructor_SetsCapacityAndInitialPosition()
    {
        var worker = new BitWorker(16);
        Assert.Equal(16, worker.CapacityBits);
        Assert.Equal(0, worker.CurrentPositionBits);
    }

    [Fact]
    public void WriteAndReadSingleBit()
    {
        var worker = new BitWorker(1);
        worker.WriteBit(true);
        worker.ResetPosition();
        Assert.True(worker.ReadBit());
    }

    [Fact]
    public void WriteAndReadByte()
    {
        var worker = new BitWorker(8);
        byte value = 0b10101010;
        worker.WriteByte(value);
        worker.ResetPosition();
        Assert.Equal(value, worker.ReadByte());
    }

    [Fact]
    public void WriteAndReadUInt32()
    {
        var worker = new BitWorker(32);
        uint value = 0xDEADBEEF;
        worker.WriteUInt32(value, 32);
        worker.ResetPosition();
        Assert.Equal(value, worker.ReadUInt32(32));
    }

    [Fact]
    public void WriteBitsFromByte_WritesCorrectBits()
    {
        var worker = new BitWorker(8);
        byte value = 0b10101010;
        worker.WriteBitsFromByte(value, 8);
        worker.ResetPosition();
    
        Assert.True(worker.ReadBit()); // bit 7
        Assert.False(worker.ReadBit()); // bit 6
        Assert.True(worker.ReadBit()); // bit 5
        Assert.False(worker.ReadBit()); // bit 4
        Assert.True(worker.ReadBit()); // bit 3
        Assert.False(worker.ReadBit()); // bit 2
        Assert.True(worker.ReadBit()); // bit 1
        Assert.False(worker.ReadBit()); // bit 0
    }


    [Fact]
    public void CreateWithHeaderFromBytes_EncodesAndDecodes()
    {
        byte[] data = { 1, 2, 3, 4 };
        var worker = BitWorker.CreateWithHeaderFromBytes(data);
        byte[] result = worker.ReadBytesWithHeader();
        Assert.Equal(data, result);
    }

    [Fact]
    public void WriteBytes_And_ReadBytes_Correctly()
    {
        byte[] data = { 42, 255, 0 };
        var worker = new BitWorker(data.Length * 8);
        worker.WriteBytes(data);
        worker.ResetPosition();
        foreach (var expected in data)
        {
            Assert.Equal(expected, worker.ReadByte());
        }
    }

    [Fact]
    public void ReadBytesWithHeader_InvalidData_Throws()
    {
        byte[] data = { 1, 2, 3 };
        var worker = BitWorker.CreateWithHeaderFromBytes(data);
        // Manually tamper with internal position to simulate corrupt header
        var tampered = new BitWorker(worker.CapacityBits);
        tampered.WriteUInt32(99999999, Constants.HeaderBits);
        Assert.Throws<InvalidDataException>(() => tampered.ReadBytesWithHeader());
    }

    [Fact]
    public void ReadBit_AtEnd_Throws()
    {
        var worker = new BitWorker(1);
        worker.WriteBit(true);
        Assert.Throws<IndexOutOfRangeException>(() => worker.ReadBit());
    }

    [Fact]
    public void WriteBit_AtEnd_Throws()
    {
        var worker = new BitWorker(1);
        worker.WriteBit(true);
        Assert.Throws<IndexOutOfRangeException>(() => worker.WriteBit(false));
    }
}