namespace Template.MobileApp.Modules.UI;

public sealed class UIMonsterTag
{
    public string Label { get; init; } = string.Empty;
    public Color BackgroundColor { get; init; } = Colors.LightGray;
    public Color TextColor { get; init; } = Colors.DarkGray;
}

public sealed partial class UIMonsterViewModel : AppViewModelBase
{
    public string MonsterName { get; } = "ぷるにゃ";

    public string Breed { get; } = "ゼリーキャット";

    public string Level { get; } = "Lv.12";

    public string FlavorText { get; } =
        "体のほとんどがひんやりしたゼリーでできた猫のモンスター。お腹の光る核をなでると喜び、機嫌がいいと全身がぷるぷる揺れる。";

    public IReadOnlyList<UIMonsterTag> Tags { get; } =
    [
        new() { Label = "水タイプ", BackgroundColor = Color.FromArgb("#E3F2FD"), TextColor = Color.FromArgb("#1565C0") },
        new() { Label = "のんびり", BackgroundColor = Color.FromArgb("#E8F5E9"), TextColor = Color.FromArgb("#2E7D32") },
        new() { Label = "甘えん坊", BackgroundColor = Color.FromArgb("#FCE4EC"), TextColor = Color.FromArgb("#C2185B") }
    ];

    // Heart で HP が増えるデモ(バーは HP/400 の比率で伸長)
    [ObservableProperty(NotifyAlso = [nameof(HpProgress)])]
    public partial int Hp { get; set; } = 320;

    public double HpProgress => Hp / 400d;

    [ObservableProperty]
    public partial bool InParty { get; set; }

    public IObserveCommand HeartCommand { get; }

    public IObserveCommand PartyCommand { get; }

    public UIMonsterViewModel()
    {
        HeartCommand = MakeDelegateCommand(() => Hp = Math.Min(400, Hp + 5));
        PartyCommand = MakeDelegateCommand(() => InParty = !InParty);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
