namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed class UIKitDashHero
{
    public string Number { get; init; } = string.Empty;
    public DateTime Eta { get; init; }
    public double Rate { get; init; }
}

public sealed class UIKitDashMetric
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;

    public int EnterDelay { get; init; }
}

public sealed class UIKitDashTileTemplateSelector : DataTemplateSelector
{
    public DataTemplate HeroTemplate { get; set; } = default!;

    public DataTemplate MetricTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        item is UIKitDashHero ? HeroTemplate : MetricTemplate;
}

public sealed partial class UIKitDashViewModel : AppViewModelBase
{
    // Field

    private readonly KitActivity activity = KitSample.LoadActivity(DateTime.Now);

    // Header

    public string Greeting { get; } = "おはようございます";
    public string UserName { get; } = "うさうささん";

    // Tile

    public IReadOnlyList<object> Tiles { get; }

    // Period

    public IReadOnlyList<string> Periods { get; } = ["日", "週", "月"];

    [ObservableProperty]
    public partial int PeriodIndex { get; set; } = (int)KitPeriod.Week;

    // Chart

    [ObservableProperty]
    public partial int SpendTotal { get; set; }

    [ObservableProperty]
    public partial int OrderTotal { get; set; }

    public ChartDrawing SpendChart { get; } = new()
    {
        ValueFormat = "{0:N0} 歩"
    };

    public ChartDrawing OrderChart { get; } = new()
    {
        LineColor = Color.FromArgb("#1E88E5"),
        LineHighColor = Color.FromArgb("#1E88E5"),
        ValueFormat = "{0:N0} bpm"
    };

    // Sleep

    // Quick links

    public string OrderNumber => activity.OrderNumber;

    public DateTime OrderEta => activity.OrderEta;

    // Command

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand SettingCommand { get; }

    public IObserveCommand OnboardCommand { get; }

    public IObserveCommand TrackingCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitDashViewModel()
    {
        Disposables.Add(SpendChart);
        Disposables.Add(OrderChart);

        Tiles =
        [
            new UIKitDashHero { Number = activity.OrderNumber, Eta = activity.OrderEta, Rate = activity.OrderRate },
            new UIKitDashMetric { Title = "今月の注文", Value = activity.OrderCount.ToString(CultureInfo.CurrentCulture), Unit = "件", Icon = Fonts.MaterialIcons.Receipt_long, EnterDelay = 80 },
            new UIKitDashMetric { Title = "今月の利用", Value = activity.Spend.ToString("N0", CultureInfo.CurrentCulture), Unit = "円", Icon = Fonts.MaterialIcons.Payments, EnterDelay = 140 },
            new UIKitDashMetric { Title = "クーポン", Value = activity.CouponCount.ToString(CultureInfo.CurrentCulture), Unit = "枚", Icon = Fonts.MaterialIcons.Local_offer, EnterDelay = 200 },
            new UIKitDashMetric { Title = "ポイント", Value = activity.Points.ToString("N0", CultureInfo.CurrentCulture), Unit = "pt", Icon = Fonts.MaterialIcons.Stars, EnterDelay = 260 }
        ];

        NotifyCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitNotify));
        SettingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitSetting));
        OnboardCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitOnboard));
        TrackingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitTracking));

        SubscribePeriodIndex(_ => ShowPeriod());
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        ShowPeriod();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ShowPeriod()
    {
        var period = (KitPeriod)PeriodIndex;

        var spend = KitSample.LoadSpendSeries(activity, period);
        SpendChart.ShowBar(spend.Values, spend.Labels);
        SpendTotal = (int)spend.Values.Sum();

        var orders = KitSample.LoadOrderSeries(activity, period);
        OrderChart.ShowLine(orders.Values, orders.Labels);
        OrderTotal = (int)orders.Values.Sum();
    }
}
