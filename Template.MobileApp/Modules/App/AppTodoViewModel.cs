namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class AppTodoViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan MoveDelay = TimeSpan.FromMilliseconds(400);

    private static readonly TimeSpan UndoDuration = TimeSpan.FromSeconds(4);

    private readonly IDispatcher dispatcher;

    // Property

    private SerialDisposable UndoTimer { get; } = new();

    [Scope]
    [ObservableProperty]
    public partial AppTodoContext Context { get; set; } = default!;

    // Command

    public IObserveCommand BackCommand { get; }
    public IObserveCommand ShowDoneCommand { get; }

    public IObserveCommand SectionCommand { get; }

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
        SectionCommand = MakeDelegateCommand<TodoSection>(static x => x.ToggleExpanded());

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

    private Task OpenEditAsync(TodoItem? item)
    {
        HideUndo();
        Context.BeginEdit(item);
        return Navigator.PushAsync(ViewId.AppTodoEdit);
    }

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

    private void ShowUndo()
    {
        if (Context.DeletedItem is not null)
        {
            UndoTimer.Disposable = Observable.Timer(UndoDuration)
                .ObserveOnCurrentContext()
                .Subscribe(_ => Context.ClearDeleted());
        }
    }

    private void HideUndo()
    {
        UndoTimer.Disposable = null;
        Context.ClearDeleted();
    }
}
