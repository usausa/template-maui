namespace Template.MobileApp.Modules.Sample;

using Template.MobileApp.Graphics.Drawing;

#pragma warning disable CA5394
public sealed partial class SampleChartViewModel : AppViewModelBase
{
    private readonly Random random = new();

    [ObservableProperty]
    public partial ChartKind Kind { get; set; }

    public ChartDrawing Drawing { get; } = new();

    public ICommand SelectCommand { get; }

    public SampleChartViewModel()
    {
        Disposables.Add(Drawing);

        SelectCommand = MakeDelegateCommand<ChartKind>(Show);
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Show(Kind);
        return Task.CompletedTask;
    }

    private void Show(ChartKind kind)
    {
        Kind = kind;
        switch (kind)
        {
            case ChartKind.Line:
                Drawing.ShowLine(CreateWalk(12, 50, 12));
                break;
            case ChartKind.Bar:
                Drawing.ShowBar(CreateWalk(12, 50, 15));
                break;
            case ChartKind.Donut:
                Drawing.ShowDonut(Enumerable.Range(0, 5).Select(_ => (double)random.Next(10, 40)).ToList());
                break;
            case ChartKind.Stacked:
                Drawing.ShowStacked(Enumerable.Range(0, 6)
                    .Select(_ => Enumerable.Range(0, 3).Select(_ => (double)random.Next(10, 40)).ToArray())
                    .ToList());
                break;
            case ChartKind.Scatter:
                Drawing.ShowScatter(Enumerable.Range(0, 24)
                    .Select(_ => new PointF(random.Next(0, 100), random.Next(0, 100)))
                    .ToList());
                break;
            case ChartKind.Heat:
                Drawing.ShowHeat(Enumerable.Range(0, 7)
                    .Select(r => Enumerable.Range(0, 12)
                        .Select(c => (Math.Sin((r * 0.9d) + (c * 0.5d)) * 40d) + random.Next(0, 30))
                        .ToArray())
                    .ToArray());
                break;
            default:
                Drawing.ShowCandle(CreateCandles(12));
                break;
        }
    }

    // 乱数ウォークのダミーデータを生成する
    private List<double> CreateWalk(int count, double start, double step)
    {
        var values = new List<double>(count);
        var value = start;
        for (var i = 0; i < count; i++)
        {
            value = Math.Clamp(value + ((random.NextDouble() - 0.45) * step), 10, 100);
            values.Add(value);
        }
        return values;
    }

    private List<CandlePoint> CreateCandles(int count)
    {
        var candles = new List<CandlePoint>(count);
        var close = 50d;
        for (var i = 0; i < count; i++)
        {
            var open = close;
            close = Math.Clamp(open + ((random.NextDouble() - 0.5) * 16), 15, 95);
            var high = Math.Max(open, close) + (random.NextDouble() * 5);
            var low = Math.Min(open, close) - (random.NextDouble() * 5);
            candles.Add(new CandlePoint(open, high, low, close));
        }
        return candles;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
#pragma warning restore CA5394
