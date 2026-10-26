namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum SleepStage
{
    Awake,
    Rem,
    Light,
    Deep
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record SleepSpan(SleepStage Stage, int Offset, int Minutes)
{
    public int End => Offset + Minutes;
}

public sealed class SleepNight
{
    public required DateOnly Date { get; init; }

    public required TimeOnly BedTime { get; init; }

    public required IReadOnlyList<SleepSpan> Spans { get; init; }

    public required int RestingHeartRate { get; init; }

    public required double Respiration { get; init; }

    public required int Steps { get; init; }

    public int TotalMinutes => Spans.Count > 0 ? Spans[^1].End : 0;

    public TimeOnly WakeTime => BedTime.AddMinutes(TotalMinutes);

    // A brief waking in the middle of the night, the first and the last span never count
    public int AwakeCount => Spans
        .Where((x, i) => (x.Stage == SleepStage.Awake) && (i > 0) && (i < Spans.Count - 1))
        .Count();

    public int MinutesOf(SleepStage stage) =>
        Spans.Where(x => x.Stage == stage).Sum(static x => x.Minutes);

    public double RatioOf(SleepStage stage) =>
        TotalMinutes > 0 ? (double)MinutesOf(stage) / TotalMinutes : 0;
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class SleepLog
{
    private const double DurationWeight = 50;

    private const double DeepWeight = 30;

    private const double SteadyWeight = 20;

    private const double DeepTarget = 0.22;

    private const int AwakeLimit = 8;

    public required IReadOnlyList<SleepNight> Nights { get; init; }

    public required int GoalMinutes { get; init; }

    public int AverageMinutes => Nights.Count > 0 ? (int)Nights.Average(static x => x.TotalMinutes) : 0;

    public int LongestMinutes => Nights.Count > 0 ? Nights.Max(static x => x.TotalMinutes) : 0;

    public int ScoreOf(SleepNight night)
    {
        var duration = Math.Min(1d, (double)night.TotalMinutes / GoalMinutes) * DurationWeight;
        var deep = Math.Clamp(night.RatioOf(SleepStage.Deep) / DeepTarget, 0d, 1d) * DeepWeight;
        var steady = Math.Clamp(1d - ((double)night.AwakeCount / AwakeLimit), 0d, 1d) * SteadyWeight;
        return (int)Math.Round(duration + deep + steady);
    }

    public string AdviceOf(SleepNight night)
    {
        var gap = GoalMinutes - night.TotalMinutes;
        if (gap > 30)
        {
            return $"目標より {gap} 分不足しています。就寝を {gap} 分早めると深い睡眠が伸びやすくなります。";
        }

        if (night.AwakeCount >= 3)
        {
            return $"中途覚醒が {night.AwakeCount} 回ありました。就寝前のカフェインと画面の明るさを見直してみてください。";
        }

        if (night.RatioOf(SleepStage.Deep) < DeepTarget)
        {
            return $"深い睡眠が {night.RatioOf(SleepStage.Deep):P0} でした。日中の運動量を増やすと割合が上がりやすくなります。";
        }

        return $"過去 {Nights.Count} 日の平均を上回る良い睡眠でした。今の生活リズムを保ちましょう。";
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class SleepSample
{
    private const int GoalMinutes = 450;

    private const int WeekDays = 7;

    private static readonly (SleepStage Stage, int Minutes)[][] Cycles =
    [
        [(SleepStage.Light, 22), (SleepStage.Deep, 48), (SleepStage.Light, 18), (SleepStage.Rem, 12)],
        [(SleepStage.Light, 20), (SleepStage.Deep, 36), (SleepStage.Light, 20), (SleepStage.Rem, 20), (SleepStage.Awake, 4)],
        [(SleepStage.Light, 25), (SleepStage.Deep, 20), (SleepStage.Light, 22), (SleepStage.Rem, 26)],
        [(SleepStage.Light, 28), (SleepStage.Deep, 12), (SleepStage.Light, 24), (SleepStage.Rem, 32), (SleepStage.Awake, 5)],
        [(SleepStage.Light, 30), (SleepStage.Deep, 8), (SleepStage.Light, 26), (SleepStage.Rem, 34)]
    ];

    private static readonly (int BedHour, int BedMinute, int Total)[] Plan =
    [
        (23, 34, 432),
        (0, 12, 388),
        (23, 48, 446),
        (1, 5, 326),
        (23, 20, 470),
        (0, 40, 408),
        (22, 58, 462)
    ];

    public static SleepLog LoadLog(DateOnly today)
    {
        var start = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var nights = new List<SleepNight>(WeekDays);
        for (var i = 0; i < WeekDays; i++)
        {
            var (hour, minute, total) = Plan[i];
            nights.Add(new SleepNight
            {
                Date = start.AddDays(i),
                BedTime = new TimeOnly(hour, minute),
                Spans = CreateSpans(total, i),
                RestingHeartRate = 50 + ((i * 3) % 7),
                Respiration = 13.4 + (((i * 7) % 11) * 0.2),
                Steps = 6200 + (((i * 1361) % 47) * 110)
            });
        }

        return new SleepLog { Nights = nights, GoalMinutes = GoalMinutes };
    }

    private static List<SleepSpan> CreateSpans(int total, int seed)
    {
        var spans = new List<SleepSpan>();
        var offset = 0;
        var cycle = 0;
        while (offset < total)
        {
            foreach (var (stage, length) in Cycles[cycle % Cycles.Length])
            {
                var minutes = Math.Min(total - offset, Math.Max(3, length + (((seed + cycle) % 7) - 3)));
                if (minutes <= 0)
                {
                    break;
                }

                spans.Add(new SleepSpan(stage, offset, minutes));
                offset += minutes;
                if (offset >= total)
                {
                    break;
                }
            }

            cycle++;
        }

        return spans;
    }
}
