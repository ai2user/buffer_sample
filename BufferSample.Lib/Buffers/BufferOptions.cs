namespace BufferSample.Lib.Buffers;

public class BufferOptions
{
    public BufferOptions()
    {
    }

    public BufferOptions(int maxCount)
    {
        MaxCount = maxCount > 0
            ? maxCount
            : throw new ArgumentOutOfRangeException(nameof(maxCount) + " must be positive");
    }

    public int MaxCount { get; set; } = 100;
}