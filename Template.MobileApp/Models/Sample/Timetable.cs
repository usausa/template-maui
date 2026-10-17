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

public static class TimetableCalculator
{
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

    public static TimeSpan GetBusyTotal(IEnumerable<TimetableEvent> events, TimeSpan start, TimeSpan end) =>
        TimeSpan.FromTicks(MergeBusy(events, start, end).Sum(static x => (x.End - x.Start).Ticks));

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
