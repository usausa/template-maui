namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum KitPeriod
{
    Day,
    Week,
    Month
}

public enum KitSleepStage
{
    Awake,
    Rem,
    Light,
    Deep
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record KitSeries(
    IReadOnlyList<double> Values,
    IReadOnlyList<string> Labels);

public sealed record KitSleepSpan(
    DateTime Start,
    TimeSpan Length,
    KitSleepStage Stage);

public sealed record KitSleepTotal(
    KitSleepStage Stage,
    TimeSpan Length);

public sealed class KitActivity
{
    public required DateTime Today { get; init; }

    public required int Steps { get; init; }

    public required int StepGoal { get; init; }

    public required int HeartRate { get; init; }

    public required int Calories { get; init; }

    public required string OrderNumber { get; init; }

    public required DateTime OrderEta { get; init; }

    public required IReadOnlyList<KitSleepSpan> Sleep { get; init; }

    public double StepRate => (double)Steps / StepGoal;

    public int StepRemain => Math.Max(0, StepGoal - Steps);

    public double Distance => Math.Round(Steps * 0.00072, 1);

    public DateTime BedTime => Sleep[0].Start;

    public DateTime WakeTime => Sleep[^1].Start + Sleep[^1].Length;

    public TimeSpan SleepLength => WakeTime - BedTime;

    public IReadOnlyList<KitSleepTotal> SleepTotals() =>
        Enum.GetValues<KitSleepStage>()
            .Reverse()
            .Select(x => new KitSleepTotal(x, TimeSpan.FromMinutes(Sleep.Where(y => y.Stage == x).Sum(static y => y.Length.TotalMinutes))))
            .ToList();
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class KitSample
{
    private const int MonthDays = 30;

    private static readonly int[] HourWeights = [0, 0, 0, 0, 0, 0, 2, 9, 12, 4, 3, 4, 10, 6, 3, 3, 4, 8, 11, 7, 5, 3, 1, 0];

    private static readonly int[] HourHeart = [56, 55, 54, 54, 55, 57, 63, 78, 82, 72, 70, 71, 76, 73, 70, 69, 72, 80, 96, 84, 76, 70, 64, 60];

    private static readonly (int Minutes, KitSleepStage Stage)[] SleepPlan =
    [
        (18, KitSleepStage.Light), (42, KitSleepStage.Deep), (24, KitSleepStage.Light), (16, KitSleepStage.Rem),
        (30, KitSleepStage.Light), (34, KitSleepStage.Deep), (22, KitSleepStage.Light), (24, KitSleepStage.Rem),
        (4, KitSleepStage.Awake), (38, KitSleepStage.Light), (18, KitSleepStage.Deep), (26, KitSleepStage.Light),
        (30, KitSleepStage.Rem), (5, KitSleepStage.Awake), (40, KitSleepStage.Light), (26, KitSleepStage.Rem),
        (37, KitSleepStage.Light), (10, KitSleepStage.Awake)
    ];

    public static KitActivity LoadActivity(DateTime now)
    {
        var today = now.Date;

        var start = today.AddDays(-1).AddHours(23).AddMinutes(12);
        var spans = new List<KitSleepSpan>();
        foreach (var (minutes, stage) in SleepPlan)
        {
            spans.Add(new KitSleepSpan(start, TimeSpan.FromMinutes(minutes), stage));
            start = start.AddMinutes(minutes);
        }

        var slot = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute >= 30 ? 30 : 0, 0);
        return new KitActivity
        {
            Today = today,
            Steps = 7852,
            StepGoal = 10000,
            HeartRate = 72,
            Calories = 412,
            OrderNumber = "#A1284",
            OrderEta = slot.AddMinutes(60),
            Sleep = spans
        };
    }

    public static KitSeries LoadStepSeries(KitActivity activity, KitPeriod period)
    {
        if (period == KitPeriod.Day)
        {
            var total = HourWeights.Sum();
            var hours = HourWeights.Select(x => Math.Floor((double)activity.Steps * x / total)).ToArray();
            hours[Array.IndexOf(HourWeights, HourWeights.Max())] += activity.Steps - hours.Sum();
            return new KitSeries(hours, Enumerable.Range(0, 24).Select(static x => $"{x}時").ToList());
        }

        return DailySeries(activity.Today, period, x => StepsOf(activity, x));
    }

    public static KitSeries LoadHeartSeries(KitActivity activity, KitPeriod period)
    {
        if (period == KitPeriod.Day)
        {
            return new KitSeries(
                HourHeart.Select(static x => (double)x).ToList(),
                Enumerable.Range(0, 24).Select(static x => $"{x}時").ToList());
        }

        return DailySeries(activity.Today, period, x => HeartOf(activity, x));
    }

    private static KitSeries DailySeries(DateTime today, KitPeriod period, Func<DateTime, int> valueOf)
    {
        var count = period == KitPeriod.Week ? 7 : MonthDays;
        var format = period == KitPeriod.Week ? "ddd" : "M/d";
        var days = Enumerable.Range(0, count).Select(x => today.AddDays(x - count + 1)).ToList();
        return new KitSeries(
            days.Select(x => (double)valueOf(x)).ToList(),
            days.Select(x => x.ToString(format, CultureInfo.CurrentCulture)).ToList());
    }

    private static int StepsOf(KitActivity activity, DateTime date) =>
        date == activity.Today ? activity.Steps : 7400 + (int)((2600 * Math.Sin(date.DayOfYear * 1.37)) + (1400 * Math.Sin(date.DayOfYear * 0.53)));

    private static int HeartOf(KitActivity activity, DateTime date) =>
        date == activity.Today ? activity.HeartRate : 70 + (int)Math.Round((3 * Math.Sin(date.DayOfYear * 1.13)) + (2 * Math.Sin(date.DayOfYear * 0.41)));
}
