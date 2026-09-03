namespace Template.MobileApp.Modules.UI;

public sealed class UIPetTag
{
    public string Label { get; init; } = string.Empty;
    public Color BackgroundColor { get; init; } = Colors.LightGray;
    public Color TextColor { get; init; } = Colors.DarkGray;
}

public sealed partial class UIPetViewModel : AppViewModelBase
{
    public string PetName { get; } = "Milo";

    public string Breed { get; } = "Shiba Inu";

    public string Level { get; } = "Lv.12";

    public string FlavorText { get; } =
        "A spirited companion from the eastern hills. Quick on its feet, loyal to a fault — never misses a squirrel, never forgets a friend.";

    public IReadOnlyList<UIPetTag> Tags { get; } =
    [
        new() { Label = "Playful", BackgroundColor = Color.FromArgb("#FCE4EC"), TextColor = Color.FromArgb("#C2185B") },
        new() { Label = "Friendly", BackgroundColor = Color.FromArgb("#E8F5E9"), TextColor = Color.FromArgb("#2E7D32") },
        new() { Label = "Curious", BackgroundColor = Color.FromArgb("#E3F2FD"), TextColor = Color.FromArgb("#1565C0") }
    ];

    // Heart で HP が増えるデモ(バーは HP/400 の比率で伸長)
    [ObservableProperty(NotifyAlso = [nameof(HpProgress)])]
    public partial int Hp { get; set; } = 320;

    public double HpProgress => Hp / 400d;

    [ObservableProperty]
    public partial bool InParty { get; set; }

    public IObserveCommand HeartCommand { get; }

    public IObserveCommand PartyCommand { get; }

    public UIPetViewModel()
    {
        HeartCommand = MakeDelegateCommand(() => Hp = Math.Min(400, Hp + 5));
        PartyCommand = MakeDelegateCommand(() => InParty = !InParty);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
