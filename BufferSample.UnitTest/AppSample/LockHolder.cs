namespace BufferSemple.UnitTest.AppSample;

public class LockHolder : IDisposable
{
    private readonly List<SemaphoreSlim> _locks = new();
    private bool _disposed;

    public void Add(SemaphoreSlim lockItem)
    {
        _locks.Add(lockItem);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        foreach (var semaphoreSlim in _locks)
        {
            semaphoreSlim.Release();
        }
    }
}