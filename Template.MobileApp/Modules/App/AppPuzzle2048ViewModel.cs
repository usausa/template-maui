namespace Template.MobileApp.Modules.App;

using System.Security.Cryptography;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class AppPuzzle2048ViewModel : AppViewModelBase
{
    private readonly JsonStore store;

    private readonly Puzzle2048Game game = new(static x => RandomNumberGenerator.GetInt32(x));

    // 盤面 (直前の 1 手があればコントロールが動かす)
    [ObservableProperty]
    public partial Puzzle2048Frame? Board { get; set; }

    [ObservableProperty]
    public partial int Score { get; set; }

    [ObservableProperty]
    public partial int BestScore { get; set; }

    // 2048 に届いて、続けるかを聞いている
    [ObservableProperty]
    public partial bool IsWon { get; set; }

    [ObservableProperty]
    public partial bool IsOver { get; set; }

    public IObserveCommand BackCommand { get; }
    public IObserveCommand NewGameCommand { get; }
    public IObserveCommand ContinueCommand { get; }
    public IObserveCommand SwipeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppPuzzle2048ViewModel(JsonStore store)
    {
        this.store = store;

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        NewGameCommand = MakeDelegateCommand(NewGame);
        ContinueCommand = MakeDelegateCommand(Continue);
        SwipeCommand = MakeDelegateCommand<Puzzle2048Direction>(Swipe);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
            Apply(null);
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void NewGame()
    {
        game.NewGame();
        Apply(null);
        Save();
    }

    private void Continue()
    {
        game.KeepPlaying = true;
        Apply(null);
        Save();
    }

    private void Swipe(Puzzle2048Direction direction)
    {
        var move = game.Move(direction);
        if (move.Moved)
        {
            Apply(move);
            Save();
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Apply(Puzzle2048Move? move)
    {
        Board = new Puzzle2048Frame(game.Tiles, move);
        Score = game.Score;
        BestScore = game.BestScore;
        IsWon = game.IsWinPending;
        IsOver = game.IsOver;
    }

    // 前の続き (読めなければ新しいゲーム)
    private void Load()
    {
        var snapshot = store.Load(AppJsonContext.Default.Puzzle2048Snapshot);
        if ((snapshot is null) || !game.Import(snapshot))
        {
            game.NewGame();
        }
    }

    private void Save() =>
        store.Save(game.Export(), AppJsonContext.Default.Puzzle2048Snapshot);
}
