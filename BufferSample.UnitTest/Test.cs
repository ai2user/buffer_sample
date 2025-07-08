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
        foreach (var request in requests)
        {
            await singleKeyBuffer.EnqueueAsync(1, request, bufferOptions);
        }

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
        foreach (var request in requests)
        {
            await multiplyKeysBuffer.EnqueueAsync(1, request, bufferOptions);
        }

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
        foreach (var request in requests)
        {
            var bufer = new Buffer<long, BufferPayload>(1);
            bufer.AddItem(request);
            //no await
            _ = queueHandler.HandleAsync(bufer);
        }

        await Task.WhenAll(requests.Select(q => q.TaskCompletionSource.Task));
    }
}