using BufferSample.Lib.Buffers;
using BufferSemple.UnitTest.AppSample;
using Xunit.Abstractions;

namespace BufferSemple.UnitTest;

public class Test
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const int Count = 1000;

    public Test(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TestBufferedSingle()
    {
        var singleKeyBuffer = new Buffers();
        var multiplyKeysBuffer = new MultiplyKeysLongBuffer();
        var storage = new StorageMock();
        var queueHandler = new BufferHandler(storage, TimeSpan.FromMilliseconds(10), _testOutputHelper);
        var worker = new DequeueWorker(queueHandler, singleKeyBuffer, multiplyKeysBuffer);
        var bufferOptions = new BufferOptions()
        {
            MaxCount = 2000
        };

        _ = new[] { worker.ProcessMultiplyKey(), worker.ProcessSingleKeys() };
        var requests = Enumerable.Range(1, Count).Select(_ => new BufferPayload(1)).ToArray();
        await Parallel.ForEachAsync(requests,
            async (request, _) => { await singleKeyBuffer.EnqueueAsync(1, request, bufferOptions); });

        await Task.WhenAll(requests.Select(q => q.TaskCompletionSource.Task));
    }

    [Fact]
    public async Task TestBufferedMulti()
    {
        var singleKeyBuffer = new Buffers();
        var multiplyKeysBuffer = new MultiplyKeysLongBuffer();
        var storage = new StorageMock();
        var queueHandler = new BufferHandler(storage, TimeSpan.FromMilliseconds(10), _testOutputHelper);
        var worker = new DequeueWorker(queueHandler, singleKeyBuffer, multiplyKeysBuffer);
        var bufferOptions = new BufferOptions()
        {
            MaxCount = 2000
        };

        _ = new[] { worker.ProcessMultiplyKey(), worker.ProcessSingleKeys() };
        var requests = Enumerable.Range(1, Count).Select(_ => new BufferPayload(1)).ToArray();
        await Parallel.ForEachAsync(requests,
            async (request, _) => { await multiplyKeysBuffer.EnqueueAsync(1, request, bufferOptions); });

        await Task.WhenAll(requests.Select(q => q.TaskCompletionSource.Task));
    }


    /// <summary>
    /// Вариант последовательной обработки. Что будет без буферизации
    /// </summary>
    [Fact]
    public async Task TestBypass()
    {
        var storage = new StorageMock();
        var queueHandler = new BufferHandler(storage, TimeSpan.FromMilliseconds(10), _testOutputHelper);
        var requests = Enumerable.Range(1, Count).Select(_ => new BufferPayload(1)).ToArray();
        await Parallel.ForEachAsync(requests, async (request, _) =>
        {
            var buffer = new Buffer<long, BufferPayload>(1);
            buffer.AddItem(request);
            //no await
            await queueHandler.HandleAsync(buffer);
        });

        await Task.WhenAll(requests.Select(q => q.TaskCompletionSource.Task));
    }
}