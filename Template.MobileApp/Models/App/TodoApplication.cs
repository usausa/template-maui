namespace Template.MobileApp.Models.App;

using System.Collections.Specialized;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

// 期限の区分け (日付で比べる)
public enum TodoDue
{
    None,
    Overdue,
    Today,
    Tomorrow,
    Later
}

// 一覧のグループ (並びの順)
public enum TodoGroup
{
    Overdue,
    Today,
    Tomorrow,
    Later,
    NoDue,
    Done
}

//--------------------------------------------------------------------------------
// Rule
//--------------------------------------------------------------------------------

public static class TodoDueRule
{
    public static TodoDue Classify(DateTime? dueDate, DateTime today) =>
        dueDate?.Date switch
        {
            null => TodoDue.None,
            { } date when date < today.Date => TodoDue.Overdue,
            { } date when date == today.Date => TodoDue.Today,
            { } date when date == today.Date.AddDays(1) => TodoDue.Tomorrow,
            _ => TodoDue.Later
        };

    // 完了した行は期限によらず完了のグループ
    public static TodoGroup ToGroup(TodoDue due, bool isDone) =>
        isDone
            ? TodoGroup.Done
            : due switch
            {
                TodoDue.Overdue => TodoGroup.Overdue,
                TodoDue.Today => TodoGroup.Today,
                TodoDue.Tomorrow => TodoGroup.Tomorrow,
                TodoDue.Later => TodoGroup.Later,
                _ => TodoGroup.NoDue
            };
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

// 一覧の行
public sealed partial class TodoItem : ObservableObject
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty(NotifyAlso = [nameof(HasNote)])]
    public partial string Note { get; set; } = string.Empty;

    public bool HasNote => Note.Length > 0;

    [ObservableProperty]
    public partial DateTime? DueDate { get; set; }

    // 今日から見た期限の区分け
    [ObservableProperty]
    public partial TodoDue Due { get; set; }

    [ObservableProperty]
    public partial bool IsImportant { get; set; }

    [ObservableProperty]
    public partial bool IsDone { get; set; }

    public void UpdateDue(DateTime today) => Due = TodoDueRule.Classify(DueDate, today);
}

// グループの行。一覧に出す行 (列挙と Count) は、畳んでいる間は空。全部の行は Items、見出しの件数は Total
public sealed class TodoSection : IReadOnlyList<TodoItem>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs IsExpandedChangedEventArgs = new(nameof(IsExpanded));

    private static readonly PropertyChangedEventArgs TotalChangedEventArgs = new(nameof(Total));

    private static readonly NotifyCollectionChangedEventArgs ResetEventArgs = new(NotifyCollectionChangedAction.Reset);

    private readonly List<TodoItem> items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TodoGroup Group { get; }

    public IReadOnlyList<TodoItem> Items => items;

    public int Total => items.Count;

    public bool IsExpanded { get; private set; } = true;

    public int Count => IsExpanded ? items.Count : 0;

    public TodoItem this[int index] => IsExpanded ? items[index] : throw new ArgumentOutOfRangeException(nameof(index));

    public TodoSection(TodoGroup group)
    {
        Group = group;
    }

    public IEnumerator<TodoItem> GetEnumerator() => (IsExpanded ? items : []).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // 畳む・開く
    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        PropertyChanged?.Invoke(this, IsExpandedChangedEventArgs);
        CollectionChanged?.Invoke(this, ResetEventArgs);
    }

    public void Insert(int index, TodoItem item)
    {
        items.Insert(index, item);
        PropertyChanged?.Invoke(this, TotalChangedEventArgs);
        if (IsExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }
    }

    public bool Remove(TodoItem item)
    {
        var index = items.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        items.RemoveAt(index);
        PropertyChanged?.Invoke(this, TotalChangedEventArgs);
        if (IsExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        return true;
    }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// 一覧。行を期限と完了でグループに分けて並べ、件数と完了の割合を数える
public sealed partial class TodoList : ObservableObject
{
    // すべてのグループ (行の無いグループと、隠している完了も持つ)
    private readonly Dictionary<TodoGroup, TodoSection> sections =
        Enum.GetValues<TodoGroup>().ToDictionary(static x => x, static x => new TodoSection(x));

    // 表示するグループ (並びは Group の順)
    public ObservableCollection<TodoSection> Sections { get; } = [];

    [ObservableProperty]
    public partial bool ShowDone { get; set; } = true;

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial int DoneCount { get; set; }

    [ObservableProperty]
    public partial int RemainingCount { get; set; }

    // 完了の割合
    [ObservableProperty]
    public partial double Progress { get; set; }

    public void ToggleShowDone()
    {
        ShowDone = !ShowDone;
        UpdateVisibility(sections[TodoGroup.Done]);
    }

    // グループの中の並びの位置に入れる
    public void Place(TodoItem item)
    {
        var section = sections[TodoDueRule.ToGroup(item.Due, item.IsDone)];
        var index = 0;
        while ((index < section.Total) && (Compare(section.Items[index], item) <= 0))
        {
            index++;
        }

        section.Insert(index, item);
        UpdateVisibility(section);
    }

    public void Remove(TodoItem item)
    {
        foreach (var section in sections.Values)
        {
            if (section.Remove(item))
            {
                UpdateVisibility(section);
            }
        }
    }

    // 消されていなければ、今の状態のグループの位置へ動かす
    public void Move(TodoItem item)
    {
        if (sections.Values.Any(x => x.Items.Contains(item)))
        {
            Remove(item);
            Place(item);
        }
    }

    public void UpdateSummary()
    {
        TotalCount = sections.Values.Sum(static x => x.Total);
        DoneCount = sections.Values.Sum(static x => x.Items.Count(static y => y.IsDone));
        RemainingCount = TotalCount - DoneCount;
        Progress = TotalCount > 0 ? (double)DoneCount / TotalCount : 0;
    }

    // 行があり、隠している完了でなければ、グループの順の位置に出す
    private void UpdateVisibility(TodoSection section)
    {
        var visible = (section.Total > 0) && ((section.Group != TodoGroup.Done) || ShowDone);
        var index = Sections.IndexOf(section);
        if (visible && (index < 0))
        {
            var position = 0;
            while ((position < Sections.Count) && (Sections[position].Group < section.Group))
            {
                position++;
            }

            Sections.Insert(position, section);
        }
        else if (!visible && (index >= 0))
        {
            Sections.RemoveAt(index);
        }
    }

    // 完了は新しく完了した順、ほかは期限・作った順
    private static int Compare(TodoItem x, TodoItem y)
    {
        var result = x.IsDone ? y.UpdatedAt.CompareTo(x.UpdatedAt) : Nullable.Compare(x.DueDate, y.DueDate);
        return result != 0 ? result : x.Id.CompareTo(y.Id);
    }
}
