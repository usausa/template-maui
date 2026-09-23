namespace Template.MobileApp.State;

#pragma warning disable CA1724
public sealed class Session
{
    // アプリが前面か (Window の Resumed / Stopped で更新。通知の出し方の切り替えに使う)
    public bool IsForeground { get; set; } = true;
}
#pragma warning restore CA1724
