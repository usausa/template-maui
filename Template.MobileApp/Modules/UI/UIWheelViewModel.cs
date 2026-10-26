namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed record UIWheelEntry(
    int Index,
    WheelItem Item,
    Color Color);

public sealed record UIWheelResult(
    string Label,
    bool IsJackpot);

#pragma warning disable CA5394
public sealed partial class UIWheelViewModel : AppViewModelBase
{
    private const int MinItems = 2;

    private const int MaxItems = 12;

    private const int HistoryCount = 5;

    private const int MaxLabelLength = 8;

    // Field

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

    // Result

    [ObservableProperty]
    public partial string Winner { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasResult { get; set; }

    [ObservableProperty]
    public partial bool IsJackpot { get; set; }

    // Item

    [ObservableProperty]
    public partial IReadOnlyList<UIWheelEntry> Entries { get; private set; } = [];

    [ObservableProperty]
    public partial bool CanAdd { get; private set; }

    [ObservableProperty]
    public partial bool CanRemove { get; private set; }

    // History

    public ObservableCollection<UIWheelResult> History { get; } = [];

    // Wheel

    public WheelDrawing Drawing { get; } = new();

    // Command

    public IObserveCommand SpinCommand { get; }

    public IObserveCommand RemoveCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIWheelViewModel(IDialog dialog)
    {
        this.dialog = dialog;

        SpinCommand = MakeDelegateCommand(Spin);
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu3);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

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
        Spin();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Spin()
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
