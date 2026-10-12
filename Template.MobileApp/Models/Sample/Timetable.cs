namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class TimetableEvent
{
    public required string Title { get; init; }

    public required TimeSpan Start { get; init; }

    public required TimeSpan End { get; init; }

    public required Color Color { get; init; }

    public string Place { get; init; } = string.Empty;

    // 参加者のアイコンの画像
    public IReadOnlyList<string> Members { get; init; } = [];

    public string Memo { get; init; } = string.Empty;
}

public sealed class TimetableDay
{
    public required DateOnly Date { get; init; }

    public string DowText => Date.ToString("ddd");

    public int Day => Date.Day;

    public bool IsToday => Date == DateOnly.FromDateTime(DateTime.Today);
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// タイムテーブルの時間計算 (描画側の空き時間帯と VM 側の日合計で共用する)
public static class TimetableCalculator
{
    // イベントを範囲へクランプしてマージした使用中区間を返す
    public static IReadOnlyList<(TimeSpan Start, TimeSpan End)> MergeBusy(IEnumerable<TimetableEvent> events, TimeSpan start, TimeSpan end)
    {
        var merged = new List<(TimeSpan Start, TimeSpan End)>();
        foreach (var ev in events.OrderBy(static x => x.Start))
        {
            var s = ev.Start < start ? start : ev.Start;
            var e = ev.End > end ? end : ev.End;
            if (s >= e)
            {
                continue;
            }

            if ((merged.Count > 0) && (s <= merged[^1].End))
            {
                if (e > merged[^1].End)
                {
                    merged[^1] = (merged[^1].Start, e);
                }
            }
            else
            {
                merged.Add((s, e));
            }
        }

        return merged;
    }

    // 使用中区間の隙間 (空き時間帯) を返す
    public static IReadOnlyList<(TimeSpan Start, TimeSpan End)> GetFreeSlots(IEnumerable<TimetableEvent> events, TimeSpan start, TimeSpan end)
    {
        var free = new List<(TimeSpan Start, TimeSpan End)>();
        var current = start;
        foreach (var (busyStart, busyEnd) in MergeBusy(events, start, end))
        {
            if (busyStart > current)
            {
                free.Add((current, busyStart));
            }
            current = busyEnd;
        }
        if (current < end)
        {
            free.Add((current, end));
        }

        return free;
    }

    // 範囲内でイベントに埋まっている時間の合計 (重複はマージして数える)
    public static TimeSpan GetBusyTotal(IEnumerable<TimetableEvent> events, TimeSpan start, TimeSpan end) =>
        TimeSpan.FromTicks(MergeBusy(events, start, end).Sum(static x => (x.End - x.Start).Ticks));

    // 所要時間の表示形式 (1h30m / 2h / 45m)
    public static string FormatDuration(TimeSpan value)
    {
        var hours = (int)value.TotalHours;
        var minutes = value.Minutes;
        if (hours > 0)
        {
            return minutes > 0
                ? $"{hours}h{minutes}m"
                : $"{hours}h";
        }
        return $"{minutes}m";
    }
}
