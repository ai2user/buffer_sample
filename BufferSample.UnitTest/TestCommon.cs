using BufferSample.Lib.Buffers;

namespace BufferSemple.UnitTest;

public class TestCommon
{
    [Fact]
    public async Task TestHappy()
    {
        var buffer = new CommonBufferQueue<int, long>();
        var bufferOptions = new BufferOptions();
        await buffer.EnqueueAsync(1, 1, bufferOptions);
        await buffer.EnqueueAsync(2, 1, bufferOptions);
        await buffer.EnqueueAsync(1, 1, bufferOptions);

        var buffer1 = await buffer.DequeueAsync();
        Assert.Equal(3, buffer1.Items.Count);
        Assert.Equal(BufferType.Common, buffer1.BufferType);
    }
}