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

public sealed record KitSeries(IReadOnlyList<double> Values, IReadOnlyList<string> Labels);

public sealed record KitSleepSpan(DateTime Start, TimeSpan Length, KitSleepStage Stage);

public sealed record KitSleepTotal(KitSleepStage Stage, TimeSpan Length);

// 今日の活動 (歩数・心拍・消費・配達中の注文・昨夜の睡眠)
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

// 活動の見本 (今日の値・昨夜の睡眠・1 日 / 週 / 月の歩数と心拍)。日時は今日からの相対
public static class KitSample
{
    private const int MonthDays = 30;

    // 1 時間ごとの歩数の割合 (0 時〜23 時)
    private static readonly int[] HourWeights = [0, 0, 0, 0, 0, 0, 2, 9, 12, 4, 3, 4, 10, 6, 3, 3, 4, 8, 11, 7, 5, 3, 1, 0];

    // 1 時間ごとの心拍の目安 (0 時〜23 時)
    private static readonly int[] HourHeart = [56, 55, 54, 54, 55, 57, 63, 78, 82, 72, 70, 71, 76, 73, 70, 69, 72, 80, 96, 84, 76, 70, 64, 60];

    // 昨夜の睡眠の段階 (寝てからの分と段階)
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

        // 配達の予定は次の 30 分の区切りの 30 分後
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
            // 割合で分けた端数は、いちばん多い時間に足して合計を今日の歩数にそろえる
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

    // 週は曜日、月は日付の名前
    private static KitSeries DailySeries(DateTime today, KitPeriod period, Func<DateTime, int> valueOf)
    {
        var count = period == KitPeriod.Week ? 7 : MonthDays;
        var format = period == KitPeriod.Week ? "ddd" : "M/d";
        var days = Enumerable.Range(0, count).Select(x => today.AddDays(x - count + 1)).ToList();
        return new KitSeries(
            days.Select(x => (double)valueOf(x)).ToList(),
            days.Select(x => x.ToString(format, CultureInfo.CurrentCulture)).ToList());
    }

    // 日ごとの値は周期の違う波を重ねて日付でずらす (今日は今の値)
    private static int StepsOf(KitActivity activity, DateTime date) =>
        date == activity.Today ? activity.Steps : 7400 + (int)((2600 * Math.Sin(date.DayOfYear * 1.37)) + (1400 * Math.Sin(date.DayOfYear * 0.53)));

    private static int HeartOf(KitActivity activity, DateTime date) =>
        date == activity.Today ? activity.HeartRate : 70 + (int)Math.Round((3 * Math.Sin(date.DayOfYear * 1.13)) + (2 * Math.Sin(date.DayOfYear * 0.41)));
}
