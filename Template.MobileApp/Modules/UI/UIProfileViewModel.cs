namespace Template.MobileApp.Modules.UI;

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

public sealed record UIProfilePost(DateTime Posted, string Text, int Likes, int Comments);

public sealed record UIProfileLike(string Avatar, string Author, DateTime Posted, string Text);

public sealed partial class UIProfileViewModel : AppViewModelBase
{
    public string UserName { get; } = "山奥 うさぎ, 29";
    public string Occupation { get; } = "C# / .NET エンジニア · AWS ソリューションアーキテクト";
    public string Location { get; } = "東京都 渋谷区";
    public string Bio { get; } =
        ".NET と AWS を組み合わせたバックエンド設計が専門。ECS/Lambda を駆使したサーバーレスアーキテクチャと、C# で書くインフラ as Code に情熱を注いでいます。";

    public int Posts { get; } = 247;

    public int Following { get; } = 420;

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
        new() { Image = "gallery01.jpg" },
        new() { Image = "gallery02.jpg" },
        new() { Image = "gallery03.jpg" },
        new() { Image = "gallery04.jpg" },
        new() { Image = "gallery05.jpg" },
        new() { Image = "gallery06.jpg" }
    ];

    public IReadOnlyList<string> Tabs { get; } = ["写真", "投稿", "いいね"];

    public IReadOnlyList<UIProfilePost> PostList { get; }

    public IReadOnlyList<UIProfileLike> LikeList { get; }

    public int Followers { get; } = 3812;

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    [ObservableProperty]
    public partial bool IsLiked { get; set; } = true;

    [ObservableProperty]
    public partial bool IsStarred { get; set; } = true;

    [ObservableProperty]
    public partial int SelectedTab { get; set; }

    [ObservableProperty]
    public partial bool IsPhotoViewerVisible { get; set; }

    [ObservableProperty]
    public partial int PhotoPosition { get; set; }

    public IObserveCommand FollowCommand { get; }

    public IObserveCommand LikeCommand { get; }

    public IObserveCommand StarCommand { get; }

    public IObserveCommand OpenPhotoCommand { get; }

    public IObserveCommand ClosePhotoCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIProfileViewModel()
    {
        var now = DateTime.Now;
        PostList =
        [
            new(now.AddHours(-2), "ECS のデプロイを Blue/Green に切り替えました。ロールバックが一瞬で済むのは安心感が違います。", 48, 6),
            new(now.AddDays(-1).AddHours(-3), ".NET MAUI のアプリの起動時間を測りました。アイコンの準備をやめただけで 1 秒近く縮みました。", 112, 14),
            new(now.AddDays(-3), "週末は CDK で個人のインフラを書き直し。Terraform との違いも近いうちにまとめます。", 75, 9)
        ];
        LikeList =
        [
            new("avatar_person02.jpg", "日本酒飲郎", now.AddHours(-5), "Lambda の SnapStart が .NET でも使えるようになっていて驚き。コールドスタートがかなり速い。"),
            new("avatar_person01.jpg", "M･I･O", now.AddDays(-1).AddHours(-1), "CollectionView のヘッダーを固定する小技を共有します。"),
            new("avatar_person03.jpg", "悪いスライム", now.AddDays(-2), "DynamoDB の単一テーブル設計、やっと腑に落ちました。")
        ];

        FollowCommand = MakeDelegateCommand(() => IsFollowed = !IsFollowed);
        LikeCommand = MakeDelegateCommand(() => IsLiked = !IsLiked);
        StarCommand = MakeDelegateCommand(() => IsStarred = !IsStarred);
        OpenPhotoCommand = MakeDelegateCommand<UIProfilePhoto>(OpenPhoto);
        ClosePhotoCommand = MakeDelegateCommand(() => IsPhotoViewerVisible = false);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync()
    {
        if (IsPhotoViewerVisible)
        {
            IsPhotoViewerVisible = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void OpenPhoto(UIProfilePhoto photo)
    {
        for (var i = 0; i < Photos.Count; i++)
        {
            if (ReferenceEquals(Photos[i], photo))
            {
                PhotoPosition = i;
                break;
            }
        }

        IsPhotoViewerVisible = true;
    }
}
