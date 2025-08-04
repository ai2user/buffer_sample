namespace BufferSample.Lib.Buffers;

public class Buffer<TKey, TPayload>
{
    public readonly TKey Key;
    public List<TPayload> Items { get; } = new();
    public bool IsCompleted { get; set; }
    public bool Enqueued { get; set; }

    private readonly Lock _lock = new();

    public Buffer(TKey key)
    {
        Key = key;
        BufferType = BufferType.SingleKey;
    }

    public Buffer()
    {
        Key = default;
        BufferType = BufferType.Common;
    }

    public BufferType BufferType { get; private set; }

    public void Lock() => _lock.Enter();

    public void UnLock() => _lock.Exit();

    public void AddItem(TPayload item) => Items.Add(item);
}

public enum BufferType
{
    SingleKey,
    Common
}