namespace Template.MobileApp.Modules.View;

public sealed partial class DragTask : ObservableObject
{
    public string Text { get; }

    public Color Accent { get; }

    [ObservableProperty]
    public partial bool IsSource { get; set; }

    [ObservableProperty]
    public partial bool IsOver { get; set; }

    [ObservableProperty]
    public partial bool IsOverAbove { get; set; }

    [ObservableProperty]
    public partial bool IsOverBelow { get; set; }

    public DragTask(string text, Color accent)
    {
        Text = text;
        Accent = accent;
    }
}

public sealed partial class ViewDragDropViewModel : AppViewModelBase
{
    // Field

    private static readonly Color Blue = Color.FromArgb("#42A5F5");

    private static readonly Color Green = Color.FromArgb("#66BB6A");

    private static readonly Color Orange = Color.FromArgb("#FF7043");

    private static readonly Color Purple = Color.FromArgb("#AB47BC");

    private static readonly Color Cyan = Color.FromArgb("#26C6DA");

    private DragTask? dragging;

    // List

    public ObservableCollection<DragTask> ReorderList { get; } =
    [
        new("1. 要件定義", Blue),
        new("2. 設計", Green),
        new("3. 実装", Orange),
        new("4. テスト", Purple),
        new("5. リリース", Cyan)
    ];

    public ObservableCollection<DragTask> TodoList { get; } =
    [
        new("牛乳を買う", Blue),
        new("資料を送る", Green),
        new("会議室を予約", Orange)
    ];

    public ObservableCollection<DragTask> DoneList { get; } =
    [
        new("朝会", Purple)
    ];

    // Drag state

    [ObservableProperty]
    public partial bool IsDragging { get; set; }

    [ObservableProperty]
    public partial bool TodoOver { get; set; }

    [ObservableProperty]
    public partial bool DoneOver { get; set; }

    [ObservableProperty]
    public partial bool TrashActive { get; set; }

    // Command

    public IObserveCommand DragStartingCommand { get; }

    public IObserveCommand ItemOverCommand { get; }
    public IObserveCommand ItemLeaveCommand { get; }
    public IObserveCommand DropOnItemCommand { get; }

    public IObserveCommand ListOverCommand { get; }
    public IObserveCommand ListLeaveCommand { get; }
    public IObserveCommand DropOnListCommand { get; }

    public IObserveCommand TrashOverCommand { get; }
    public IObserveCommand TrashLeaveCommand { get; }
    public IObserveCommand DropOnTrashCommand { get; }

    public IObserveCommand DropCompletedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewDragDropViewModel()
    {
        DragStartingCommand = MakeDelegateCommand<DragTask>(CommandMode.Simple, BeginDrag);
        ItemOverCommand = MakeDelegateCommand<DragTask>(CommandMode.Simple, x => SetItemOver(x, true));
        ItemLeaveCommand = MakeDelegateCommand<DragTask>(CommandMode.Simple, x => SetItemOver(x, false));
        DropOnItemCommand = MakeDelegateCommand<DragTask>(CommandMode.Simple, DropOnItem);
        ListOverCommand = MakeDelegateCommand<string>(CommandMode.Simple, x => SetListOver(x, true));
        ListLeaveCommand = MakeDelegateCommand<string>(CommandMode.Simple, x => SetListOver(x, false));
        DropOnListCommand = MakeDelegateCommand<string>(CommandMode.Simple, DropOnList);
        TrashOverCommand = MakeDelegateCommand(CommandMode.Simple, () => TrashActive = true);
        TrashLeaveCommand = MakeDelegateCommand(CommandMode.Simple, () => TrashActive = false);
        DropOnTrashCommand = MakeDelegateCommand(CommandMode.Simple, DropOnTrash);
        DropCompletedCommand = MakeDelegateCommand(CommandMode.Simple, EndDrag);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void BeginDrag(DragTask item)
    {
        dragging = item;
        item.IsSource = true;
        IsDragging = true;
    }

    private void EndDrag()
    {
        dragging = null;
        IsDragging = false;
        TodoOver = false;
        DoneOver = false;
        TrashActive = false;
        foreach (var item in ReorderList.Concat(TodoList).Concat(DoneList))
        {
            item.IsSource = false;
            item.IsOver = false;
            item.IsOverAbove = false;
            item.IsOverBelow = false;
        }
    }

    private void SetItemOver(DragTask target, bool value)
    {
        var over = value && (dragging is not null) && !ReferenceEquals(target, dragging);
        var below = over && IsMovingDown(dragging!, target);
        target.IsOver = over;
        target.IsOverAbove = over && !below;
        target.IsOverBelow = below;
    }

    private bool IsMovingDown(DragTask item, DragTask target)
    {
        var list = FindList(item);
        return (list is not null) && ReferenceEquals(list, FindList(target)) && (list.IndexOf(item) < list.IndexOf(target));
    }

    private void SetListOver(string name, bool value)
    {
        if (name == "Done")
        {
            DoneOver = value;
        }
        else
        {
            TodoOver = value;
        }
    }

    private void DropOnItem(DragTask target)
    {
        var item = dragging;
        if ((item is null) || ReferenceEquals(item, target))
        {
            return;
        }

        var source = FindList(item);
        var destination = FindList(target);
        if ((source is null) || (destination is null))
        {
            return;
        }

        var after = IsMovingDown(item, target);
        source.Remove(item);
        destination.Insert(destination.IndexOf(target) + (after ? 1 : 0), item);
        EndDrag();
    }

    private void DropOnList(string name)
    {
        var item = dragging;
        if (item is null)
        {
            return;
        }

        var destination = name == "Done" ? DoneList : TodoList;
        var source = FindList(item);
        if ((source is null) || ReferenceEquals(source, destination))
        {
            return;
        }

        source.Remove(item);
        destination.Add(item);
        EndDrag();
    }

    private void DropOnTrash()
    {
        var item = dragging;
        if (item is null)
        {
            return;
        }

        FindList(item)?.Remove(item);
        EndDrag();
    }

    private ObservableCollection<DragTask>? FindList(DragTask item)
    {
        if (ReorderList.Contains(item))
        {
            return ReorderList;
        }

        if (TodoList.Contains(item))
        {
            return TodoList;
        }

        if (DoneList.Contains(item))
        {
            return DoneList;
        }

        return null;
    }
}
