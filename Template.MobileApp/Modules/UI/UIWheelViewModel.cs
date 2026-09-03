namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

#pragma warning disable CA5394
public sealed partial class UIWheelViewModel : AppViewModelBase
{
    private static readonly string[] MenuItems =
    [
        "ラーメン",
        "カレー",
        "寿司",
        "パスタ",
        "焼肉",
        "そば",
        "ハンバーガー",
        "サラダ"
    ];

    private readonly Random random = new();

    [ObservableProperty]
    public partial string Winner { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasResult { get; set; }

    public WheelDrawing Drawing { get; } = new();

    public IObserveCommand SpinCommand { get; }

    public UIWheelViewModel()
    {
        Drawing.SetItems(MenuItems);

        SpinCommand = MakeDelegateCommand(ExecuteSpin);
    }

    private void ExecuteSpin()
    {
        // 3周+ランダム角。減速停止はWheelDrawing側 (実行中の再実行はWheelDrawingが無視する)
        var extra = 1080f + (random.Next(360 * 4) / 4f);
        var started = Drawing.Spin(extra, 4200, winner =>
        {
            Winner = winner;
            HasResult = true;
        });
        if (started)
        {
            HasResult = false;
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Drawing.CancelSpin();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
