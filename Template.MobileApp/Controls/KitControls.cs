namespace Template.MobileApp.Controls;

using System.Collections.Specialized;

using SkiaSharp.Views.Maui;

using Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Sleep
//--------------------------------------------------------------------------------
public sealed class KitSleepBand : SKCanvasView
{
    private const string FontFamily = "sans-serif";

    private const float LabelWidth = 36f;

    private const float AxisHeight = 20f;

    private const float LabelBaselineRate = 0.35f;

    private const float LaneFillMargin = 0.18f;

    private const float FillCornerRadius = 2f;

    private const float HourLabelMargin = 16f;

    private const float HourTickLength = 3f;

    private const float HourLabelBottom = 4f;

    private static readonly string[] LaneNames = ["覚醒", "レム", "浅い", "深い"];

    private static readonly string[] HourTexts = [.. Enumerable.Range(0, 24).Select(static x => $"{x}時")];

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private FontResolver? fonts;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SpansProperty = BindableProperty.Create(
        nameof(Spans),
        typeof(IReadOnlyList<KitSleepSpan>),
        typeof(KitSleepBand),
        propertyChanged: OnSpansChanged);

    public static readonly BindableProperty AwakeColorProperty = BindableProperty.Create(
        nameof(AwakeColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.Orange,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty RemColorProperty = BindableProperty.Create(
        nameof(RemColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.MediumPurple,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty LightColorProperty = BindableProperty.Create(
        nameof(LightColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.LightSkyBlue,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty DeepColorProperty = BindableProperty.Create(
        nameof(DeepColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.DarkSlateBlue,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(KitSleepBand),
        11d,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.Gray,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty GridColorProperty = BindableProperty.Create(
        nameof(GridColor),
        typeof(Color),
        typeof(KitSleepBand),
        Colors.LightGray,
        propertyChanged: OnVisualChanged);

    public IReadOnlyList<KitSleepSpan>? Spans
    {
        get => (IReadOnlyList<KitSleepSpan>?)GetValue(SpansProperty);
        set => SetValue(SpansProperty, value);
    }

    public Color AwakeColor
    {
        get => (Color)GetValue(AwakeColorProperty);
        set => SetValue(AwakeColorProperty, value);
    }

    public Color RemColor
    {
        get => (Color)GetValue(RemColorProperty);
        set => SetValue(RemColorProperty, value);
    }

    public Color LightColor
    {
        get => (Color)GetValue(LightColorProperty);
        set => SetValue(LightColorProperty, value);
    }

    public Color DeepColor
    {
        get => (Color)GetValue(DeepColorProperty);
        set => SetValue(DeepColorProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color GridColor
    {
        get => (Color)GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object? oldValue, object? newValue) =>
        ((KitSleepBand)bindable).InvalidateSurface();

    private static void OnSpansChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var band = (KitSleepBand)bindable;
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= band.OnSpansCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += band.OnSpansCollectionChanged;
        }

        band.InvalidateSurface();
    }

    private void OnSpansCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateSurface();

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
        {
            fonts?.Dispose();
            fonts = null;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var spans = Spans;
        if ((spans is null) || (spans.Count == 0) || (Width <= 0))
        {
            return;
        }

        var density = (float)(e.Info.Width / Width);
        var left = LabelWidth * density;
        var bottom = e.Info.Height - (AxisHeight * density);
        var laneHeight = bottom / LaneNames.Length;
        var start = spans[0].Start;
        var end = spans[^1].Start + spans[^1].Length;
        var total = (float)(end - start).TotalMinutes;
        var width = e.Info.Width - left;

        float ToX(DateTime time) => left + ((float)(time - start).TotalMinutes / total * width);

        fonts ??= new FontResolver(FontFamily);
        var size = (float)FontSize * density;
        using var text = new SKPaint();
        text.IsAntialias = true;
        text.Color = TextColor.ToSKColor();
        using var grid = new SKPaint();
        grid.Color = GridColor.ToSKColor();
        grid.StrokeWidth = density;

        for (var lane = 0; lane < LaneNames.Length; lane++)
        {
            var center = (lane + 0.5f) * laneHeight;
            canvas.DrawLine(left, center, e.Info.Width, center, grid);
            canvas.DrawText(fonts.CreateText(LaneNames[lane], size, false), 0, center + (size * LabelBaselineRate), SKTextAlign.Left, text);
        }

        using var fill = new SKPaint();
        fill.IsAntialias = true;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < spans.Count; i++)
        {
            var span = spans[i];
            var lane = (int)span.Stage;
            var x0 = ToX(span.Start);
            var x1 = ToX(span.Start + span.Length);
            fill.Color = ColorOf(span.Stage).ToSKColor();
            canvas.DrawRoundRect(
                new SKRect(x0, (lane + LaneFillMargin) * laneHeight, Math.Max(x0 + density, x1), (lane + 1 - LaneFillMargin) * laneHeight),
                FillCornerRadius * density,
                FillCornerRadius * density,
                fill);
        }

        var hour = start.Date.AddHours(start.Hour + 1);
        while (hour < end)
        {
            var x = ToX(hour);
            if ((x - left > HourLabelMargin * density) && (e.Info.Width - x > HourLabelMargin * density))
            {
                canvas.DrawLine(x, bottom, x, bottom + (HourTickLength * density), grid);
                canvas.DrawText(fonts.CreateText(HourTexts[hour.Hour], size, false), x, e.Info.Height - (HourLabelBottom * density), SKTextAlign.Center, text);
            }
            hour = hour.AddHours(1);
        }
    }

    private Color ColorOf(KitSleepStage stage) => stage switch
    {
        KitSleepStage.Awake => AwakeColor,
        KitSleepStage.Rem => RemColor,
        KitSleepStage.Light => LightColor,
        _ => DeepColor
    };
}
