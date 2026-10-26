namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UITradeTimeframe : ObservableObject
{
    public required StockTimeframe Value { get; init; }

    public required string Name { get; init; }

    public required int Ticks { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public sealed partial class UITradeViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    private readonly TimeProvider timeProvider;

    private readonly IDispatcherTimer timer;

    // Property

    public StockBoard Board { get; }

    public ChartDrawing Drawing { get; } = new()
    {
        Background = Color.FromArgb("#10151B"),
        GridColor = Color.FromArgb("#25303D"),
        TextColor = Color.FromArgb("#8A99A8"),
        UpColor = Color.FromArgb("#26A69A"),
        DownColor = Color.FromArgb("#EF5350"),
        VolumeRatio = 0.22f,
        ValueFormat = "{0:N1}"
    };

    public IReadOnlyList<UITradeTimeframe> Timeframes { get; } =
    [
        new() { Value = StockTimeframe.Minute1, Name = "1分", Ticks = 3 },
        new() { Value = StockTimeframe.Minute5, Name = "5分", Ticks = 6, IsSelected = true },
        new() { Value = StockTimeframe.Daily, Name = "日足", Ticks = 10 },
        new() { Value = StockTimeframe.Weekly, Name = "週足", Ticks = 15 },
        new() { Value = StockTimeframe.Monthly, Name = "月足", Ticks = 20 }
    ];

    [ObservableProperty]
    public partial bool IsTicks { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    // Command

    public IObserveCommand SelectCommand { get; }

    public IObserveCommand OrderCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITradeViewModel(TimeProvider timeProvider, IDispatcher dispatcher)
    {
        this.timeProvider = timeProvider;

        Board = StockSample.LoadBoard(timeProvider.GetLocalNow().DateTime);

        Disposables.Add(Drawing);

        SelectCommand = MakeDelegateCommand<UITradeTimeframe>(CommandMode.Simple, SelectTimeframe);
        OrderCommand = MakeDelegateCommand<StockSide>(CommandMode.Simple, Order);

        timer = dispatcher.CreateTimer();
        timer.Interval = Interval;
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            ShowCandles();
        }

        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsTicks = true;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        IsTicks = false;
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void SelectTimeframe(UITradeTimeframe timeframe)
    {
        foreach (var entry in Timeframes)
        {
            entry.IsSelected = ReferenceEquals(entry, timeframe);
        }

        Board.CandleTicks = timeframe.Ticks;
        Message = $"{timeframe.Name} に切り替えました";
    }

    private void Order(StockSide side)
    {
        var price = (side == StockSide.Buy ? Board.Bids[0].Price : Board.Asks[^1].Price).ToString("N1", CultureInfo.CurrentCulture);
        Message = $"{(side == StockSide.Buy ? "買" : "売")} 指値 {price} 100株 の注文を受け付けました";
    }

    private void Tick()
    {
        Board.Update(timeProvider.GetLocalNow().DateTime);

        var candle = ToPoint(Board.Candles[^1]);
        Drawing.MarkerValue = Board.Quote.Price;
        if (Board.CandleAdded)
        {
            Drawing.AddCandle(candle);
        }
        else
        {
            Drawing.UpdateCandle(candle);
        }
    }

    private void ShowCandles()
    {
        Drawing.MarkerValue = Board.Quote.Price;
        Drawing.ShowCandle([.. Board.Candles.Select(ToPoint)]);
    }

    private static CandlePoint ToPoint(StockCandle candle) =>
        new(candle.Open, candle.High, candle.Low, candle.Close, candle.Volume);
}
