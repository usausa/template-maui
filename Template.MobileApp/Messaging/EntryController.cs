namespace Template.MobileApp.Messaging;

public class EntryCompleteEvent
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

    private string? text;

    public string? Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    private bool enable;

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
