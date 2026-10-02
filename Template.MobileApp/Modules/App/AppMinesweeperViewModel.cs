namespace Template.MobileApp.Modules.App;

using System.Security.Cryptography;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class AppMinesweeperViewModel : AppViewModelBase
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(250);

    private static readonly TimeSpan FlagVibration = TimeSpan.FromMilliseconds(30);

    private static readonly TimeSpan ExplodeVibration = TimeSpan.FromMilliseconds(400);

    private readonly IVibration vibration;

    private readonly TimeProvider timeProvider;

    private readonly JsonStore store;

    private readonly IDispatcherTimer ticker;

    private readonly MinesweeperGame game = new(static x => RandomNumberGenerator.GetInt32(x));

    private bool visible;

    [ObservableProperty]
    public partial MinesweeperLevel Level { get; set; }

    // 盤面 (直前に見せたマスがあればコントロールが覆いを消す)
    [ObservableProperty]
    public partial MinesweeperFrame? Board { get; set; }

    [ObservableProperty]
    public partial MinesweeperState State { get; set; }

    [ObservableProperty]
    public partial int RemainingMines { get; set; }

    [ObservableProperty]
    public partial int ElapsedSeconds { get; set; }

    // タップで旗を立てる
    [ObservableProperty]
    public partial bool IsFlagMode { get; set; }

    // 今の難易度のベストタイム (秒。0 は記録なし)
    [ObservableProperty]
    public partial int BestSeconds { get; set; }

    [ObservableProperty]
    public partial bool IsNewBest { get; set; }

    public IObserveCommand BackCommand { get; }
    public IObserveCommand NewGameCommand { get; }
    public IObserveCommand LevelCommand { get; }
    public IObserveCommand FlagModeCommand { get; }

    public IObserveCommand TapCommand { get; }
    public IObserveCommand LongPressCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppMinesweeperViewModel(
        IDispatcher dispatcher,
        IVibration vibration,
        TimeProvider timeProvider,
        JsonStore store)
    {
        this.vibration = vibration;
        this.timeProvider = timeProvider;
        this.store = store;

        ticker = dispatcher.CreateTimer();
        ticker.Interval = TickInterval;
        Disposables.Add(ticker.TickAsObservable().Subscribe(_ => UpdateElapsed()));
        Disposables.Add(new DelegateDisposable(ticker.Stop));

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        NewGameCommand = MakeDelegateCommand(NewGame);
        LevelCommand = MakeDelegateCommand<MinesweeperLevel>(ChangeLevel);
        FlagModeCommand = MakeDelegateCommand<bool>(x => IsFlagMode = x);

        TapCommand = MakeDelegateCommand<MinesweeperCell>(Tap);
        LongPressCommand = MakeDelegateCommand<MinesweeperCell>(LongPress);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
            ChangeLevel(MinesweeperLevel.Beginner);
        }

        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        visible = true;
        UpdateTicker();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        visible = false;
        UpdateTicker();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ChangeLevel(MinesweeperLevel level)
    {
        Level = level;
        NewGame();
    }

    private void NewGame()
    {
        game.NewGame(Level);
        Refresh([]);
    }

    private void Tap(MinesweeperCell cell)
    {
        var now = timeProvider.GetUtcNow();
        Apply(IsFlagMode ? game.ToggleFlag(cell, now) : game.Open(cell, now));
    }

    private void LongPress(MinesweeperCell cell) =>
        Apply(game.ToggleFlag(cell, timeProvider.GetUtcNow()));

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // 旗と負けは振動で知らせ、ベストタイムを更新したら保存する
    private void Apply(MinesweeperMove move)
    {
        if (move.Type == MinesweeperMoveType.Flagged)
        {
            vibration.Vibrate(FlagVibration);
            Refresh([]);
        }
        else if (move.Type == MinesweeperMoveType.Opened)
        {
            if (game.State == MinesweeperState.Lost)
            {
                vibration.Vibrate(ExplodeVibration);
            }
            else if (game.IsNewBest)
            {
                store.Save(game.Export(), AppJsonContext.Default.MinesweeperSnapshot);
            }

            Refresh(move.Revealed);
        }
    }

    // 難易度ごとのベストタイム
    private void Load()
    {
        if (store.Load(AppJsonContext.Default.MinesweeperSnapshot) is { } snapshot)
        {
            game.Import(snapshot);
        }
    }

    private void Refresh(IReadOnlyList<MinesweeperCell> revealed)
    {
        Board = new MinesweeperFrame(game, revealed);
        State = game.State;
        RemainingMines = game.RemainingMines;
        BestSeconds = game.BestSeconds;
        IsNewBest = game.IsNewBest;
        UpdateElapsed();
        UpdateTicker();
    }

    private void UpdateElapsed() => ElapsedSeconds = GetSeconds();

    // 始まったら 1 秒から数える
    private int GetSeconds() => (int)Math.Ceiling(game.GetElapsed(timeProvider.GetUtcNow()).TotalSeconds);

    // 表示中で、始まってから終わるまでの間だけ経過時間を更新する
    private void UpdateTicker()
    {
        if (visible && (game.State == MinesweeperState.Playing))
        {
            ticker.Start();
        }
        else
        {
            ticker.Stop();
        }
    }
}
