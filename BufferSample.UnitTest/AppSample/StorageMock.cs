using System.Collections.Concurrent;

namespace BufferSemple.UnitTest.AppSample;

/// <summary>
/// эмулятор взятия блокировки на БД
/// </summary>
public class StorageMock
{
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();
    
    /// <summary>
    /// имитация взятия и удержания блокировки над сущностью
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<LockHolder> GetExclusiveLock(long[] ids)
    {
        var lockItems = ids.Select(id => _locks.GetOrAdd(id, id => new SemaphoreSlim(1))).ToArray();
        var lockHolder = new LockHolder();

        foreach (var lockItem in lockItems)
        {
            await lockItem.WaitAsync();
            lockHolder.Add(lockItem);
        }

        return lockHolder;
    }
}