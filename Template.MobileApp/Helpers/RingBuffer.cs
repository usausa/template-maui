namespace Template.MobileApp.Helpers;

public sealed class RingBuffer<T>
{
    private readonly T[] buffer;

    private int head;

    public int Capacity => buffer.Length;

    public int Count { get; private set; }

    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return buffer[(head - Count + index + buffer.Length) % buffer.Length];
        }
    }

    public RingBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        buffer = new T[capacity];
    }

    public void Add(T value)
    {
        buffer[head] = value;
        head = (head + 1) % buffer.Length;
        if (Count < buffer.Length)
        {
            Count++;
        }
    }

    public void RemoveFirst()
    {
        if (Count == 0)
        {
            return;
        }

        buffer[(head - Count + buffer.Length) % buffer.Length] = default!;
        Count--;
    }

    public void Clear()
    {
        Array.Clear(buffer);
        head = 0;
        Count = 0;
    }
}
