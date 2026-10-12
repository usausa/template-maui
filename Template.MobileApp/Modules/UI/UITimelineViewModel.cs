namespace Template.MobileApp.Modules.UI;

public enum TimelineStatus
{
    Upcoming,
    Current,
    Done
}

// Tag が null でブックマークでもないものはすべて
public sealed record UITimelineFilterOption(string? Tag, bool Bookmarked, string Name);

public sealed partial class UITimelineEvent : ObservableObject
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Tag1 { get; init; } = string.Empty;
    public string Tag2 { get; init; } = string.Empty;
    public Color DotColor { get; init; } = Colors.Gray;

    [ObservableProperty(NotifyAlso = [nameof(Done), nameof(Current), nameof(DotFill), nameof(DotStroke), nameof(RowOpacity), nameof(RowBackground)])]
    public partial TimelineStatus Status { get; set; }

    [ObservableProperty]
    public partial bool IsBookmarked { get; set; }

    // 今の時刻の線をこの行の下に出す
    [ObservableProperty]
    public partial bool IsNowAfter { get; set; }

    public bool Done => Status == TimelineStatus.Done;
    public bool Current => Status == TimelineStatus.Current;

    public Color DotFill => Done || Current ? DotColor : Colors.White;
    public Color DotStroke => Done || Current ? Colors.White : DotColor;
    public double RowOpacity => Done ? 0.7 : 1.0;

    public Color RowBackground => Current ? DotColor.WithAlpha(0.08f) : Colors.Transparent;

    public bool HasTag(string tag) => (Tag1 == tag) || (Tag2 == tag);
}

public sealed partial class UITimelineViewModel : AppViewModelBase
{
    // 90 分ごとに 75 分のセッション。4 番目が今の 30 分の区切りに始まる
    private const int SessionInterval = 90;

    private const int SessionLength = 75;

    private const int CurrentIndex = 3;

    private static readonly UITimelineFilterOption AllFilter = new(null, false, "すべて");

    private readonly IDispatcherTimer timer;

    private readonly IReadOnlyList<UITimelineEvent> sessions;

    private int minute = -1;

    public DateTime Today { get; }

    [ObservableProperty]
    public partial DateTime Now { get; set; }

    public IReadOnlyList<UITimelineFilterOption> Filters { get; }

    [ObservableProperty]
    public partial UITimelineFilterOption SelectedFilter { get; set; } = AllFilter;

    [ObservableProperty]
    public partial IReadOnlyList<UITimelineEvent> Events { get; set; } = [];

    public IObserveCommand BookmarkCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITimelineViewModel(IDispatcher dispatcher)
    {
        var now = DateTime.Now;
        Today = now.Date;
        var first = now.Date.AddHours(now.Hour).AddMinutes(now.Minute - (now.Minute % 30) - (SessionInterval * CurrentIndex));
        sessions =
        [
            new()
            {
                Start = first, End = first.AddMinutes(SessionLength),
                Title = "基調講演 · AWS イノベーション戦略",
                Description = "生成AI × クラウドネイティブで変わるアーキテクチャ設計の展望",
                Tag1 = "基調講演", Tag2 = "AI/ML",
                DotColor = Color.FromArgb("#E53935")
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval), End = first.AddMinutes(SessionInterval + SessionLength),
                Title = ".NET on AWS - Lambda & ECS 最新アップデート",
                Description = "NativeAOT Lambda、ARM64 Graviton3、.NET 10 サポートの詳細解説",
                Tag1 = ".NET", Tag2 = "サーバーレス",
                DotColor = Color.FromArgb("#1E88E5"),
                IsBookmarked = true
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval * 2), End = first.AddMinutes((SessionInterval * 2) + SessionLength),
                Title = "ランチセッション · CDK v3 ハンズオン",
                Description = "C# で書く AWS CDK v3 - Stack 設計パターンとベストプラクティス",
                Tag1 = "ハンズオン", Tag2 = "IaC",
                DotColor = Color.FromArgb("#43A047")
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval * 3), End = first.AddMinutes((SessionInterval * 3) + SessionLength),
                Title = "Amazon Bedrock × .NET 統合事例",
                Description = "AWS SDK for .NET を使った生成AI チャットボットの本番投入レポート",
                Tag1 = "AI/ML", Tag2 = ".NET",
                DotColor = Color.FromArgb("#8E24AA")
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval * 4), End = first.AddMinutes((SessionInterval * 4) + SessionLength),
                Title = "DynamoDB 設計パターン深掘り",
                Description = "シングルテーブル設計と GSI 活用 - C# SDK による実装例",
                Tag1 = "データベース", Tag2 = "アーキテクチャ",
                DotColor = Color.FromArgb("#FB8C00"),
                IsBookmarked = true
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval * 5), End = first.AddMinutes((SessionInterval * 5) + SessionLength),
                Title = "ECS Fargate + App Mesh で作るマイクロサービス",
                Description = "サービスメッシュとブルー/グリーンデプロイの組み合わせ実践",
                Tag1 = "コンテナ", Tag2 = "DevOps",
                DotColor = Color.FromArgb("#43A047")
            },
            new()
            {
                Start = first.AddMinutes(SessionInterval * 6), End = first.AddMinutes((SessionInterval * 6) + SessionLength),
                Title = "ネットワーキングパーティ",
                Description = "スポンサーブース巡り · AWS Hero / Community Builder との交流",
                Tag1 = "交流", Tag2 = "コミュニティ",
                DotColor = Color.FromArgb("#1E88E5")
            }
        ];

        Filters =
        [
            AllFilter,
            new(null, true, "☆ ブックマーク"),
            .. sessions.SelectMany(static x => new[] { x.Tag1, x.Tag2 }).Distinct().Select(static x => new UITimelineFilterOption(x, false, x))
        ];
        SubscribeSelectedFilter(_ => Refresh());

        BookmarkCommand = MakeDelegateCommand<UITimelineEvent>(ToggleBookmark);

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => UpdateStatus()));
        UpdateStatus();
        Refresh();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ToggleBookmark(UITimelineEvent session)
    {
        session.IsBookmarked = !session.IsBookmarked;
        if (SelectedFilter.Bookmarked)
        {
            Refresh();
        }
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    // 分が変わったら終わった・進行中を見直す
    private void UpdateStatus()
    {
        var now = DateTime.Now;
        if (now.Minute != minute)
        {
            minute = now.Minute;
            Now = now;
            foreach (var session in sessions)
            {
                session.Status = now >= session.End
                    ? TimelineStatus.Done
                    : now >= session.Start ? TimelineStatus.Current : TimelineStatus.Upcoming;
            }
            MarkNow();
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Refresh()
    {
        var filter = SelectedFilter;
        Events = sessions
            .Where(x => filter.Bookmarked ? x.IsBookmarked : ((filter.Tag is null) || x.HasTag(filter.Tag)))
            .ToArray();
        MarkNow();
    }

    // 表示している中で最後に始まったセッションの下に今の時刻の線
    private void MarkNow()
    {
        var last = Events.LastOrDefault(x => x.Start <= Now);
        foreach (var session in sessions)
        {
            session.IsNowAfter = session == last;
        }
    }
}
