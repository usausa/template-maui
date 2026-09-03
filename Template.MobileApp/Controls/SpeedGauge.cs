namespace Template.MobileApp.Controls;

public sealed class SpeedGauge : GraphicsView, IDrawable
{
    public static readonly BindableProperty GaugeColorProperty = BindableProperty.Create(
        nameof(GaugeColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.LightGray,
        propertyChanged: Invalidate);

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    public static readonly BindableProperty ValueColorProperty = BindableProperty.Create(
        nameof(ValueColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Turquoise,
        propertyChanged: Invalidate);

    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public static readonly BindableProperty MidColorProperty = BindableProperty.Create(
        nameof(MidColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Yellow,
        propertyChanged: Invalidate);

    public Color MidColor
    {
        get => (Color)GetValue(MidColorProperty);
        set => SetValue(MidColorProperty, value);
    }

    public static readonly BindableProperty HighColorProperty = BindableProperty.Create(
        nameof(HighColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Red,
        propertyChanged: Invalidate);

    public Color HighColor
    {
        get => (Color)GetValue(HighColorProperty);
        set => SetValue(HighColorProperty, value);
    }

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(SpeedGauge),
        10f,
        propertyChanged: Invalidate);

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public static readonly BindableProperty GaugeWidthProperty = BindableProperty.Create(
        nameof(GaugeWidth),
        typeof(float),
        typeof(SpeedGauge),
        32f,
        propertyChanged: Invalidate);

    public float GaugeWidth
    {
        get => (float)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    public static readonly BindableProperty MaxSpeedProperty = BindableProperty.Create(
        nameof(MaxSpeed),
        typeof(int),
        typeof(SpeedGauge),
        propertyChanged: Invalidate);

    public int MaxSpeed
    {
        get => (int)GetValue(MaxSpeedProperty);
        set => SetValue(MaxSpeedProperty, value);
    }

    public static readonly BindableProperty SpeedProperty = BindableProperty.Create(
        nameof(Speed),
        typeof(int),
        typeof(SpeedGauge),
        propertyChanged: Invalidate);

    public int Speed
    {
        get => (int)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public SpeedGauge()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
    }

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((SpeedGauge)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2) - BorderMargin;

        var gaugeWidth = GaugeWidth;
        var strokeMargin = gaugeWidth / 2;
        var gaugeSize = (radius * 2) - gaugeWidth;
        var gaugeRect = new RectF(cx - radius + strokeMargin, cy - radius + strokeMargin, gaugeSize, gaugeSize);

        canvas.Antialias = true;

        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(0, 0, width, height);

        // Arc
        const float startAngle = 210f;
        const float gaugeAngle = 240f;
        var valueAngle = startAngle - (gaugeAngle * Speed / MaxSpeed);

        // Gauge
        canvas.StrokeColor = GaugeColor;
        canvas.StrokeSize = gaugeWidth;

        canvas.DrawArc(gaugeRect, startAngle, startAngle - gaugeAngle, true, false);

        // Value
        canvas.StrokeColor = CalcValueColor();
        canvas.StrokeSize = gaugeWidth;

        canvas.DrawArc(gaugeRect, startAngle, valueAngle, true, false);
    }

    private Color CalcValueColor()
    {
        var ratio = MaxSpeed > 0 ? (float)Speed / MaxSpeed : 0f;
        if (ratio < 0.5f)
        {
            return LerpColor(ValueColor, MidColor, ratio / 0.5f);
        }
        if (ratio < 0.75f)
        {
            return LerpColor(MidColor, HighColor, (ratio - 0.5f) / 0.25f);
        }
        return HighColor;
    }

    private static Color LerpColor(Color from, Color to, float t) =>
        new(from.Red + ((to.Red - from.Red) * t), from.Green + ((to.Green - from.Green) * t), from.Blue + ((to.Blue - from.Blue) * t), from.Alpha + ((to.Alpha - from.Alpha) * t));
}
