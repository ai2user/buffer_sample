using BufferSample.Lib.Buffers;
using Xunit.Abstractions;

namespace BufferSemple.UnitTest.AppSample;

public class BufferHandler
{
    private readonly StorageMock _storageMock;

    private readonly TimeSpan _processingTime;

    private readonly ITestOutputHelper _testOutputHelper;

    /// <summary>
    /// Обработчик буфера
    /// </summary>
    /// <param name="storageMock">мок хранилища</param>
    /// <param name="processingTime">Время обработки</param>
    /// <param name="testOutputHelper">для вывода размера накопленного буфера</param>
    public BufferHandler(StorageMock storageMock, TimeSpan processingTime, ITestOutputHelper testOutputHelper)
    {
        _storageMock = storageMock;
        _processingTime = processingTime;
        _testOutputHelper = testOutputHelper;
    }

    public async Task HandleAsync(Buffer<long, BufferPayload> buffer)
    {
        var keys = buffer.Items.SelectMany(q => q.GetKeys()).OrderBy(q => q).Distinct().ToArray();

        if (buffer.Items.Count > 1)
        {
            _testOutputHelper.WriteLine($"{buffer.Items.Count} -> 1");
        }

        using (await _storageMock.GetExclusiveLock(keys))
        {
            // Имитация выполнения работы под блокировкой
            await Task.Delay(_processingTime); 
        }

        foreach (var item in buffer.Items)
        {
            // Установка результата выполнения для каждого запроса
            item.TaskCompletionSource.TrySetResult();
        }
    }
}