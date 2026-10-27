namespace Template.MobileApp.Modules.UI;

using ClamChart;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UIHealthDay : ObservableObject
{
    public static ChartAccessorCollection<UIHealthDay> Accessors { get; } = new()
    {
        { "day", static x => x.Source.Date.ToString("ddd", CultureInfo.CurrentCulture) },
        { "deep", static x => x.Deep },
        { "light", static x => x.Light },
        { "rem", static x => x.Rem },
        { "awake", static x => x.Awake }
    };

    public required SleepNight Source { get; init; }

    public int Day => Source.Date.Day;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial double BarHeight { get; set; }

    public int Deep => Source.MinutesOf(SleepStage.Deep);

    public int Light => Source.MinutesOf(SleepStage.Light);

    public int Rem => Source.MinutesOf(SleepStage.Rem);

    public int Awake => Source.MinutesOf(SleepStage.Awake);
}

public sealed class UIHealthStage
{
    public required string Name { get; init; }

    public required Color Color { get; init; }

    public required int Minutes { get; init; }

    public required double Ratio { get; init; }
}

public sealed partial class UIHealthViewModel : AppViewModelBase
{
    // Field

    private const double BarArea = 96;

    private static readonly (SleepStage Stage, string Name, string Color)[] StagePlan =
    [
        (SleepStage.Awake, "覚醒", "#FB8C00"),
        (SleepStage.Rem, "REM", "#AB47BC"),
        (SleepStage.Light, "浅い", "#4FC3F7"),
        (SleepStage.Deep, "深い", "#1565C0")
    ];

    // Property

    public SleepLog Log { get; }

    public IReadOnlyList<UIHealthDay> Days { get; }

    public SleepStageDrawing Stage { get; } = new();

    [ObservableProperty]
    public partial SleepNight Night { get; set; } = default!;

    [ObservableProperty]
    public partial int Score { get; set; }

    [ObservableProperty]
    public partial double ScoreRatio { get; set; }

    [ObservableProperty]
    public partial double DurationRatio { get; set; }

    [ObservableProperty]
    public partial double DeepRatio { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<ChartEntry> Rings { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<UIHealthStage> Stages { get; set; } = [];

    [ObservableProperty]
    public partial string Advice { get; set; } = string.Empty;

    // Command

    public IObserveCommand SelectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIHealthViewModel(TimeProvider timeProvider)
    {
        Log = SleepSample.LoadLog(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
        Days = [.. Log.Nights.Select(x => new UIHealthDay { Source = x })];

        var longest = Math.Max(1, Log.LongestMinutes);
        foreach (var day in Days)
        {
            day.BarHeight = BarArea * day.Source.TotalMinutes / longest;
        }

        SelectCommand = MakeDelegateCommand<UIHealthDay>(CommandMode.Simple, Select);

        Select(Days.FirstOrDefault(x => x.Source.Date == DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime)) ?? Days[^1]);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Select(UIHealthDay day)
    {
        foreach (var item in Days)
        {
            item.IsSelected = ReferenceEquals(item, day);
        }

        Night = day.Source;
        Score = Log.ScoreOf(Night);
        ScoreRatio = Score / 100d;
        DurationRatio = Math.Min(1d, (double)Night.TotalMinutes / Log.GoalMinutes);
        DeepRatio = Night.RatioOf(SleepStage.Deep);
        Rings =
        [
            new("スコア", ScoreRatio, Color.FromArgb("#1565C0"), 1),
            new("睡眠時間", DurationRatio, Color.FromArgb("#4FC3F7"), 1),
            new("深い睡眠", DeepRatio, Color.FromArgb("#AB47BC"), 1)
        ];
        Stages =
        [
            .. StagePlan.Select(x => new UIHealthStage
            {
                Name = x.Name,
                Color = Color.FromArgb(x.Color),
                Minutes = Night.MinutesOf(x.Stage),
                Ratio = Night.RatioOf(x.Stage)
            })
        ];
        Advice = Log.AdviceOf(Night);
        Stage.Update(Night);
    }
}
