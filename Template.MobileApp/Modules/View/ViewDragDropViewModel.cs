namespace Template.MobileApp.Modules.View;

public sealed partial class DragTask : ObservableObject
{
    public string Text { get; }

    public Color Accent { get; }

    // ドラッグ中の元アイテム (半透明表示)
    [ObservableProperty]
    public partial bool IsSource { get; set; }

    // ドラッグ中のアイテムがこの行に重なっている (行の強調)
    [ObservableProperty]
    public partial bool IsOver { get; set; }

    // 挿入位置の線。上へ動かすときは行の上端、下へ動かすときは行の下端に出す
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
    private static readonly Color Blue = Color.FromArgb("#42A5F5");
    private static readonly Color Green = Color.FromArgb("#66BB6A");
    private static readonly Color Orange = Color.FromArgb("#FF7043");
    private static readonly Color Purple = Color.FromArgb("#AB47BC");
    private static readonly Color Cyan = Color.FromArgb("#26C6DA");

    private DragTask? dragging;

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

    // ドラッグ中は受け入れ先 (列・行・ゴミ箱) を全て示し、重なっている先だけ強調する
    [ObservableProperty]
    public partial bool IsDragging { get; set; }

    [ObservableProperty]
    public partial bool TodoOver { get; set; }

    [ObservableProperty]
    public partial bool DoneOver { get; set; }

    [ObservableProperty]
    public partial bool TrashActive { get; set; }

    public IObserveCommand DragStartingCommand { get; }

    public IObserveCommand DropCompletedCommand { get; }

    public IObserveCommand ItemOverCommand { get; }

    public IObserveCommand ItemLeaveCommand { get; }

    public IObserveCommand ListOverCommand { get; }

    public IObserveCommand ListLeaveCommand { get; }

    public IObserveCommand DropOnItemCommand { get; }

    public IObserveCommand DropOnListCommand { get; }

    public IObserveCommand DropOnTrashCommand { get; }

    public IObserveCommand TrashOverCommand { get; }

    public IObserveCommand TrashLeaveCommand { get; }

    public ViewDragDropViewModel()
    {
        DragStartingCommand = MakeDelegateCommand<DragTask>(BeginDrag);
        DropCompletedCommand = MakeDelegateCommand(EndDrag);
        ItemOverCommand = MakeDelegateCommand<DragTask>(x => SetItemOver(x, true));
        ItemLeaveCommand = MakeDelegateCommand<DragTask>(x => SetItemOver(x, false));
        ListOverCommand = MakeDelegateCommand<string>(x => SetListOver(x, true));
        ListLeaveCommand = MakeDelegateCommand<string>(x => SetListOver(x, false));
        DropOnItemCommand = MakeDelegateCommand<DragTask>(DropOnItem);
        DropOnListCommand = MakeDelegateCommand<string>(DropOnList);
        DropOnTrashCommand = MakeDelegateCommand(DropOnTrash);
        TrashOverCommand = MakeDelegateCommand(() => TrashActive = true);
        TrashLeaveCommand = MakeDelegateCommand(() => TrashActive = false);
    }

    private void BeginDrag(DragTask item)
    {
        dragging = item;
        item.IsSource = true;
        IsDragging = true;
    }

    // 強調表示を全て戻す。対象外へ落とした場合は DropCompleted で呼ばれるが、ドロップ成功時は
    // 元の行が再生成されて DropCompleted が届かないため、各ドロップ処理の最後でも呼ぶ
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

    // 同じリスト内で下方向へ動かす場合はドロップ先の後ろに入る
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

    // ドロップ先アイテムの位置へ挿入する (同一リスト内=並べ替え / 別リスト=位置指定の移動)。
    // 同じリスト内で下へ動かす場合はドロップ先の後ろに入る (隣の行へ落として入れ替わらないのを防ぐ)
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

    // リストの空き領域へのドロップは末尾に追加する
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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
