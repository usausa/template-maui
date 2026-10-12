namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

// 項目のチップ (Index は削除する位置)
public sealed record UIWheelEntry(int Index, WheelItem Item, Color Color);

public sealed record UIWheelResult(string Label, bool IsJackpot);

#pragma warning disable CA5394
public sealed partial class UIWheelViewModel : AppViewModelBase
{
    private const int MinItems = 2;

    private const int MaxItems = 12;

    private const int HistoryCount = 5;

    // ホイールの中心に届かない長さ
    private const int MaxLabelLength = 8;

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

    private readonly IDialog dialog;

    private readonly Random random = new();

    private readonly List<WheelItem> items = [.. MenuItems];

    [ObservableProperty]
    public partial string Winner { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasResult { get; set; }

    [ObservableProperty]
    public partial bool IsJackpot { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<UIWheelEntry> Entries { get; private set; } = [];

    [ObservableProperty]
    public partial bool CanAdd { get; private set; }

    [ObservableProperty]
    public partial bool CanRemove { get; private set; }

    // 新しい順
    public ObservableCollection<UIWheelResult> History { get; } = [];

    public WheelDrawing Drawing { get; } = new();

    public IObserveCommand SpinCommand { get; }

    public IObserveCommand RemoveCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIWheelViewModel(IDialog dialog)
    {
        this.dialog = dialog;

        SpinCommand = MakeDelegateCommand(ExecuteSpin);
        RemoveCommand = MakeDelegateCommand<UIWheelEntry>(Remove);

        UpdateItems();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Drawing.CancelSpin();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // 項目を足す (回っている間は変えない)
    protected override async Task OnNotifyFunction3()
    {
        if (CanAdd && !Drawing.IsSpinning)
        {
            var result = await dialog.PromptAsync(title: "項目を追加", placeHolder: "項目の名前", parameter: new PromptParameter { MaxLength = MaxLabelLength });
            var label = result.Text.Trim();
            if (result.Accepted && (label.Length > 0))
            {
                items.Add(new WheelItem(label));
                UpdateItems();
            }
        }
    }

    protected override Task OnNotifyFunction4()
    {
        ExecuteSpin();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ExecuteSpin()
    {
        var extra = 1080f + (random.Next(360 * 4) / 4f);
        var started = Drawing.Spin(extra, 4200, ShowResult);

        if (started)
        {
            HasResult = false;
            IsJackpot = false;
        }
    }

    private void Remove(UIWheelEntry entry)
    {
        if (CanRemove && !Drawing.IsSpinning)
        {
            items.RemoveAt(entry.Index);
            UpdateItems();
        }
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void ShowResult(WheelItem winner)
    {
        Winner = winner.Label;
        IsJackpot = winner.Effect == WheelEffect.Confetti;
        HasResult = true;

        History.Insert(0, new UIWheelResult(winner.Label, IsJackpot));
        if (History.Count > HistoryCount)
        {
            History.RemoveAt(HistoryCount);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void UpdateItems()
    {
        Drawing.SetItems(items.ToArray());
        Entries = items.Select((x, i) => new UIWheelEntry(i, x, WheelDrawing.ColorOf(i, items.Count))).ToArray();
        CanAdd = items.Count < MaxItems;
        CanRemove = items.Count > MinItems;
    }
}
