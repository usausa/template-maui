namespace Template.MobileApp.Modules.UI;

public sealed class UIProfileStat
{
    public string Value { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;
}

public sealed class UIProfileInterest
{
    public string Label { get; init; } = string.Empty;
    public Color BackgroundColor { get; init; } = Colors.LightGray;
    public Color TextColor { get; init; } = Colors.DarkGray;
}

public sealed class UIProfilePhoto
{
    public string Image { get; init; } = string.Empty;
}

public sealed partial class UIProfileViewModel : AppViewModelBase
{
    public string UserName { get; } = "山奥 うさぎ, 29";
    public string Occupation { get; } = "C# / .NET エンジニア · AWS ソリューションアーキテクト";
    public string Location { get; } = "東京都 渋谷区";
    public string Bio { get; } =
        ".NET と AWS を組み合わせたバックエンド設計が専門。ECS/Lambda を駆使したサーバーレスアーキテクチャと、C# で書くインフラ as Code に情熱を注いでいます。";

    public IReadOnlyList<UIProfileStat> Stats { get; } =
    [
        new() { Value = "247", Caption = "投稿" },
        new() { Value = "3.8k", Caption = "フォロワー" },
        new() { Value = "420", Caption = "フォロー中" }
    ];

    public IReadOnlyList<UIProfileInterest> Interests { get; } =
    [
        new() { Label = "C# / .NET", BackgroundColor = Color.FromArgb("#E3F2FD"), TextColor = Color.FromArgb("#1565C0") },
        new() { Label = "AWS Lambda", BackgroundColor = Color.FromArgb("#FFF8E1"), TextColor = Color.FromArgb("#E65100") },
        new() { Label = "Amazon ECS", BackgroundColor = Color.FromArgb("#E8F5E9"), TextColor = Color.FromArgb("#1B5E20") },
        new() { Label = "CDK / Terraform", BackgroundColor = Color.FromArgb("#FCE4EC"), TextColor = Color.FromArgb("#880E4F") },
        new() { Label = ".NET MAUI", BackgroundColor = Color.FromArgb("#F3E5F5"), TextColor = Color.FromArgb("#6A1B9A") },
        new() { Label = "DynamoDB", BackgroundColor = Color.FromArgb("#E0F2F1"), TextColor = Color.FromArgb("#004D40") }
    ];

    public IReadOnlyList<UIProfilePhoto> Photos { get; } =
    [
        new() { Image = "usa1_full.jpg" },
        new() { Image = "usa2_full.jpg" },
        new() { Image = "usa3_full.jpg" },
        new() { Image = "usa4_full.jpg" },
        new() { Image = "usa5_full.jpg" },
        new() { Image = "usa6_full.jpg" }
    ];

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    [ObservableProperty]
    public partial bool IsLiked { get; set; } = true;

    [ObservableProperty]
    public partial bool IsStarred { get; set; } = true;

    public IObserveCommand FollowCommand { get; }

    public IObserveCommand LikeCommand { get; }

    public IObserveCommand StarCommand { get; }

    public UIProfileViewModel()
    {
        FollowCommand = MakeDelegateCommand(() => IsFollowed = !IsFollowed);
        LikeCommand = MakeDelegateCommand(() => IsLiked = !IsLiked);
        StarCommand = MakeDelegateCommand(() => IsStarred = !IsStarred);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
