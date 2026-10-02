namespace Template.MobileApp.Models.App;

public enum TimerMode
{
    Stopwatch,
    Countdown
}

// 保存する状態 (ストップウォッチとカウントダウン)
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

// ラップ (No は 1 から。Lap は前のラップからの時間、Split は通算)
public sealed record TimerLap(int No, TimeSpan Lap, TimeSpan Split);

// ラップの行 (最速と最遅の印、一番遅いラップに対する長さの比)
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

// ラップの一覧 (新しい順)
public sealed class TimerLapList : ObservableCollection<TimerLapItem>
{
    public void AddLatest(TimerLap lap)
    {
        Insert(0, new TimerLapItem(lap));
        UpdateMarks();
    }

    // 古い順のラップから作り直す
    public void Rebuild(IEnumerable<TimerLap> laps)
    {
        Clear();
        foreach (var lap in laps.Reverse())
        {
            Add(new TimerLapItem(lap));
        }

        UpdateMarks();
    }

    // 最速と最遅の印はラップが 2 つ以上のとき (同じ長さなら古いラップ)
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

// 保存する状態
public sealed class TimerStopwatchSnapshot
{
    public TimerStopwatchState State { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public TimeSpan Accumulated { get; set; }

    public IReadOnlyList<TimeSpan> Splits { get; set; } = [];
}

// ストップウォッチ。経過時間は刻みを数えず、開始時刻と止めた時点までの時間から計算する (時刻は呼び出し側が渡す)
public sealed class TimerStopwatch
{
    // ラップを取った時点の通算の時間
    private readonly List<TimeSpan> splits = [];

    private DateTimeOffset? startedAt;

    private TimeSpan accumulated;

    public TimerStopwatchState State { get; private set; }

    public int LapCount => splits.Count;

    public TimeSpan GetElapsed(DateTimeOffset now) =>
        accumulated + (startedAt is { } start ? now - start : TimeSpan.Zero);

    // 今のラップ (最後にラップを取ってからの時間)
    public TimeSpan GetCurrentLap(DateTimeOffset now) =>
        GetElapsed(now) - (splits.Count > 0 ? splits[^1] : TimeSpan.Zero);

    // 1 分で 1 周 (0〜1)
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

    // 計測中だけ取れる
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

    // 古い順
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

// 保存する状態
public sealed class TimerCountdownSnapshot
{
    public TimerCountdownState State { get; set; }

    public TimeSpan Duration { get; set; }

    public TimeSpan Remaining { get; set; }

    public DateTimeOffset? EndAt { get; set; }
}

// カウントダウン。残り時間は終わる時刻から計算する (時刻は呼び出し側が渡す)
public sealed class TimerCountdown
{
    // 残りがこれ以下になると警告
    private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(10);

    private TimeSpan remaining;

    public TimerCountdownState State { get; private set; }

    public TimeSpan Duration { get; private set; }

    // 計測中に終わる時刻
    public DateTimeOffset? EndAt { get; private set; }

    public TimeSpan GetRemaining(DateTimeOffset now) =>
        EndAt is { } end ? (end > now ? end - now : TimeSpan.Zero) : remaining;

    // 残りの割合 (減っていく)
    public double GetProgress(DateTimeOffset now) =>
        Duration > TimeSpan.Zero ? GetRemaining(now) / Duration : 0;

    // 計測中で残りが少ない
    public bool IsWarning(DateTimeOffset now) =>
        (State == TimerCountdownState.Running) && (GetRemaining(now) <= WarningTime);

    // 計測中は変えられない
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

    // 終わった後は最初から
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

    // 計測中に終わる時刻を過ぎていれば終了にして true
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
