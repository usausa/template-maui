namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class AppTodoViewModel : AppViewModelBase
{
    // 完了を切り替えてから行を動かすまでの間 (チェックの動きを見せる)
    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(400);

    // 元に戻すの帯を出しておく時間
    private static readonly TimeSpan UndoDuration = TimeSpan.FromSeconds(4);

    private readonly IDispatcher dispatcher;

    // 元に戻すの帯を閉じる予約。破棄は Disposables に任せる
    private SerialDisposable UndoTimer { get; } = new();

    [Scope]
    [ObservableProperty]
    public partial AppTodoContext Context { get; set; } = default!;

    public IObserveCommand BackCommand { get; }
    public IObserveCommand ShowDoneCommand { get; }

    public IObserveCommand AddCommand { get; }
    public IObserveCommand EditCommand { get; }
    public IObserveCommand DoneCommand { get; }
    public IObserveCommand ImportantCommand { get; }
    public IObserveCommand DeleteCommand { get; }
    public IObserveCommand UndoCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppTodoViewModel(IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;

        Disposables.Add(UndoTimer);

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
#pragma warning disable IDE0200
        ShowDoneCommand = MakeDelegateCommand(() => Context.List.ToggleShowDone());
#pragma warning restore IDE0200

        AddCommand = MakeAsyncCommand(() => OpenEditAsync(null));
        EditCommand = MakeAsyncCommand<TodoItem>(OpenEditAsync);
        DoneCommand = MakeAsyncCommand<TodoItem>(ToggleDoneAsync);
#pragma warning disable IDE0200
        ImportantCommand = MakeAsyncCommand<TodoItem>(x => Context.ToggleImportantAsync(x));
#pragma warning restore IDE0200
        DeleteCommand = MakeAsyncCommand<TodoItem>(DeleteAsync);
        UndoCommand = MakeAsyncCommand(UndoAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // 編集から戻ったときは、文脈が一覧に反映している。編集の画面で消したときは元に戻すの帯を出す
    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        var task = Task.CompletedTask;
        if (!context.Attribute.IsRestore())
        {
            task = Context.LoadAsync();
        }
        else
        {
            ShowUndo();
        }

        return task;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 元に戻すの帯は閉じる
    private Task OpenEditAsync(TodoItem? item)
    {
        CloseUndo();
        Context.BeginEdit(item);
        return Navigator.PushAsync(ViewId.AppTodoEdit);
    }

    // チェックの動きを見せてから、行を完了 (または元) のグループへ動かす。
    // 待つ間も続けて押せるように、動かすのはコマンドの外 (実行中は画面全体のオーバーレイが入力を止める)
    private async Task ToggleDoneAsync(TodoItem item)
    {
        await Context.ToggleDoneAsync(item);
        dispatcher.DispatchDelayed(MoveDelay, () => Context.List.Move(item));
    }

    private async Task DeleteAsync(TodoItem item)
    {
        await Context.DeleteAsync(item);
        ShowUndo();
    }

    private Task UndoAsync()
    {
        UndoTimer.Disposable = null;
        return Context.UndoAsync();
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // 消した行があれば、時間が過ぎると帯を閉じる
    private void ShowUndo()
    {
        if (Context.DeletedItem is not null)
        {
            UndoTimer.Disposable = Observable.Timer(UndoDuration)
                .ObserveOnCurrentContext()
                .Subscribe(_ => Context.ClearDeleted());
        }
    }

    private void CloseUndo()
    {
        UndoTimer.Disposable = null;
        Context.ClearDeleted();
    }
}
