using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BufferSample.Lib.Buffers;

public class SingleKeyBufferQueue<TKey, TPayload> where TKey : notnull
{
    private readonly Channel<Buffer<TKey, TPayload>> _buffersQueue =
        Channel.CreateUnbounded<Buffer<TKey, TPayload>>(new UnboundedChannelOptions() { SingleReader = true });

    private readonly ConcurrentDictionary<TKey, Buffer<TKey, TPayload>> _buffers = new();

    public async ValueTask EnqueueAsync(TKey key, TPayload item, BufferOptions options)
    {
        var buffer = GetBufferWithLock(key);

        buffer.AddItem(item);
        var addToQueue = NeedAddToQueue(buffer);

        if (NeedHandle(buffer, options))
        {
            _buffers.TryRemove(key, out _);
            buffer.IsCompleted = true;
        }

        buffer.UnLock();
        if (addToQueue)
        {
            await _buffersQueue.Writer.WriteAsync(buffer);
        }
    }

    private Buffer<TKey, TPayload> GetBufferWithLock(TKey key)
    {
        while (true)
        {
            var buffer = _buffers.GetOrAdd(key, l => new Buffer<TKey, TPayload>(key));
            buffer.Lock();
            if (buffer.IsCompleted)
            {
                buffer.UnLock();
                continue;
            }

            return buffer;
        }
    }

    public async ValueTask<Buffer<TKey, TPayload>> DequeueAsync()
    {
        var bufferFromQueue = await _buffersQueue.Reader.ReadAsync();
        var key = bufferFromQueue.Key;
        if (_buffers.TryGetValue(key, out var buffer))
        {
            if (bufferFromQueue == buffer)
            {
                buffer.Lock();
                _buffers.TryGetValue(key, out var bufferSecond);
                // double check
                if (bufferFromQueue == bufferSecond)
                {
                    _buffers.TryRemove(key, out _);
                    buffer.IsCompleted = true;
                }

                buffer.UnLock();
            }
        }

        return bufferFromQueue;
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