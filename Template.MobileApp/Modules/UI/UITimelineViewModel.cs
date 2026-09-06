namespace Template.MobileApp.Modules.UI;

public sealed class UITimelineEvent
{
    public string Time { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Tag1 { get; init; } = string.Empty;
    public string Tag2 { get; init; } = string.Empty;
    public Color DotColor { get; init; } = Colors.Gray;
    public bool Done { get; init; }
    public bool Current { get; init; }

    public Color DotFill => Done || Current ? DotColor : Colors.White;
    public Color DotStroke => Done || Current ? Colors.White : DotColor;
    public double RowOpacity => Done ? 0.7 : 1.0;
}

public sealed class UITimelineViewModel : AppViewModelBase
{
    public string DateLabel { get; } = "AWS Summit Japan 2025 · Day 1 (2025/06/25)";

    public IReadOnlyList<UITimelineEvent> Events { get; } =
    [
        new()
        {
            Time = "09:00", Title = "基調講演 · AWS イノベーション戦略",
            Description = "生成AI × クラウドネイティブで変わるアーキテクチャ設計の展望",
            Tag1 = "基調講演", Tag2 = "AI/ML",
            DotColor = Color.FromArgb("#E53935"),
            Done = true
        },
        new()
        {
            Time = "10:30", Title = ".NET on AWS — Lambda & ECS 最新アップデート",
            Description = "NativeAOT Lambda、ARM64 Graviton3、.NET 10 サポートの詳細解説",
            Tag1 = ".NET", Tag2 = "サーバーレス",
            DotColor = Color.FromArgb("#1E88E5"),
            Done = true
        },
        new()
        {
            Time = "12:00", Title = "ランチセッション · CDK v3 ハンズオン",
            Description = "C# で書く AWS CDK v3 — Stack 設計パターンとベストプラクティス",
            Tag1 = "ハンズオン", Tag2 = "IaC",
            DotColor = Color.FromArgb("#43A047"),
            Done = true
        },
        new()
        {
            Time = "13:30", Title = "Amazon Bedrock × .NET 統合事例",
            Description = "AWS SDK for .NET を使った生成AI チャットボットの本番投入レポート",
            Tag1 = "AI/ML", Tag2 = ".NET",
            DotColor = Color.FromArgb("#8E24AA"),
            Current = true
        },
        new()
        {
            Time = "15:00", Title = "DynamoDB 設計パターン深掘り",
            Description = "シングルテーブル設計と GSI 活用 — C# SDK による実装例",
            Tag1 = "データベース", Tag2 = "アーキテクチャ",
            DotColor = Color.FromArgb("#FB8C00")
        },
        new()
        {
            Time = "16:30", Title = "ECS Fargate + App Mesh で作るマイクロサービス",
            Description = "サービスメッシュとブルー/グリーンデプロイの組み合わせ実践",
            Tag1 = "コンテナ", Tag2 = "DevOps",
            DotColor = Color.FromArgb("#43A047")
        },
        new()
        {
            Time = "18:00", Title = "ネットワーキングパーティ",
            Description = "スポンサーブース巡り · AWS Hero / Community Builder との交流",
            Tag1 = "交流", Tag2 = "コミュニティ",
            DotColor = Color.FromArgb("#1E88E5")
        }
    ];

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
