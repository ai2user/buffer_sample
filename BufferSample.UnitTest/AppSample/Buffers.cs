using BufferSample.Lib.Buffers;

namespace BufferSemple.UnitTest.AppSample;

public class Buffers : SingleKeyBufferQueue<long, BufferPayload>
{
}

public class MultiplyKeysLongBuffer : CommonBufferQueue<long, BufferPayload>
{
}

public class BufferPayload
{
    public readonly TaskCompletionSource TaskCompletionSource = new();
    private readonly long[] _keys;

    public BufferPayload(long key)
    {
        _keys = new[] { key };
    }

    public BufferPayload(IEnumerable<long> keys)
    {
        _keys = keys.ToArray();
    }

    public IEnumerable<long> GetKeys() => _keys.AsEnumerable();
}