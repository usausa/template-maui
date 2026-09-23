namespace Template.MobileApp.Components;

// 通知のアクションボタン (タップでアプリが開き、Tapped の Action に Id が入る)
public sealed record NotificationAction(string Id, string Title);

// 通知のタップ (本体のタップは Action が null)
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
    /// <summary>
    /// 通知本体またはアクションボタンのタップで UI スレッドから発火する (アプリの起動 Intent 経由も含む)。購読前に届いた分は TakePendingTap で取り出す
    /// </summary>
    event EventHandler<NotificationTappedEventArgs>? Tapped;

    // 正確な時刻のアラームが使えるか (Android 12 以降は SCHEDULE_EXACT_ALARM の許可が必要。未許可のスケジュールは前後する)
    bool CanScheduleExact { get; }

    NotificationTappedEventArgs? TakePendingTap();

    // 即時に表示する
    void Show(int id, string title, string message, string? payload = null, IReadOnlyList<NotificationAction>? actions = null);

    // 指定時間後に表示する (アプリが終了していても届く)
    void Schedule(int id, string title, string message, TimeSpan delay, string? payload = null);

    // 表示中の通知と予約を取り消す
    void Cancel(int id);

    // 正確なアラームの許可設定を開く (Android 12 以降)
    void OpenExactAlarmSettings();
}

public sealed partial class NotificationService : INotificationService
{
    // 通知チャンネル (Android 8 以降は必須)
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
