namespace Template.MobileApp.Helpers;

using System.Collections;
using System.Collections.Specialized;

internal static class RingBufferEvent
{
    public static readonly NotifyCollectionChangedEventArgs Reset = new(NotifyCollectionChangedAction.Reset);
}

public sealed class RingBuffer<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged
{
    private readonly T[] buffer;

    private int head;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

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

    public void Push(T value)
    {
        buffer[head] = value;
        head = (head + 1) % buffer.Length;
        if (Count < buffer.Length)
        {
            Count++;
        }

        CollectionChanged?.Invoke(this, RingBufferEvent.Reset);
    }

    public void RemoveFirst()
    {
        if (Count == 0)
        {
            return;
        }

        buffer[(head - Count + buffer.Length) % buffer.Length] = default!;
        Count--;

        CollectionChanged?.Invoke(this, RingBufferEvent.Reset);
    }

    public void Clear()
    {
        Array.Clear(buffer);
        head = 0;
        Count = 0;

        CollectionChanged?.Invoke(this, RingBufferEvent.Reset);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    //--------------------------------------------------------------------------------
    // List
    //--------------------------------------------------------------------------------

    bool IList.IsFixedSize => true;

    bool IList.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    object? IList.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException();
    }

    int IList.Add(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    bool IList.Contains(object? value) => FindIndex(value) >= 0;

    int IList.IndexOf(object? value) => FindIndex(value);

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    void ICollection.CopyTo(Array array, int index)
    {
        for (var i = 0; i < Count; i++)
        {
            array.SetValue(this[i], index + i);
        }
    }

    private int FindIndex(object? value)
    {
        for (var i = 0; i < Count; i++)
        {
            if (Equals(this[i], value))
            {
                return i;
            }
        }

        return -1;
    }
}
