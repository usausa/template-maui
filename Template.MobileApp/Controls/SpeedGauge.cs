namespace Template.MobileApp.Controls;

public sealed class SpeedGauge : GraphicsView, IDrawable
{
    private const float StartAngle = 210f;

    private const float GaugeAngle = 240f;

    private const float MidRatio = 0.5f;

    private const float HighRatio = 0.75f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty GaugeColorProperty = BindableProperty.Create(
        nameof(GaugeColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.LightGray,
        propertyChanged: Invalidate);

    public static readonly BindableProperty ValueColorProperty = BindableProperty.Create(
        nameof(ValueColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Turquoise,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MidColorProperty = BindableProperty.Create(
        nameof(MidColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Yellow,
        propertyChanged: Invalidate);

    public static readonly BindableProperty HighColorProperty = BindableProperty.Create(
        nameof(HighColor),
        typeof(Color),
        typeof(SpeedGauge),
        Colors.Red,
        propertyChanged: Invalidate);

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(SpeedGauge),
        10f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty GaugeWidthProperty = BindableProperty.Create(
        nameof(GaugeWidth),
        typeof(float),
        typeof(SpeedGauge),
        32f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MaxSpeedProperty = BindableProperty.Create(
        nameof(MaxSpeed),
        typeof(int),
        typeof(SpeedGauge),
        propertyChanged: Invalidate);

    public static readonly BindableProperty SpeedProperty = BindableProperty.Create(
        nameof(Speed),
        typeof(int),
        typeof(SpeedGauge),
        propertyChanged: Invalidate);

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public Color MidColor
    {
        get => (Color)GetValue(MidColorProperty);
        set => SetValue(MidColorProperty, value);
    }

    public Color HighColor
    {
        get => (Color)GetValue(HighColorProperty);
        set => SetValue(HighColorProperty, value);
    }

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public float GaugeWidth
    {
        get => (float)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    public int MaxSpeed
    {
        get => (int)GetValue(MaxSpeedProperty);
        set => SetValue(MaxSpeedProperty, value);
    }

    public int Speed
    {
        get => (int)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SpeedGauge()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

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

        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(0, 0, width, height);

        var valueAngle = StartAngle - (GaugeAngle * Speed / MaxSpeed);

        canvas.StrokeColor = GaugeColor;
        canvas.StrokeSize = gaugeWidth;

        canvas.DrawArc(gaugeRect, StartAngle, StartAngle - GaugeAngle, true, false);

        canvas.StrokeColor = CalcValueColor();
        canvas.StrokeSize = gaugeWidth;

        canvas.DrawArc(gaugeRect, StartAngle, valueAngle, true, false);
    }

    private Color CalcValueColor()
    {
        var ratio = MaxSpeed > 0 ? (float)Speed / MaxSpeed : 0f;
        if (ratio < MidRatio)
        {
            return LerpColor(ValueColor, MidColor, ratio / MidRatio);
        }
        if (ratio < HighRatio)
        {
            return LerpColor(MidColor, HighColor, (ratio - MidRatio) / (HighRatio - MidRatio));
        }
        return HighColor;
    }

    private static Color LerpColor(Color color, Color to, float amount)
    {
        var t = Math.Clamp(amount, 0f, 1f);
        return new Color(
            Single.Lerp(color.Red, to.Red, t),
            Single.Lerp(color.Green, to.Green, t),
            Single.Lerp(color.Blue, to.Blue, t),
            Single.Lerp(color.Alpha, to.Alpha, t));
    }
}
