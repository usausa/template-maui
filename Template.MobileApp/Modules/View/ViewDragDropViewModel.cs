namespace Template.MobileApp.Modules.View;

public sealed class DragTask
{
    public string Text { get; }

    public Color Accent { get; }

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

    [ObservableProperty]
    public partial bool TrashActive { get; set; }

    public IObserveCommand DragStartingCommand { get; }

    public IObserveCommand DropOnItemCommand { get; }

    public IObserveCommand DropOnListCommand { get; }

    public IObserveCommand DropOnTrashCommand { get; }

    public IObserveCommand TrashOverCommand { get; }

    public IObserveCommand TrashLeaveCommand { get; }

    public ViewDragDropViewModel()
    {
        DragStartingCommand = MakeDelegateCommand<DragTask>(x => dragging = x);
        DropOnItemCommand = MakeDelegateCommand<DragTask>(DropOnItem);
        DropOnListCommand = MakeDelegateCommand<string>(DropOnList);
        DropOnTrashCommand = MakeDelegateCommand(DropOnTrash);
        TrashOverCommand = MakeDelegateCommand(() => TrashActive = true);
        TrashLeaveCommand = MakeDelegateCommand(() => TrashActive = false);
    }

    // ドロップ先アイテムの位置へ挿入する (同一リスト内=並べ替え / 別リスト=位置指定の移動)
    private void DropOnItem(DragTask target)
    {
        var item = dragging;
        dragging = null;
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

        source.Remove(item);
        destination.Insert(destination.IndexOf(target), item);
    }

    // リストの空き領域へのドロップは末尾に追加する
    private void DropOnList(string name)
    {
        var item = dragging;
        dragging = null;
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
    }

    private void DropOnTrash()
    {
        var item = dragging;
        dragging = null;
        TrashActive = false;
        if (item is null)
        {
            return;
        }

        FindList(item)?.Remove(item);
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
