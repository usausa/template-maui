namespace Template.MobileApp.Modules.UI;

public enum MailPage
{
    Mail,
    Schedule,
    Application
}

public sealed partial class UIMailViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    [ObservableProperty]
    public partial MailPage Selected { get; set; } = MailPage.Mail;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<MailMessage> Messages { get; } = [];

    public IObserveCommand SelectCommand { get; }

    public ICommand ArchiveCommand { get; }

    public ICommand DeleteCommand { get; }

    public UIMailViewModel(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;

        SelectCommand = MakeDelegateCommand<MailPage>(x => Selected = x);
        ArchiveCommand = MakeDelegateCommand<MailMessage>(x => Messages.Remove(x));
        DeleteCommand = MakeDelegateCommand<MailMessage>(x => Messages.Remove(x));
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        await Navigator.PostActionAsync(LoadMessagesAsync);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // ReSharper disable StringLiteralTypo
    private async Task LoadMessagesAsync()
    {
        IsLoading = true;

        // Simulate delay
        await Task.Delay(500);

        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now,
            Image = await LoadImage("mofusand.jpg"),
            From = "山奥通信",
            Title = "タイトルだよもんタイトルだよもんタイトルだよもんタイトルだよもんタイトルだよもん",
            Body = "こんにちは。\nうさうさです、どうぞよろしくお願いしますだよもん。\n文章はまだ続きます。",
            IsUnread = true
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddHours(-3).AddMinutes(-12),
            Image = await LoadImage("genbaneko.png"),
            From = "現場猫bot",
            Title = "作業前安全確認",
            Body = "今日も一日ゼロ災ヨシ！\nあああああああああああ!!!!!!!!!!",
            IsUnread = true
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddHours(-6).AddMinutes(-15),
            Image = await LoadImage("mofusand.jpg"),
            From = "山奥通信",
            Title = "タイトルだよもんタイトルだよもんタイトルだよもんタイトルだよもんタイトルだよもん",
            Body = "こんにちは。\n私はうさうさです。\nどうぞよろしくお願いしますだよもん。\n文章はまだ続きます。"
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddDays(-1),
            Image = await LoadImage("usausa.png"),
            From = "うさうさ・メープル・フレンチトースト",
            Title = "Re: Re: おはようございます！",
            Body = "こんにちは！\n先日はありがとうございました"
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddDays(-2),
            Image = await LoadImage("genbaneko.png"),
            From = "現場猫bot",
            Title = "作業前安全確認",
            Body = "今日も一日ゼロ災ヨシ！\nあああああああああああ!!!!!!!!!!"
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddDays(-5),
            Image = await LoadImage("genbaneko.png"),
            From = "現場猫bot",
            Title = "作業前安全確認",
            Body = "今日も一日ゼロ災ヨシ！\nあああああああああああ!!!!!!!!!!"
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddDays(-10),
            Image = await LoadImage("genbaneko.png"),
            From = "現場猫bot",
            Title = "作業前安全確認",
            Body = "今日も一日ゼロ災ヨシ！\nあああああああああああ!!!!!!!!!!"
        });
        Messages.Add(new MailMessage
        {
            DateTime = DateTime.Now.AddDays(-20),
            Image = await LoadImage("genbaneko.png"),
            From = "現場猫bot",
            Title = "作業前安全確認",
            Body = "今日も一日ゼロ災ヨシ！\nあああああああああああ!!!!!!!!!!"
        });

        IsLoading = false;

        async ValueTask<SKBitmapImageSource> LoadImage(string fileName)
        {
            await using var stream = await fileSystem.OpenAppPackageFileAsync(Path.Combine("Avatar", fileName));
            var bitmap = SKBitmap.Decode(stream);
            // SKBitmapはアンマネージドメモリを持つため画面破棄時に解放する
            Disposables.Add(bitmap);
            return new SKBitmapImageSource { Bitmap = bitmap };
        }
    }
    // ReSharper restore StringLiteralTypo
}
