namespace Template.MobileApp.Modules.UI;

public sealed partial class UIKanbanViewModel : AppViewModelBase
{
    // Field

    private readonly TimeProvider timeProvider;

    private BoardCard? dragging;

    private bool dropAfter;

    // Property

    [ObservableProperty]
    public partial SprintBoard Board { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsDragging { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial int DoneCount { get; set; }

    public IReadOnlyList<string> Avatars { get; private set; } = [];

    [ObservableProperty]
    public partial int MoreMembers { get; set; }

    [ObservableProperty]
    public partial bool IsFiltered { get; set; }

    // Command

    public IObserveCommand DragStartingCommand { get; }

    public IObserveCommand DropCompletedCommand { get; }

    public IObserveCommand CardSideCommand { get; }

    public IObserveCommand CardOverCommand { get; }

    public IObserveCommand CardLeaveCommand { get; }

    public IObserveCommand DropOnCardCommand { get; }

    public IObserveCommand LaneOverCommand { get; }

    public IObserveCommand LaneHeadOverCommand { get; }

    public IObserveCommand LaneHeadLeaveCommand { get; }

    public IObserveCommand LaneLeaveCommand { get; }

    public IObserveCommand DropOnLaneCommand { get; }

    public IObserveCommand DropOnLaneHeadCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIKanbanViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        DragStartingCommand = MakeDelegateCommand<BoardCard>(CommandMode.Simple, BeginDrag);
        DropCompletedCommand = MakeDelegateCommand(CommandMode.Simple, EndDrag);
        CardSideCommand = MakeDelegateCommand<bool>(CommandMode.Simple, x => dropAfter = x);
        CardOverCommand = MakeDelegateCommand<BoardCard>(CommandMode.Simple, x => SetCardOver(x, true));
        CardLeaveCommand = MakeDelegateCommand<BoardCard>(CommandMode.Simple, x => SetCardOver(x, false));
        DropOnCardCommand = MakeDelegateCommand<BoardCard>(CommandMode.Simple, DropOnCard);
        LaneOverCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x =>
        {
            x.IsOver = dragging is not null;
            x.DropSide = dragging is not null ? BoardDropSide.After : BoardDropSide.None;
        });
        LaneLeaveCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x =>
        {
            x.IsOver = false;
            x.DropSide = BoardDropSide.None;
        });
        LaneHeadOverCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x =>
        {
            x.IsOver = dragging is not null;
            x.DropSide = dragging is not null ? BoardDropSide.Before : BoardDropSide.None;
        });
        LaneHeadLeaveCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x =>
        {
            x.IsOver = false;
            x.DropSide = BoardDropSide.None;
        });
        DropOnLaneCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x => DropOnLane(x, false));
        DropOnLaneHeadCommand = MakeDelegateCommand<BoardLane>(CommandMode.Simple, x => DropOnLane(x, true));

        Load();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsFiltered = !IsFiltered;
        Board.Filter(IsFiltered);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Load();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        Board.Add(new BoardCard
        {
            Title = $"新しいタスク {TotalCount + 1}",
            Labels = [CardLabel.Chore],
            Assignee = Board.Members[TotalCount % Board.Members.Count],
            Checked = 0,
            Total = 1,
            Due = today.AddDays(3),
            IsOverdue = false
        });
        Board.Filter(IsFiltered);
        UpdateCount();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load()
    {
        Board = SprintSample.LoadBoard(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
        Avatars = [.. Board.Members.Take(4).Select(static x => x[..1])];
        MoreMembers = Board.Members.Count - Avatars.Count;
        UpdateCount();
    }

    private void BeginDrag(BoardCard card)
    {
        dragging = card;
        card.IsSource = true;
        IsDragging = true;
    }

    private void EndDrag()
    {
        dragging = null;
        IsDragging = false;
        foreach (var lane in Board.Lanes)
        {
            lane.IsOver = false;
            lane.DropSide = BoardDropSide.None;
            foreach (var card in lane.Cards)
            {
                card.IsSource = false;
                card.DropSide = BoardDropSide.None;
            }
        }
    }

    private void SetCardOver(BoardCard target, bool value)
    {
        var over = value && (dragging is not null) && !ReferenceEquals(target, dragging);
        target.DropSide = over ? (dropAfter ? BoardDropSide.After : BoardDropSide.Before) : BoardDropSide.None;
    }

    private void DropOnCard(BoardCard target)
    {
        if ((dragging is { } card) && !ReferenceEquals(card, target) && (Board.FindLane(target) is { } lane))
        {
            Board.Move(card, lane, dropAfter ? NextOf(lane, target, card) : target);
            UpdateCount();
        }

        EndDrag();
    }

    private static BoardCard? NextOf(BoardLane lane, BoardCard target, BoardCard card)
    {
        for (var i = lane.Cards.IndexOf(target) + 1; i < lane.Cards.Count; i++)
        {
            if (!ReferenceEquals(lane.Cards[i], card))
            {
                return lane.Cards[i];
            }
        }

        return null;
    }

    private void DropOnLane(BoardLane lane, bool head)
    {
        if (dragging is { } card)
        {
            Board.Move(card, lane, head ? lane.Cards.FirstOrDefault(x => !ReferenceEquals(x, card)) : null);
            UpdateCount();
        }

        EndDrag();
    }

    private void UpdateCount()
    {
        TotalCount = Board.TotalCount;
        DoneCount = Board.DoneCount;
    }
}
