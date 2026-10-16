namespace Template.MobileApp.Components;

public sealed record NotificationAction(string Id, string Title);

public sealed class NotificationTappedEventArgs : EventArgs
{
    public int Id { get; }

    public string? Action { get; }

    public string? Payload { get; }

    public NotificationTappedEventArgs(int id, string? action, string? payload)
    {
        Id = id;
        Action = action;
        Payload = payload;
    }
}

public interface INotificationService
{
    event EventHandler<NotificationTappedEventArgs>? Tapped;

    bool CanScheduleExact { get; }

    NotificationTappedEventArgs? TakePendingTap();

    void Show(int id, string title, string message, string? payload = null, IReadOnlyList<NotificationAction>? actions = null);

    void Schedule(int id, string title, string message, TimeSpan delay, string? payload = null);

    void Cancel(int id);

    void OpenExactAlarmSettings();
}

public sealed partial class NotificationService : INotificationService
{
    public const string ChannelId = "template.default";

    private NotificationTappedEventArgs? pendingTap;

    public event EventHandler<NotificationTappedEventArgs>? Tapped;

    public partial bool CanScheduleExact { get; }

    public NotificationTappedEventArgs? TakePendingTap()
    {
        var tap = pendingTap;
        pendingTap = null;
        return tap;
    }

    public partial void Show(int id, string title, string message, string? payload, IReadOnlyList<NotificationAction>? actions);

    public partial void Schedule(int id, string title, string message, TimeSpan delay, string? payload);

    public partial void Cancel(int id);

    public partial void OpenExactAlarmSettings();

    private void RaiseTapped(NotificationTappedEventArgs args)
    {
        if (Tapped is null)
        {
            pendingTap = args;
        }
        else
        {
            Tapped(this, args);
        }
    }
}
