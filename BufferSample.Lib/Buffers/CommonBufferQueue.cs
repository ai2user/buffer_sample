using System.Threading.Channels;

namespace BufferSample.Lib.Buffers;

public class CommonBufferQueue<TKey, TPayload>
{
    private readonly Channel<Buffer<TKey, TPayload>> _buffersQueue =
        Channel.CreateUnbounded<Buffer<TKey, TPayload>>(new UnboundedChannelOptions() { SingleReader = true });

    private volatile Buffer<TKey, TPayload> _commonBuffer = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public async ValueTask EnqueueAsync(TKey key, TPayload item, BufferOptions options)
    {
        _lock.EnterWriteLock();
        var currentBuffer = _commonBuffer;
        currentBuffer.AddItem(item);
        var addToQueue = NeedAddToQueue(currentBuffer);

        if (NeedHandle(currentBuffer, options))
        {
            _commonBuffer = new Buffer<TKey, TPayload>();
        }

        _lock.ExitWriteLock();

        if (addToQueue)
        {
            await _buffersQueue.Writer.WriteAsync(currentBuffer);
        }
    }

    public async ValueTask<Buffer<TKey, TPayload>> DequeueAsync()
    {
        var dequeuedBuffer = await _buffersQueue.Reader.ReadAsync();
        if (dequeuedBuffer == _commonBuffer)
        {
            _lock.EnterWriteLock();
            if (dequeuedBuffer == _commonBuffer)
            {
                _commonBuffer.IsCompleted = true;
                _commonBuffer = new Buffer<TKey, TPayload>();
            }
            _lock.ExitWriteLock();
        }

        return dequeuedBuffer;
    }

    private static bool NeedHandle(Buffer<TKey, TPayload> bufferToProcess, BufferOptions optionsSnapshot)
    {
        return bufferToProcess.Items.Count >= optionsSnapshot.MaxCount;
    }

    private static bool NeedAddToQueue(Buffer<TKey, TPayload> buffer)
    {
        if (!buffer.Enqueued)
        {
            buffer.Enqueued = true;
            return true;
        }

        return false;
    }
}
