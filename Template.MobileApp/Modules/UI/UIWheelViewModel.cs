namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

#pragma warning disable CA5394
public sealed partial class UIWheelViewModel : AppViewModelBase
{
    // 寿司と焼肉は当たり枠 (停止時に紙吹雪)。それ以外はきらめきの演出
    private static readonly WheelItem[] MenuItems =
    [
        new("ラーメン"),
        new("カレー"),
        new("寿司", WheelEffect.Confetti),
        new("パスタ"),
        new("焼肉", WheelEffect.Confetti),
        new("そば"),
        new("ハンバーガー"),
        new("サラダ")
    ];

    private readonly Random random = new();

    [ObservableProperty]
    public partial string Winner { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasResult { get; set; }

    [ObservableProperty]
    public partial bool IsJackpot { get; set; }

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
            Winner = winner.Label;
            IsJackpot = winner.Effect == WheelEffect.Confetti;
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

    protected override Task OnNotifyFunction4()
    {
        ExecuteSpin();
        return Task.CompletedTask;
    }
}
