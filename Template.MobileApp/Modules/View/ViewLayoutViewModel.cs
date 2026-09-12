namespace Template.MobileApp.Modules.View;

public sealed partial class ViewLayoutHex : ObservableObject
{
    public string Label { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public Color Fill { get; init; } = Colors.White;
    public Color Accent { get; init; } = Colors.Gray;
    public int Delay { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public sealed class ViewLayoutViewModel : AppViewModelBase
{
    // HoneycombLayout のセル (3 列 7 個で中央 1 + 周囲 6 の花形。Delay は中央→外周の順に入場させる)
    public IReadOnlyList<ViewLayoutHex> Hexes { get; } =
    [
        new() { Label = "Calm", Icon = Fonts.MaterialIcons.Self_improvement, Fill = Color.FromArgb("#80CBC4"), Accent = Color.FromArgb("#00695C"), Delay = 80 },
        new() { Label = "Tired", Icon = Fonts.MaterialIcons.Sentiment_neutral, Fill = Color.FromArgb("#90CAF9"), Accent = Color.FromArgb("#1565C0"), Delay = 140 },
        new() { Label = "Excited", Icon = Fonts.MaterialIcons.Bolt, Fill = Color.FromArgb("#FFE0B2"), Accent = Color.FromArgb("#F57C00"), Delay = 140 },
        new() { Label = "Happy", Icon = Fonts.MaterialIcons.Sentiment_very_satisfied, Fill = Color.FromArgb("#F8BBD0"), Accent = Color.FromArgb("#C2185B"), IsSelected = true },
        new() { Label = "Focus", Icon = Fonts.MaterialIcons.Center_focus_strong, Fill = Color.FromArgb("#FFF59D"), Accent = Color.FromArgb("#F57F17"), Delay = 200 },
        new() { Label = "Angry", Icon = Fonts.MaterialIcons.Sentiment_very_dissatisfied, Fill = Color.FromArgb("#FFCDD2"), Accent = Color.FromArgb("#D32F2F"), Delay = 200 },
        new() { Label = "Sad", Icon = Fonts.MaterialIcons.Sentiment_dissatisfied, Fill = Color.FromArgb("#EEEEEE"), Accent = Color.FromArgb("#616161"), Delay = 260 }
    ];

    public IObserveCommand SelectHexCommand { get; }

    public ViewLayoutViewModel()
    {
        SelectHexCommand = MakeDelegateCommand<ViewLayoutHex>(x =>
        {
            foreach (var hex in Hexes)
            {
                hex.IsSelected = hex == x;
            }
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
