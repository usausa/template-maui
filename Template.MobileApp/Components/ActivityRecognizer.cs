namespace Template.MobileApp.Components;

public sealed class ActivityEventArgs : EventArgs
{
    public DateTime Timestamp { get; }

    public int Counter { get; }

    public ActivityEventArgs(DateTime timestamp, int counter)
    {
        Timestamp = timestamp;
        Counter = counter;
    }
}

public interface IActivityRecognizer
{
    event EventHandler<ActivityEventArgs>? Changed;

    public bool Enabled { get; set; }
}

public sealed partial class ActivityRecognizer : IActivityRecognizer
{
    public event EventHandler<ActivityEventArgs>? Changed;

    public bool Enabled
    {
        get;
        set
        {
            if (value)
            {
                if (!field)
                {
                    Start();
                    field = true;
                }
            }
            else
            {
                if (field)
                {
                    Stop();
                    field = false;
                }
            }
        }
    }

    private partial void Start();

    private partial void Stop();
}
