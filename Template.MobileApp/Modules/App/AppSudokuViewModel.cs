namespace Template.MobileApp.Modules.App;

using System.Security.Cryptography;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class AppSudokuViewModel : AppViewModelBase
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(250);

    private readonly IDialog dialog;

    private readonly TimeProvider timeProvider;

    private readonly JsonStore store;

    private readonly Session session;

    private readonly IDispatcherTimer ticker;

    private readonly SudokuGame game = new(static x => RandomNumberGenerator.GetInt32(x));

    private SudokuCell? selected;

    private bool visible;

    [ObservableProperty]
    public partial SudokuLevel Level { get; set; }

    // 盤面 (数字を入れたマスや、そろったマスがあればコントロールが動かす)
    [ObservableProperty]
    public partial SudokuFrame? Board { get; set; }

    // 数字のキーでメモを切り替える
    [ObservableProperty]
    public partial bool IsNoteMode { get; set; }

    [ObservableProperty]
    public partial int Mistakes { get; set; }

    [ObservableProperty]
    public partial int HintsLeft { get; set; }

    // 答えになっていないマスの数
    [ObservableProperty]
    public partial int Remaining { get; set; }

    [ObservableProperty]
    public partial TimeSpan Elapsed { get; set; }

    [ObservableProperty]
    public partial bool CanUndo { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    // 今の難易度のベストタイム (記録なしは null)
    [ObservableProperty]
    public partial TimeSpan? BestTime { get; set; }

    [ObservableProperty]
    public partial bool IsNewBest { get; set; }

    public IReadOnlyList<SudokuDigit> Digits { get; } = SudokuGame.CreateDigits();

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

        // 背面では経過時間を数えない (止めた時点の経過時間を残す)
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

    // 遊んでいる途中なら確かめてから始める
    private async Task NewGameAsync(SudokuLevel level)
    {
        if (game.IsCompleted || !game.HasProgress || await dialog.ConfirmAsync("今のゲームをやめて、新しいゲームを始めますか?"))
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

    // メモのときはメモを切り替え、そうでなければ数字を入れる
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

    // 選んだマス (答えになっていれば置ける数字のいちばん少ないマス) に答えを入れる
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

    // 入れたマスを弾ませ、そろった行・列・ブロックを光らせる。完成したら選んだマスを外す
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

    // 始めたゲーム、または読み込んだゲームを表示する
    private void ShowGame()
    {
        selected = null;
        IsNoteMode = false;
        Refresh(null, []);
    }

    private void Refresh(SudokuCell? placed, IReadOnlyList<SudokuCell> flash)
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
        game.UpdateDigits(Digits);
    }

    // 前の続き (読めなければ新しいゲーム)
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

    // 表示中で前面、完成していない間だけ経過時間を数える
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

    // 秒の単位で表示する
    private void UpdateElapsed() =>
        Elapsed = TimeSpan.FromSeconds(Math.Floor(game.GetElapsed(timeProvider.GetUtcNow()).TotalSeconds));
}
