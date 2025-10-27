namespace Template.MobileApp.Models.Sample;

public sealed class StatDataSet
{
    public event EventHandler<EventArgs>? Updated;

    private readonly int capacity;

    private readonly float[] buffer;

    private int head;

    public int Capacity => capacity;

    public float LastValue
    {
        get
        {
            var actualIndex = head == 0 ? capacity - 1 : head - 1;
            return buffer[actualIndex];
        }
    }

    public StatDataSet(int capacity)
    {
        this.capacity = capacity;
        buffer = new float[capacity];
    }

    public void Add(float value)
    {
        var index = head;
        head = (head + 1) % capacity;
        buffer[index] = value;

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public float GetValue(int index)
    {
        var actualIndex = (head + index) % capacity;
        return buffer[actualIndex];
    }
}
