namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum HabitCategory
{
    Fitness,
    Wellness,
    Learning,
    Work
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class HabitItem : ObservableObject
{
    public string Name { get; set; } = default!;

    public HabitCategory Category { get; set; }

    public string Goal { get; set; } = default!;

    public int Steps { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Progress), nameof(IsCompleted)])]
    public partial int Done { get; set; }

    [ObservableProperty]
    public partial int Streak { get; set; }

    public double Progress => Steps > 0 ? (double)Done / Steps : 0d;

    public bool IsCompleted => Done >= Steps;
}

// 今日は全部の習慣が終わったときに達成になる
public sealed partial class HabitDay : ObservableObject
{
    public DateTime Date { get; set; }

    public bool IsToday { get; set; }

    [ObservableProperty]
    public partial bool IsAchieved { get; set; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// 習慣を 1 つ進めると今日の進みも進み、目標に届いた習慣は連続日数が 1 増える
public sealed partial class HabitTracker : ObservableObject
{
    public ObservableCollection<HabitItem> Habits { get; } = [];

    public ObservableCollection<HabitDay> Week { get; } = [];

    public int TotalSteps { get; }

    [ObservableProperty(NotifyAlso = [nameof(Progress)])]
    public partial int DoneSteps { get; set; }

    public double Progress => TotalSteps > 0 ? (double)DoneSteps / TotalSteps : 0d;

    public HabitTracker(IEnumerable<HabitItem> habits, IEnumerable<HabitDay> week)
    {
        Habits.AddRange(habits);
        Week.AddRange(week);
        TotalSteps = Habits.Sum(static x => x.Steps);
        DoneSteps = Habits.Sum(static x => x.Done);
    }

    public void Step(HabitItem item)
    {
        if (!item.IsCompleted)
        {
            item.Done++;
            if (item.IsCompleted)
            {
                item.Streak++;
            }

            DoneSteps++;
            Week[^1].IsAchieved = DoneSteps >= TotalSteps;
        }
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// 習慣の見本 (4 つの習慣と、今週の達成)
public static class HabitSample
{
    public static IReadOnlyList<HabitItem> LoadHabits() =>
    [
        new() { Name = "水を飲む", Category = HabitCategory.Fitness, Goal = "1日2.5L", Steps = 8, Done = 6, Streak = 12 },
        new() { Name = "朝のヨガ", Category = HabitCategory.Wellness, Goal = "15分", Steps = 2, Done = 2, Streak = 21 },
        new() { Name = "読書", Category = HabitCategory.Learning, Goal = "20分", Steps = 5, Done = 2, Streak = 8 },
        new() { Name = "コードを書く", Category = HabitCategory.Work, Goal = "1時間", Steps = 3, Done = 2, Streak = 5 }
    ];

    public static IReadOnlyList<HabitDay> LoadWeek(DateTime today)
    {
        bool[] achieved = [true, true, false, true, true, true];
        return
        [
            .. achieved.Select((x, i) => new HabitDay { Date = today.AddDays(i - achieved.Length), IsAchieved = x }),
            new HabitDay { Date = today, IsToday = true }
        ];
    }
}
