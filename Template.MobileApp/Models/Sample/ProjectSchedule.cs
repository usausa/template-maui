namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum ProjectTaskState
{
    Completed,
    Running,
    Delayed,
    Waiting,
    Milestone
}

public enum GanttScale
{
    Day,
    Week,
    Month
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class ProjectTask
{
    public required string Name { get; init; }

    public required string Assignee { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required double Progress { get; init; }

    public required ProjectTaskState State { get; init; }

    public int DependsOn { get; init; } = -1;

    public int Days => End.DayNumber - Start.DayNumber + 1;
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class ProjectSchedule
{
    public required string Name { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required DateOnly Today { get; init; }

    public required IReadOnlyList<ProjectTask> Tasks { get; init; }

    public int Days => End.DayNumber - Start.DayNumber + 1;

    public int TaskCount => Tasks.Count;

    public int CompletedCount => Tasks.Count(static x => x.State == ProjectTaskState.Completed);

    public int DelayedCount => Tasks.Count(static x => x.State == ProjectTaskState.Delayed);

    // Weighted by duration so a long task moves the number more than a short one
    public double Progress
    {
        get
        {
            var total = Tasks.Sum(static x => x.Days);
            return total > 0 ? Tasks.Sum(static x => x.Days * x.Progress) / total : 0;
        }
    }

    public int OffsetOf(DateOnly date) => date.DayNumber - Start.DayNumber;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ProjectSample
{
    private const int RangeBefore = 44;

    private const int RangeAfter = 50;

    private static readonly (string Name, string Assignee, int Offset, int Days, double Progress, ProjectTaskState State, int DependsOn)[] Plan =
    [
        ("要件定義", "佐藤", -42, 6, 1.00, ProjectTaskState.Completed, -1),
        ("業務フロー整理", "中村", -38, 5, 1.00, ProjectTaskState.Completed, 0),
        ("画面設計", "鈴木", -34, 10, 1.00, ProjectTaskState.Completed, 1),
        ("設計レビュー", "中村", -24, 3, 1.00, ProjectTaskState.Completed, 2),
        ("API 設計", "高橋", -21, 8, 1.00, ProjectTaskState.Completed, 3),
        ("DB 設計", "山本", -18, 6, 1.00, ProjectTaskState.Completed, 3),
        ("基盤実装", "田中", -13, 9, 1.00, ProjectTaskState.Completed, 4),
        ("認証基盤", "田中", -10, 7, 1.00, ProjectTaskState.Completed, 6),
        ("DB 移行", "山本", -6, 6, 0.85, ProjectTaskState.Running, 5),
        ("UI 実装", "伊藤", -4, 14, 0.70, ProjectTaskState.Running, 6),
        ("API 実装", "高橋", -2, 12, 0.55, ProjectTaskState.Running, 4),
        ("通知基盤", "渡辺", 0, 9, 0.45, ProjectTaskState.Delayed, 7),
        ("決済連携", "田中", 2, 8, 0.30, ProjectTaskState.Delayed, 10),
        ("結合テスト", "渡辺", 8, 10, 0.00, ProjectTaskState.Waiting, 9),
        ("性能試験", "山本", 14, 7, 0.00, ProjectTaskState.Waiting, 13),
        ("受入テスト", "中村", 19, 8, 0.00, ProjectTaskState.Waiting, 13),
        ("ドキュメント", "鈴木", 16, 10, 0.00, ProjectTaskState.Waiting, -1),
        ("教育資料", "伊藤", 21, 7, 0.00, ProjectTaskState.Waiting, 16),
        ("本番環境構築", "田中", 22, 6, 0.00, ProjectTaskState.Waiting, -1),
        ("リリース判定", "佐藤", 29, 1, 0.00, ProjectTaskState.Milestone, 15),
        ("ストア申請", "鈴木", 30, 4, 0.00, ProjectTaskState.Waiting, 19),
        ("審査対応", "高橋", 34, 5, 0.00, ProjectTaskState.Waiting, 20),
        ("リリース", "佐藤", 40, 1, 0.00, ProjectTaskState.Milestone, 21),
        ("初期サポート", "中村", 41, 7, 0.00, ProjectTaskState.Waiting, 22)
    ];

    public static ProjectSchedule LoadSchedule(DateOnly today) =>
        new()
        {
            Name = "モバイル刷新 PJ",
            Start = today.AddDays(-RangeBefore),
            End = today.AddDays(RangeAfter),
            Today = today,
            Tasks =
            [
                .. Plan.Select(x => new ProjectTask
                {
                    Name = x.Name,
                    Assignee = x.Assignee,
                    Start = today.AddDays(x.Offset),
                    End = today.AddDays(x.Offset + x.Days - 1),
                    Progress = x.Progress,
                    State = x.State,
                    DependsOn = x.DependsOn
                })
            ]
        };

    public static double WidthOf(GanttScale scale) => scale switch
    {
        GanttScale.Day => 72,
        GanttScale.Month => 11,
        _ => 28
    };
}
