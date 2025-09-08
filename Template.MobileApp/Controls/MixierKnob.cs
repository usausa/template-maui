namespace Template.MobileApp.Controls;

public sealed class MixierKnob : GraphicsView, IDrawable
{
    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    // Value

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(MixierKnob),
        0.0,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((MixierKnob)b).Invalidate());

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(MixierKnob),
        0.0);

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(MixierKnob),
        100.0);

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // Color

    public static readonly BindableProperty KnobBackgroundProperty = BindableProperty.Create(
        nameof(KnobBackground),
        typeof(Brush),
        typeof(MixierKnob),
        Brush.LightGray,
        propertyChanged: OnPropertyChanged);

    public Brush KnobBackground
    {
        get => (Brush)GetValue(KnobBackgroundProperty);
        set => SetValue(KnobBackgroundProperty, value);
    }

    public static readonly BindableProperty PointerBackgroundProperty = BindableProperty.Create(
        nameof(PointerBackground),
        typeof(Brush),
        typeof(MixierKnob),
        Brush.Black,
        propertyChanged: OnPropertyChanged);

    public Brush PointerBackground
    {
        get => (Brush)GetValue(PointerBackgroundProperty);
        set => SetValue(PointerBackgroundProperty, value);
    }

    public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(
        nameof(IndicatorColor),
        typeof(Color),
        typeof(MixierKnob),
        Colors.Orange,
        propertyChanged: OnPropertyChanged);

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public static readonly BindableProperty IndicatorBackgroundColorProperty = BindableProperty.Create(
        nameof(IndicatorBackgroundColor),
        typeof(Color),
        typeof(MixierKnob),
        Colors.Snow,
        propertyChanged: OnPropertyChanged);

    public Color IndicatorBackgroundColor
    {
        get => (Color)GetValue(IndicatorBackgroundColorProperty);
        set => SetValue(IndicatorBackgroundColorProperty, value);
    }

    // Size

    public static readonly BindableProperty IndicatorWidthProperty = BindableProperty.Create(
        nameof(IndicatorWidth),
        typeof(double),
        typeof(MixierKnob),
        8.0,
        propertyChanged: OnPropertyChanged);

    public double IndicatorWidth
    {
        get => (double)GetValue(IndicatorWidthProperty);
        set => SetValue(IndicatorWidthProperty, value);
    }

    public static readonly BindableProperty IndicatorMarginProperty = BindableProperty.Create(
        nameof(IndicatorMargin),
        typeof(double),
        typeof(MixierKnob),
        2.0,
        propertyChanged: OnPropertyChanged);

    public double IndicatorMargin
    {
        get => (double)GetValue(IndicatorMarginProperty);
        set => SetValue(IndicatorMarginProperty, value);
    }

    public static readonly BindableProperty PointerWidthProperty = BindableProperty.Create(
        nameof(PointerWidth),
        typeof(double),
        typeof(MixierKnob),
        12.0,
        propertyChanged: OnPropertyChanged);

    public double PointerWidth
    {
        get => (double)GetValue(PointerWidthProperty);
        set => SetValue(PointerWidthProperty, value);
    }

    public static readonly BindableProperty PointerMarginProperty = BindableProperty.Create(
        nameof(PointerMargin),
        typeof(double),
        typeof(MixierKnob),
        4.0,
        propertyChanged: OnPropertyChanged);

    public double PointerMargin
    {
        get => (double)GetValue(PointerMarginProperty);
        set => SetValue(PointerMarginProperty, value);
    }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public MixierKnob()
    {
        Drawable = this;

        StartInteraction += (_, e) => UpdateValue(e.Touches[0]);
        DragInteraction += (_, e) => UpdateValue(e.Touches[0]);
    }

    // ------------------------------------------------------------
    // Handler
    // ------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixierKnob)bindable).Invalidate();
    }

    // ------------------------------------------------------------
    // Update
    // ------------------------------------------------------------

    private void UpdateValue(PointF point)
    {
        var center = new Point(Width / 2, Height / 2);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;

        var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        angle = Math.Clamp(angle > 90 ? angle - 90 : angle + 270, 45, 315);
        angle -= 45;

        var percent = angle / 270.0;

        Value = Minimum + (percent * (Maximum - Minimum));
    }

    // ------------------------------------------------------------
    // Draw
    // ------------------------------------------------------------

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);

        var valuePercent = (Value - Minimum) / (Maximum - Minimum);
        var valueSweepAngle = (float)(270 * valuePercent);

        // Indicator
        var indicatorWidth = (float)IndicatorWidth;
        var indicatorSize = size - indicatorWidth;
        var indicatorRadius = indicatorSize / 2;
        var indicatorRect = new RectF(cx - indicatorRadius, cy - indicatorRadius, indicatorSize, indicatorSize);

        canvas.StrokeLineCap = LineCap.Butt;
        canvas.StrokeSize = indicatorWidth;
        canvas.StrokeColor = IndicatorBackgroundColor;
        canvas.DrawArc(indicatorRect, 225, -45, true, false);

        canvas.StrokeColor = IndicatorColor;
        canvas.DrawArc(indicatorRect, 225, 225 - valueSweepAngle, true, false);

        // Knob
        var knobSize = size - (indicatorWidth * 2) - ((float)IndicatorMargin * 2);
        var knobRadius = knobSize / 2;

        canvas.SetFillPaint(KnobBackground, new RectF(cx - knobRadius, cy - knobRadius, knobSize, knobSize));
        canvas.FillCircle(cx, cy, knobRadius);

        // Knob pointer
        var pointerSize = (float)PointerWidth;
        var pointerRadius = pointerSize / 2;

        var pointerDistance = indicatorRadius - pointerSize - PointerMargin;
        var pointerRadians = Math.PI * (valueSweepAngle - 225) / 180.0;
        var pointerX = (float)(cx + (Math.Cos(pointerRadians) * pointerDistance));
        var pointerY = (float)(cy + (Math.Sin(pointerRadians) * pointerDistance));

        canvas.SetFillPaint(PointerBackground, new RectF(pointerX - pointerRadius, pointerY - pointerRadius, pointerSize, pointerSize));
        canvas.FillCircle(pointerX, pointerY, pointerRadius);
    }
}
