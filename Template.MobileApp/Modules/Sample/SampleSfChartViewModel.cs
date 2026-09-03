namespace Template.MobileApp.Modules.Sample;

public sealed record ChartEntry(string Label, double Value);

public sealed record SunburstEntry(string Category, string Item, double Value);

public sealed class SampleSfChartViewModel : AppViewModelBase
{
    // 月別売上 (Cartesian ColumnSeries)
    public IReadOnlyList<ChartEntry> Monthly { get; } =
    [
        new("4月", 42),
        new("5月", 58),
        new("6月", 49),
        new("7月", 73),
        new("8月", 66),
        new("9月", 81)
    ];

    // カテゴリ構成比 (Circular DoughnutSeries)
    public IReadOnlyList<ChartEntry> Share { get; } =
    [
        new("食品", 38),
        new("日用品", 24),
        new("衣料", 17),
        new("家電", 12),
        new("その他", 9)
    ];

    // スキルバランス (PolarAreaSeries)
    public IReadOnlyList<ChartEntry> Skills { get; } =
    [
        new("攻撃", 82),
        new("防御", 64),
        new("速度", 91),
        new("技術", 70),
        new("体力", 58),
        new("運", 45)
    ];

    // 成約ファネル (SfFunnelChart / SfPyramidChart)
    public IReadOnlyList<ChartEntry> Funnel { get; } =
    [
        new("訪問", 1000),
        new("会員登録", 620),
        new("カート投入", 340),
        new("購入", 180),
        new("リピート", 90)
    ];

    // 週間推移 (SparkCharts)
    public IReadOnlyList<double> Trend { get; } = [3, 6, 4, 8, 5, 9, 7, 11, 8, 12, 10, 14];

    public IReadOnlyList<double> WinLoss { get; } = [1, 1, -1, 1, -1, 1, 1, -1, 1, 1];

    // 地域別売上の階層 (SfSunburstChart)
    public IReadOnlyList<SunburstEntry> Regions { get; } =
    [
        new("東日本", "東京", 48),
        new("東日本", "宮城", 17),
        new("東日本", "北海道", 21),
        new("西日本", "大阪", 36),
        new("西日本", "福岡", 22),
        new("西日本", "広島", 14)
    ];

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
