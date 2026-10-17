namespace Template.MobileApp.Models.App;

//--------------------------------------------------------------------------------
// Common
//--------------------------------------------------------------------------------

public enum TimerMode
{
    Stopwatch,
    Countdown
}

public sealed class TimerSnapshot
{
    public TimerStopwatchSnapshot Stopwatch { get; set; } = new();

    public TimerCountdownSnapshot Countdown { get; set; } = new();
}

//--------------------------------------------------------------------------------
// Stopwatch
//--------------------------------------------------------------------------------

public enum TimerStopwatchState
{
    Stopped,
    Running,
    Paused
}

public sealed record TimerLap(int No, TimeSpan Lap, TimeSpan Split);

public sealed partial class TimerLapItem : ObservableObject
{
    public int No { get; }

    public TimeSpan Lap { get; }

    public TimeSpan Split { get; }

    [ObservableProperty]
    public partial bool IsFastest { get; set; }

    [ObservableProperty]
    public partial bool IsSlowest { get; set; }

    [ObservableProperty]
    public partial double Ratio { get; set; }

    public TimerLapItem(TimerLap lap)
    {
        No = lap.No;
        Lap = lap.Lap;
        Split = lap.Split;
    }
}

public sealed class TimerLapList : ObservableCollection<TimerLapItem>
{
    public void AddLatest(TimerLap lap)
    {
        Insert(0, new TimerLapItem(lap));
        UpdateMarks();
    }

    public void Rebuild(IEnumerable<TimerLap> laps)
    {
        Clear();
        foreach (var lap in laps.Reverse())
        {
            Add(new TimerLapItem(lap));
        }

        UpdateMarks();
    }

    private void UpdateMarks()
    {
        var laps = this.Reverse().ToList();
        var fastest = laps.Count >= 2 ? laps.MinBy(static x => x.Lap)!.No : 0;
        var slowest = laps.Count >= 2 ? laps.MaxBy(static x => x.Lap)!.No : 0;
        var longest = laps.Count > 0 ? laps.Max(static x => x.Lap) : TimeSpan.Zero;
        foreach (var lap in laps)
        {
            lap.IsFastest = lap.No == fastest;
            lap.IsSlowest = lap.No == slowest;
            lap.Ratio = longest > TimeSpan.Zero ? lap.Lap / longest : 0;
        }
    }
}

public sealed class TimerStopwatchSnapshot
{
    public TimerStopwatchState State { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public TimeSpan Accumulated { get; set; }

    public IReadOnlyList<TimeSpan> Splits { get; set; } = [];
}

public sealed class TimerStopwatch
{
    private readonly List<TimeSpan> splits = [];

    private DateTimeOffset? startedAt;

    private TimeSpan accumulated;

    public TimerStopwatchState State { get; private set; }

    public int LapCount => splits.Count;

    public TimeSpan GetElapsed(DateTimeOffset now) =>
        accumulated + (startedAt is { } start ? now - start : TimeSpan.Zero);

    public TimeSpan GetCurrentLap(DateTimeOffset now) =>
        GetElapsed(now) - (splits.Count > 0 ? splits[^1] : TimeSpan.Zero);

    public double GetMinuteProgress(DateTimeOffset now) => (GetElapsed(now).TotalSeconds % 60) / 60;

    public void Start(DateTimeOffset now)
    {
        if (State != TimerStopwatchState.Running)
        {
            startedAt = now;
            State = TimerStopwatchState.Running;
        }
    }

    public void Pause(DateTimeOffset now)
    {
        if (State == TimerStopwatchState.Running)
        {
            accumulated = GetElapsed(now);
            startedAt = null;
            State = TimerStopwatchState.Paused;
        }
    }

    public TimerLap? Lap(DateTimeOffset now)
    {
        TimerLap? lap = null;
        if (State == TimerStopwatchState.Running)
        {
            var split = GetElapsed(now);
            var previous = splits.Count > 0 ? splits[^1] : TimeSpan.Zero;
            splits.Add(split);
            lap = new TimerLap(splits.Count, split - previous, split);
        }

        return lap;
    }

    public void Reset()
    {
        splits.Clear();
        startedAt = null;
        accumulated = TimeSpan.Zero;
        State = TimerStopwatchState.Stopped;
    }

    public IReadOnlyList<TimerLap> GetLaps()
    {
        var laps = new List<TimerLap>(splits.Count);
        var previous = TimeSpan.Zero;
        for (var i = 0; i < splits.Count; i++)
        {
            laps.Add(new TimerLap(i + 1, splits[i] - previous, splits[i]));
            previous = splits[i];
        }

        return laps;
    }

    public TimerStopwatchSnapshot Export() => new()
    {
        State = State,
        StartedAt = startedAt,
        Accumulated = accumulated,
        Splits = [.. splits]
    };

    public void Import(TimerStopwatchSnapshot snapshot)
    {
        splits.Clear();
        splits.AddRange(snapshot.Splits);
        startedAt = snapshot.StartedAt;
        accumulated = snapshot.Accumulated;
        State = snapshot.State;
    }
}

//--------------------------------------------------------------------------------
// Countdown
//--------------------------------------------------------------------------------

public enum TimerCountdownState
{
    Stopped,
    Running,
    Paused,
    Finished
}

public sealed class TimerCountdownSnapshot
{
    public TimerCountdownState State { get; set; }

    public TimeSpan Duration { get; set; }

    public TimeSpan Remaining { get; set; }

    public DateTimeOffset? EndAt { get; set; }
}

public sealed class TimerCountdown
{
    private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(10);

    private TimeSpan remaining;

    public TimerCountdownState State { get; private set; }

    public TimeSpan Duration { get; private set; }

    public DateTimeOffset? EndAt { get; private set; }

    public TimeSpan GetRemaining(DateTimeOffset now) =>
        EndAt is { } end ? (end > now ? end - now : TimeSpan.Zero) : remaining;

    public double GetProgress(DateTimeOffset now) =>
        Duration > TimeSpan.Zero ? GetRemaining(now) / Duration : 0;

    public bool IsWarning(DateTimeOffset now) =>
        (State == TimerCountdownState.Running) && (GetRemaining(now) <= WarningTime);

    public void SetDuration(TimeSpan duration)
    {
        if (State != TimerCountdownState.Running)
        {
            Duration = duration;
            remaining = duration;
            EndAt = null;
            State = TimerCountdownState.Stopped;
        }
    }

    public void Start(DateTimeOffset now)
    {
        if (State == TimerCountdownState.Finished)
        {
            remaining = Duration;
        }

        if ((State != TimerCountdownState.Running) && (remaining > TimeSpan.Zero))
        {
            EndAt = now + remaining;
            State = TimerCountdownState.Running;
        }
    }

    public void Pause(DateTimeOffset now)
    {
        if (State == TimerCountdownState.Running)
        {
            remaining = GetRemaining(now);
            EndAt = null;
            State = TimerCountdownState.Paused;
        }
    }

    public void Reset()
    {
        remaining = Duration;
        EndAt = null;
        State = TimerCountdownState.Stopped;
    }

    public bool CheckFinished(DateTimeOffset now)
    {
        if ((State == TimerCountdownState.Running) && (EndAt <= now))
        {
            remaining = TimeSpan.Zero;
            EndAt = null;
            State = TimerCountdownState.Finished;
            return true;
        }

        return false;
    }

    public TimerCountdownSnapshot Export() => new()
    {
        State = State,
        Duration = Duration,
        Remaining = remaining,
        EndAt = EndAt
    };

    public void Import(TimerCountdownSnapshot snapshot)
    {
        Duration = snapshot.Duration;
        remaining = snapshot.Remaining;
        EndAt = snapshot.EndAt;
        State = snapshot.State;
    }
}
