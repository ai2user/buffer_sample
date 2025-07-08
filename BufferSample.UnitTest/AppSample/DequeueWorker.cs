namespace BufferSemple.UnitTest.AppSample;

public class DequeueWorker
{
    private readonly BufferHandler _handler;
    private readonly Buffers _buffers;
    private readonly MultiplyKeysLongBuffer _multiplyKeysLongBuffer;

    public DequeueWorker(
        BufferHandler handler,
        Buffers buffers,
        MultiplyKeysLongBuffer multiplyKeysLongBuffer
    )
    {
        _handler = handler;
        _buffers = buffers;
        _multiplyKeysLongBuffer = multiplyKeysLongBuffer;
    }

    public async Task ProcessSingleKeys()
    {
        while (true)
        {
            await _handler.HandleAsync(await _buffers.DequeueAsync());
        }
    }

    public async Task ProcessMultiplyKey()
    {
        while (true)
        {
            await _handler.HandleAsync(await _multiplyKeysLongBuffer.DequeueAsync());
        }
    }
}