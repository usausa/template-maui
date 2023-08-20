namespace Template.MobileApp.Messaging;

public class EntryCompleteEvent
{
    public bool Handled { get; set; }
}

public interface IEntryController : INotifyPropertyChanged
{
    event EventHandler<EventArgs> FocusRequested;

    string? Text { get; set; }

    bool Enable { get; set; }

    void HandleCompleted(EntryCompleteEvent e);
}

public sealed class EntryController : NotificationObject, IEntryController
{
    private event EventHandler<EventArgs>? Requested;

    private readonly ICommand? command;

    private string? text;

    private bool enable;

    // Property

    public string? Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    public bool Enable
    {
        get => enable;
        set => SetProperty(ref enable, value);
    }

    // Constructor

    public EntryController()
    {
        enable = true;
    }

    public EntryController(bool enable)
    {
        this.enable = enable;
    }

    public EntryController(ICommand command)
    {
        enable = true;
        this.command = command;
    }

    public EntryController(bool enable, ICommand command)
    {
        this.enable = enable;
        this.command = command;
    }

    // Request

    event EventHandler<EventArgs> IEntryController.FocusRequested
    {
        add => Requested += value;
        remove => Requested -= value;
    }

    public void FocusRequest()
    {
        Requested?.Invoke(this, EventArgs.Empty);
    }

    // Event

    void IEntryController.HandleCompleted(EntryCompleteEvent e)
    {
        if ((command is not null) && command.CanExecute(e))
        {
            command.Execute(e);
        }
    }
}
