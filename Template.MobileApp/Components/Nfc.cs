namespace Template.MobileApp.Components;

#pragma warning disable CA1819
public interface INfc
{
    byte[] Id { get; }

    void SetTimeout(int timeout);

    byte[] Access(byte[] command);
}
#pragma warning restore CA1819

public sealed class NfcEventArgs : EventArgs
{
    public INfc Tag { get; }

    public NfcEventArgs(INfc tag)
    {
        Tag = tag;
    }
}

public interface INfcReader
{
    public event EventHandler<NfcEventArgs>? Detected;

    public bool Enabled { get; set; }
}

public sealed partial class NfcReader : INfcReader
{
    public event EventHandler<NfcEventArgs>? Detected;

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
