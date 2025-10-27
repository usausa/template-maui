namespace Template.MobileApp.Messaging;

public sealed class EntryCompleteEvent
{
    public bool Handled { get; set; }
}

public sealed class EntryController : NotificationObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<EventArgs>? FocusRequest;

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

    public void Focus()
    {
        FocusRequest?.Invoke(this, EventArgs.Empty);
    }

    // Handle

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void HandleCompleted(EntryCompleteEvent e)
    {
        if ((command is not null) && command.CanExecute(e))
        {
            command.Execute(e);
        }
    }
}
