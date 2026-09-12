namespace Template.MobileApp.Modules.UI;

public sealed class UIKitTrackingStep
{
    public string Title { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public bool Done { get; init; }
    public bool Current { get; init; }
    public int Delay { get; init; }
    public bool IsComplete => Done;
    public bool IsCurrent => Current;
    public bool IsPending => !Done && !Current;
}

public sealed class UIKitTrackingViewModel : AppViewModelBase
{
    public string OrderNumber { get; } = "#A1284";
    public string Eta { get; } = "到着予定: 本日 16:30";

    public IReadOnlyList<UIKitTrackingStep> Steps { get; } =
    [
        new() { Title = "注文受付", Time = "09:12", Done = true, Delay = 0 },
        new() { Title = "梱包完了", Time = "10:45", Done = true, Delay = 80 },
        new() { Title = "発送済み", Time = "12:30", Done = true, Delay = 160 },
        new() { Title = "配達中", Time = "15:00", Current = true, Delay = 240 },
        new() { Title = "配達完了", Time = "--:--", Delay = 320 }
    ];

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIKitDash);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
