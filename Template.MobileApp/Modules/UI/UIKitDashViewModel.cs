namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

// 全幅の歩数のカード (VariableSizeWrapPanel の ColumnSpan=2)
public sealed class UIKitDashHero
{
    public int Steps { get; init; }
    public int Goal { get; init; }
    public int Remain { get; init; }
    public double Rate { get; init; }
}

public sealed class UIKitDashMetric
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;

    // 入場アニメの段差 (ms)。BindableLayout ではインデックスが取れないためモデル側で持つ
    public int EnterDelay { get; init; }
}

// 歩数のカード (UIKitDashHero) とメトリクス (UIKitDashMetric) をタイルの種類で振り分ける
public sealed class UIKitDashTileTemplateSelector : DataTemplateSelector
{
    public DataTemplate HeroTemplate { get; set; } = default!;

    public DataTemplate MetricTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        item is UIKitDashHero ? HeroTemplate : MetricTemplate;
}

public sealed partial class UIKitDashViewModel : AppViewModelBase
{
    private readonly KitActivity activity = KitSample.LoadActivity(DateTime.Now);

    public string Greeting { get; } = "おはようございます";
    public string UserName { get; } = "うさうささん";

    // 先頭の歩数のカード + メトリクス 4 件を 1 つのタイルパネルへ流し込む (件数は可変)
    public IReadOnlyList<object> Tiles { get; }

    public IReadOnlyList<string> Periods { get; } = ["日", "週", "月"];

    [ObservableProperty]
    public partial int PeriodIndex { get; set; } = (int)KitPeriod.Week;

    [ObservableProperty]
    public partial int StepTotal { get; set; }

    [ObservableProperty]
    public partial int HeartAverage { get; set; }

    public ChartDrawing StepChart { get; } = new()
    {
        ValueFormat = "{0:N0} 歩"
    };

    public ChartDrawing HeartChart { get; } = new()
    {
        LineColor = Color.FromArgb("#1E88E5"),
        LineHighColor = Color.FromArgb("#1E88E5"),
        ValueFormat = "{0:N0} bpm"
    };

    public IReadOnlyList<KitSleepSpan> Sleep => activity.Sleep;

    public IReadOnlyList<KitSleepTotal> SleepTotals { get; }

    public DateTime BedTime => activity.BedTime;

    public DateTime WakeTime => activity.WakeTime;

    public TimeSpan SleepLength => activity.SleepLength;

    public string OrderNumber => activity.OrderNumber;

    public DateTime OrderEta => activity.OrderEta;

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand SettingCommand { get; }

    public IObserveCommand OnboardCommand { get; }

    public IObserveCommand TrackingCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKitDashViewModel()
    {
        Disposables.Add(StepChart);
        Disposables.Add(HeartChart);

        Tiles =
        [
            new UIKitDashHero { Steps = activity.Steps, Goal = activity.StepGoal, Remain = activity.StepRemain, Rate = activity.StepRate },
            new UIKitDashMetric { Title = "心拍数", Value = "72", Unit = "bpm", Icon = Fonts.MaterialIcons.Favorite, EnterDelay = 80 },
            new UIKitDashMetric { Title = "消費カロリー", Value = "412", Unit = "kcal", Icon = Fonts.MaterialIcons.Local_fire_department, EnterDelay = 140 },
            new UIKitDashMetric { Title = "睡眠", Value = "7.4", Unit = "時間", Icon = Fonts.MaterialIcons.Bedtime, EnterDelay = 200 },
            new UIKitDashMetric { Title = "距離", Value = "5.7", Unit = "km", Icon = Fonts.MaterialIcons.Directions_walk, EnterDelay = 260 }
        ];
        SleepTotals = activity.SleepTotals();

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ShowPeriod()
    {
        var period = (KitPeriod)PeriodIndex;

        var steps = KitSample.LoadStepSeries(activity, period);
        StepChart.ShowBar(steps.Values, steps.Labels);
        StepTotal = (int)steps.Values.Sum();

        var heart = KitSample.LoadHeartSeries(activity, period);
        HeartChart.ShowLine(heart.Values, heart.Labels);
        HeartAverage = (int)Math.Round(heart.Values.Average());
    }
}
