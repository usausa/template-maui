namespace Template.MobileApp.Modules.View;

public enum BorderShape
{
    RoundRectangle,
    Ellipse,
    Rectangle,
    Polygon
}

public sealed partial class ViewBorderViewModel : AppViewModelBase
{
    public ObservableCollection<BorderShape> Shapes { get; } = [BorderShape.RoundRectangle, BorderShape.Ellipse, BorderShape.Rectangle, BorderShape.Polygon];

    public ObservableCollection<ColorItem> BorderColors { get; }

    public ObservableCollection<ColorItem> StrokeColors { get; }

    public ObservableCollection<LineJoin> LineJoins { get; } = [LineJoin.Miter, LineJoin.Bevel, LineJoin.Round];

    public ObservableCollection<LineCap> LineCaps { get; } = [LineCap.Butt, LineCap.Round, LineCap.Square];

    [ObservableProperty]
    public partial BorderShape BorderShape { get; set; } = BorderShape.RoundRectangle;

    [ObservableProperty]
    public partial ColorItem BorderColor { get; set; }

    [ObservableProperty]
    public partial ColorItem StrokeColor { get; set; }

    [ObservableProperty]
    public partial double StrokeThickness { get; set; } = 1;

    [ObservableProperty]
    public partial double StrokeDashLength1 { get; set; } = 0;

    [ObservableProperty]
    public partial double StrokeDashLength2 { get; set; } = 0;

    public DoubleCollection StrokeDashArray { get; } = new();

    [ObservableProperty]
    public partial double StrokeDashOffset { get; set; } = 0;

    [ObservableProperty]
    public partial LineJoin StrokeLineJoin { get; set; } = LineJoin.Miter;

    [ObservableProperty]
    public partial LineCap StrokeLineCap { get; set; } = LineCap.Butt;

    [ObservableProperty(NotifyAlso = [nameof(CornerRadius)])]
    public partial double CornerRadiusTopLeft { get; set; } = 20;

    [ObservableProperty(NotifyAlso = [nameof(CornerRadius)])]
    public partial double CornerRadiusTopRight { get; set; } = 20;

    [ObservableProperty(NotifyAlso = [nameof(CornerRadius)])]
    public partial double CornerRadiusBottomLeft { get; set; } = 20;

    [ObservableProperty(NotifyAlso = [nameof(CornerRadius)])]
    public partial double CornerRadiusBottomRight { get; set; } = 20;

    public CornerRadius CornerRadius => new(CornerRadiusTopLeft, CornerRadiusTopRight, CornerRadiusBottomLeft, CornerRadiusBottomRight);

    public ViewBorderViewModel(ResourceDictionary resources)
    {
        BorderColors = new(resources.EnumValues<Color>().Where(x => x.Key.EndsWith("Default", StringComparison.Ordinal)).Select(x => new ColorItem(x.Key, x.Value)));
        StrokeColors = new(resources.EnumValues<Color>().Where(x => x.Key.EndsWith("Accent1", StringComparison.Ordinal) || x.Key.EndsWith("Accent4", StringComparison.Ordinal)).Select(x => new ColorItem(x.Key, x.Value)));

        BorderColor = BorderColors[0];
        StrokeColor = StrokeColors[0];

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(StrokeDashLength1) or nameof(StrokeDashLength2))
            {
                StrokeDashArray.Clear();
                if (StrokeDashLength1 > 0)
                {
                    StrokeDashArray.Add(StrokeDashLength1);
                    if (StrokeDashLength2 > 0)
                    {
                        StrokeDashArray.Add(StrokeDashLength2);
                    }
                }
            }
        };
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
