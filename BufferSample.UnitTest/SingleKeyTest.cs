using BufferSample.Lib.Buffers;

namespace BufferSemple.UnitTest;

public class SingleKeyTest
{
    [Fact]
    public async Task TestHappy()
    {
        var buffer = new SingleKeyBufferQueue<int, long>();
        var bufferOptions = new BufferOptions();
        await buffer.EnqueueAsync(1, 1, bufferOptions);
        await buffer.EnqueueAsync(2, 1, bufferOptions);
        await buffer.EnqueueAsync(1, 1, bufferOptions);

        var buffer1 = await buffer.DequeueAsync();
        var buffer2 = await buffer.DequeueAsync();
        var buffers = new[] { buffer1, buffer2 };
        foreach (var buf in buffers)
        {
            if (buf.Key == 1)
            {
                Assert.Equal(2, buf.Items.Count);
                Assert.All(buf.Items, q => Assert.Equal(1, q));
            }
            else
            {
                Assert.Single(buf.Items);
                Assert.Equal(2, buf.Key);
            }
        }
    }
}