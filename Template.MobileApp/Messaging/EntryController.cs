namespace Template.MobileApp.Messaging;

public sealed class EntryCompleteEvent
{
    public bool Handled { get; set; }
}

public interface IEntryController : INotifyPropertyChanged
{
    event EventHandler<EventArgs> FocusRequest;

    // Property

    string? Text { get; set; }

    bool Enable { get; set; }

    // Event

    void HandleCompleted(EntryCompleteEvent e);
}

public sealed class EntryController : NotificationObject, IEntryController
{
    private event EventHandler<EventArgs>? FocusRequestHandler;

    event EventHandler<EventArgs> IEntryController.FocusRequest
    {
        add => FocusRequestHandler += value;
        remove => FocusRequestHandler -= value;
    }

    // Field

    private readonly ICommand? command;

    // Property

    public string? Text
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool Enable
    {
        get;
        set => SetProperty(ref field, value);
    }

    // Constructor

    public EntryController()
    {
        Enable = true;
    }

    public EntryController(bool enable)
    {
        Enable = enable;
    }

    public EntryController(ICommand command)
    {
        Enable = true;
        this.command = command;
    }

    public EntryController(bool enable, ICommand command)
    {
        Enable = enable;
        this.command = command;
    }

    // Request

    public void FocusRequest()
    {
        FocusRequestHandler?.Invoke(this, EventArgs.Empty);
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
