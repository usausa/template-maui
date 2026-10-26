namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum CardLabel
{
    Bug,
    Feat,
    Chore,
    Docs,
    Ios,
    Android,
    Ui
}

public enum BoardDropSide
{
    None,
    Before,
    After
}

public enum LaneKind
{
    Todo,
    Doing,
    Review,
    Done
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class BoardCard : ObservableObject
{
    public required string Title { get; init; }

    public required IReadOnlyList<CardLabel> Labels { get; init; }

    public required string Assignee { get; init; }

    public required int Checked { get; init; }

    public required int Total { get; init; }

    public required DateOnly Due { get; init; }

    public required bool IsOverdue { get; init; }

    [ObservableProperty]
    public partial bool IsSource { get; set; }

    [ObservableProperty]
    public partial BoardDropSide DropSide { get; set; }

    [ObservableProperty]
    public partial bool IsShown { get; set; } = true;
}

public sealed partial class BoardLane : ObservableObject
{
    public required string Name { get; init; }

    public required LaneKind Kind { get; init; }

    public required int WipLimit { get; init; }

    public ObservableCollection<BoardCard> Cards { get; } = [];

    [ObservableProperty]
    public partial bool IsOver { get; set; }

    [ObservableProperty]
    public partial BoardDropSide DropSide { get; set; }

    [ObservableProperty]
    public partial int Count { get; set; }

    [ObservableProperty]
    public partial bool IsOverWip { get; set; }

    public void Refresh()
    {
        Count = Cards.Count;
        IsOverWip = (WipLimit > 0) && (Cards.Count > WipLimit);
    }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class SprintBoard
{
    public required string Name { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required IReadOnlyList<string> Members { get; init; }

    public required IReadOnlyList<BoardLane> Lanes { get; init; }

    public int RemainDays => Math.Max(0, End.DayNumber - Today.DayNumber);

    public required DateOnly Today { get; init; }

    public int TotalCount => Lanes.Sum(static x => x.Cards.Count);

    public int DoneCount => Lanes.Where(static x => x.Kind == LaneKind.Done).Sum(static x => x.Cards.Count);

    public BoardLane? FindLane(BoardCard card) => Lanes.FirstOrDefault(x => x.Cards.Contains(card));

    public IEnumerable<BoardCard> EnumerateCards() => Lanes.SelectMany(static x => x.Cards);

    public void Filter(bool overdueOnly)
    {
        foreach (var card in EnumerateCards())
        {
            card.IsShown = !overdueOnly || card.IsOverdue;
        }
    }

    public void Add(BoardCard card)
    {
        var lane = Lanes.First(static x => x.Kind == LaneKind.Todo);
        lane.Cards.Insert(0, card);
        lane.Refresh();
    }

    public void Move(BoardCard card, BoardLane destination, BoardCard? before)
    {
        var source = FindLane(card);
        if ((source is null) || ReferenceEquals(card, before))
        {
            return;
        }

        source.Cards.Remove(card);
        var index = (before is not null) && destination.Cards.Contains(before)
            ? destination.Cards.IndexOf(before)
            : destination.Cards.Count;
        destination.Cards.Insert(index, card);

        source.Refresh();
        destination.Refresh();
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class SprintSample
{
    private static readonly (LaneKind Kind, string Name, int Wip)[] LanePlan =
    [
        (LaneKind.Todo, "未着手", 0),
        (LaneKind.Doing, "進行中", 3),
        (LaneKind.Review, "レビュー", 2),
        (LaneKind.Done, "完了", 0)
    ];

    private static readonly (int Lane, string Title, CardLabel[] Labels, string Assignee, int Checked, int Total, int Due)[] CardPlan =
    [
        (0, "起動直後にスプラッシュが残る", [CardLabel.Bug, CardLabel.Ios], "鈴木", 0, 3, -5),
        (0, "時刻表の種別フィルタを追加", [CardLabel.Feat], "高橋", 0, 5, -1),
        (0, "依存パッケージの更新", [CardLabel.Chore], "中村", 0, 2, 1),
        (0, "カンバンの並び替えを保存", [CardLabel.Feat, CardLabel.Ui], "田中", 0, 4, 0),
        (0, "UI サンプルの手順書を更新", [CardLabel.Docs], "鈴木", 0, 1, 1),
        (1, "乗換時刻の表示がずれる", [CardLabel.Bug, CardLabel.Android], "佐藤", 1, 3, -6),
        (1, "座席図のピンチズーム実装", [CardLabel.Feat, CardLabel.Ui], "田中", 4, 7, -3),
        (1, "経路候補タブの切替", [CardLabel.Feat], "佐藤", 2, 4, -4),
        (1, "チケットのミシン目クリップ", [CardLabel.Feat], "中村", 1, 3, -2),
        (2, "ガントの依存線を調整", [CardLabel.Ui], "鈴木", 3, 3, -2),
        (2, "時刻表の現在行ハイライト", [CardLabel.Feat], "高橋", 2, 2, -1),
        (3, "メニュー構成の見直し", [CardLabel.Chore], "佐藤", 2, 2, -9),
        (3, "共通ジェスチャの追加", [CardLabel.Feat], "田中", 5, 5, -8),
        (3, "スクロール同期の実装", [CardLabel.Feat], "山本", 4, 4, -7),
        (3, "ローソク足の差分更新", [CardLabel.Feat], "高橋", 3, 3, -7),
        (3, "マインスイーパの配色変更", [CardLabel.Ui], "中村", 2, 2, -6),
        (3, "経路検索の逆方向対応", [CardLabel.Feat], "佐藤", 6, 6, -5),
        (3, "座席のヒットテスト", [CardLabel.Feat], "田中", 4, 4, -4),
        (3, "時刻表の種別色", [CardLabel.Ui], "高橋", 2, 2, -3)
    ];

    public static SprintBoard LoadBoard(DateOnly today)
    {
        var lanes = LanePlan
            .Select(x => new BoardLane
            {
                Name = x.Name,
                Kind = x.Kind,
                WipLimit = x.Wip
            })
            .ToArray();

        foreach (var (index, title, labels, assignee, done, total, offset) in CardPlan)
        {
            var due = today.AddDays(offset);
            lanes[index].Cards.Add(new BoardCard
            {
                Title = title,
                Labels = labels,
                Assignee = assignee,
                Checked = done,
                Total = total,
                Due = due,
                IsOverdue = (due < today) && (lanes[index].Kind != LaneKind.Done)
            });
        }

        foreach (var lane in lanes)
        {
            lane.Refresh();
        }

        return new SprintBoard
        {
            Name = "Sprint 24",
            Start = today.AddDays(-7),
            End = today.AddDays(6),
            Today = today,
            Members = ["佐藤", "鈴木", "高橋", "田中", "中村", "山本", "渡辺"],
            Lanes = lanes
        };
    }
}
