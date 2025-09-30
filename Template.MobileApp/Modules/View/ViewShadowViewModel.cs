namespace Template.MobileApp.Modules.View;

public sealed partial class ViewShadowViewModel : AppViewModelBase
{
    public ObservableCollection<ColorItem> BorderColors { get; }

    public ObservableCollection<ColorItem> ShadowColors { get; }

    [ObservableProperty]
    public partial ColorItem BorderColor { get; set; }

    [ObservableProperty]
    public partial ColorItem ShadowColor { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(ShadowOffset)])]
    public partial double ShadowOffsetX { get; set; } = 4;

    [ObservableProperty(NotifyAlso = [nameof(ShadowOffset)])]
    public partial double ShadowOffsetY { get; set; } = 4;

    public Point ShadowOffset => new(ShadowOffsetX, ShadowOffsetY);

    [ObservableProperty]
    public partial double ShadowRadius { get; set; } = 10;

    [ObservableProperty]
    public partial double ShadowOpacity { get; set; } = 0.5;

    public ViewShadowViewModel(ResourceDictionary resources)
    {
        BorderColors = new(resources.EnumValues<Color>().Where(x => x.Key.EndsWith("Default", StringComparison.Ordinal)).Select(x => new ColorItem(x.Key, x.Value)));
        ShadowColors = new(resources.EnumValues<Color>().Where(x => x.Key.StartsWith("Gray", StringComparison.Ordinal)).Select(x => new ColorItem(x.Key, x.Value)));

        BorderColor = BorderColors[0];
        ShadowColor = ShadowColors[0];
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
