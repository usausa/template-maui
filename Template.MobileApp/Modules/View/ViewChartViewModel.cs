namespace Template.MobileApp.Modules.View;

using ClamChart;

public enum ViewChartKind
{
    Line,
    Area,
    Bar,
    Stacked,
    Scatter,
    Pie,
    Heatmap,
    Climate
}

public sealed partial class ViewChartViewModel : AppViewModelBase
{
    // Property

    [ObservableProperty]
    public partial ViewChartKind Kind { get; set; }

    public IReadOnlyList<MonthlySales> Sales { get; } = ChartSample.LoadSales();

    public IReadOnlyList<ChartEntry> Shares { get; } = ChartSample.LoadShares();

    public IReadOnlyList<ChartPoint> Scatter { get; } = ChartSample.LoadScatter();

    public IReadOnlyList<AccessCount> Accesses { get; } = ChartSample.LoadAccesses();

    public IReadOnlyList<ChartTimePoint> Contributions { get; }

    public IReadOnlyList<ClimateRecord> Records { get; }

    public double Temperature => Records[^1].Temperature;

    public double Humidity => Records[^1].Humidity;

    // Command

    public ICommand SelectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewChartViewModel(TimeProvider timeProvider)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        Contributions = ChartSample.LoadContributions(now.Date);
        Records = ChartSample.LoadClimate(now);

        SelectCommand = MakeDelegateCommand<ViewChartKind>(x => Kind = x);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
