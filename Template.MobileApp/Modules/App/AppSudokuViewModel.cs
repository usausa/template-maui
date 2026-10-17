namespace Template.MobileApp.Modules.App;

using System.Security.Cryptography;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class SudokuDigit : ObservableObject
{
    public int Value { get; }

    public int Column => Value - 1;

    [ObservableProperty]
    public partial int Remaining { get; set; }

    public SudokuDigit(int value)
    {
        Value = value;
    }
}

public sealed partial class AppSudokuViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(250);

    private readonly IDialog dialog;

    private readonly TimeProvider timeProvider;

    private readonly JsonStore store;

    private readonly Session session;

    private readonly IDispatcherTimer ticker;

    private readonly SudokuGame game = new(static x => RandomNumberGenerator.GetInt32(x));

    private SudokuCell? selected;

    private bool visible;

    // Property

    [ObservableProperty]
    public partial SudokuLevel Level { get; set; }

    [ObservableProperty]
    public partial SudokuFrame? Board { get; set; }

    [ObservableProperty]
    public partial bool IsNoteMode { get; set; }

    [ObservableProperty]
    public partial int Mistakes { get; set; }

    [ObservableProperty]
    public partial int HintsLeft { get; set; }

    [ObservableProperty]
    public partial int Remaining { get; set; }

    [ObservableProperty]
    public partial TimeSpan Elapsed { get; set; }

    [ObservableProperty]
    public partial bool CanUndo { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial TimeSpan? BestTime { get; set; }

    [ObservableProperty]
    public partial bool IsNewBest { get; set; }

    public IReadOnlyList<SudokuDigit> Digits { get; } = [.. Enumerable.Range(1, SudokuGame.Size).Select(static x => new SudokuDigit(x))];

    // Command

    public IObserveCommand BackCommand { get; }
    public IObserveCommand NewGameCommand { get; }
    public IObserveCommand LevelCommand { get; }

    public IObserveCommand SelectCommand { get; }
    public IObserveCommand NumberCommand { get; }
    public IObserveCommand EraseCommand { get; }
    public IObserveCommand UndoCommand { get; }
    public IObserveCommand NoteCommand { get; }
    public IObserveCommand HintCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppSudokuViewModel(
        IDispatcher dispatcher,
        IDialog dialog,
        TimeProvider timeProvider,
        JsonStore store,
        Session session)
    {
        this.dialog = dialog;
        this.timeProvider = timeProvider;
        this.store = store;
        this.session = session;

        ticker = dispatcher.CreateTimer();
        ticker.Interval = TickInterval;
        Disposables.Add(ticker.TickAsObservable().Subscribe(_ => UpdateElapsed()));
        Disposables.Add(new DelegateDisposable(ticker.Stop));

        Disposables.Add(session.AsObservable(nameof(Session.IsForeground)).Subscribe(_ =>
        {
            UpdateClock();
            if (visible)
            {
                Save();
            }
        }));

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        NewGameCommand = MakeAsyncCommand(() => NewGameAsync(Level));
        LevelCommand = MakeAsyncCommand<SudokuLevel>(NewGameAsync);

        SelectCommand = MakeDelegateCommand<SudokuCell>(Select);
        NumberCommand = MakeDelegateCommand<int>(Input);
        EraseCommand = MakeDelegateCommand(Erase);
        UndoCommand = MakeDelegateCommand(Undo, () => CanUndo);
        NoteCommand = MakeDelegateCommand(() => IsNoteMode = !IsNoteMode);
        HintCommand = MakeDelegateCommand(Hint, () => HintsLeft > 0);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
        }

        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        visible = true;
        UpdateClock();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        visible = false;
        UpdateClock();
        Save();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task NewGameAsync(SudokuLevel level)
    {
        if (game.IsCompleted || !game.HasProgress || await dialog.ConfirmAsync("新しいゲームを始めますか?"))
        {
            game.NewGame(level);
            ShowGame();
            UpdateClock();
            Save();
        }
    }

    private void Select(SudokuCell cell)
    {
        selected = cell;
        Refresh(null, []);
    }

    private void Input(int value)
    {
        if (selected is { } cell)
        {
            var changed = IsNoteMode ? game.ToggleNote(cell, value) : game.SetValue(cell, value, timeProvider.GetUtcNow());
            if (changed)
            {
                Place(cell);
            }
        }
    }

    private void Erase()
    {
        if ((selected is { } cell) && game.Clear(cell))
        {
            Refresh(null, []);
            Save();
        }
    }

    private void Undo()
    {
        if (game.Undo() is { } cell)
        {
            selected = cell;
            Refresh(null, []);
            Save();
        }
    }

    private void Hint()
    {
        if (game.Hint(selected, timeProvider.GetUtcNow()) is { } cell)
        {
            selected = cell;
            Place(cell);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Place(SudokuCell cell)
    {
        var flash = game.GetCompletedCells(cell);
        if (game.IsCompleted)
        {
            selected = null;
        }

        UpdateClock();
        Refresh(cell, flash);
        Save();
    }

    private void ShowGame()
    {
        selected = null;
        IsNoteMode = false;
        Refresh(null, []);
    }

    private void Refresh(SudokuCell? placed, SudokuCell[] flash)
    {
        Board = new SudokuFrame(game, selected, placed, flash);
        Level = game.Level;
        Mistakes = game.Mistakes;
        HintsLeft = game.HintsLeft;
        Remaining = game.RemainingCount;
        CanUndo = game.CanUndo;
        IsCompleted = game.IsCompleted;
        IsNewBest = game.IsNewBest;
        BestTime = game.BestSeconds > 0 ? TimeSpan.FromSeconds(game.BestSeconds) : null;
        foreach (var digit in Digits)
        {
            digit.Remaining = game.CountRemaining(digit.Value);
        }
    }

    private void Load()
    {
        var snapshot = store.Load(AppJsonContext.Default.SudokuSnapshot);
        if ((snapshot is null) || !game.Import(snapshot))
        {
            game.NewGame(SudokuLevel.Easy);
        }

        ShowGame();
        UpdateElapsed();
    }

    private void Save() =>
        store.Save(game.Export(timeProvider.GetUtcNow()), AppJsonContext.Default.SudokuSnapshot);

    private void UpdateClock()
    {
        var now = timeProvider.GetUtcNow();
        if (visible && session.IsForeground && !game.IsCompleted)
        {
            game.Resume(now);
            ticker.Start();
        }
        else
        {
            game.Pause(now);
            ticker.Stop();
        }

        UpdateElapsed();
    }

    private void UpdateElapsed() => Elapsed = TimeSpan.FromSeconds(Math.Floor(game.GetElapsed(timeProvider.GetUtcNow()).TotalSeconds));
}
