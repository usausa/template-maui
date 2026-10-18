namespace Template.MobileApp.Controls;

//--------------------------------------------------------------------------------
// Knob
//--------------------------------------------------------------------------------
public sealed class MixerKnob : GraphicsView, IDrawable
{
    private const float StartAngle = 225f;

    private const float SweepAngle = 270f;

    private const float GapAngle = (360f - SweepAngle) / 2;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(MixerKnob),
        0.0,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((MixerKnob)b).Invalidate());

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(MixerKnob),
        0.0);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(MixerKnob),
        100.0);

    public static readonly BindableProperty KnobBackgroundProperty = BindableProperty.Create(
        nameof(KnobBackground),
        typeof(Brush),
        typeof(MixerKnob),
        Brush.LightGray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty PointerBackgroundProperty = BindableProperty.Create(
        nameof(PointerBackground),
        typeof(Brush),
        typeof(MixerKnob),
        Brush.Black,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(
        nameof(IndicatorColor),
        typeof(Color),
        typeof(MixerKnob),
        Colors.Orange,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty IndicatorBackgroundColorProperty = BindableProperty.Create(
        nameof(IndicatorBackgroundColor),
        typeof(Color),
        typeof(MixerKnob),
        Colors.Snow,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty IndicatorWidthProperty = BindableProperty.Create(
        nameof(IndicatorWidth),
        typeof(double),
        typeof(MixerKnob),
        8.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty IndicatorMarginProperty = BindableProperty.Create(
        nameof(IndicatorMargin),
        typeof(double),
        typeof(MixerKnob),
        2.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty PointerWidthProperty = BindableProperty.Create(
        nameof(PointerWidth),
        typeof(double),
        typeof(MixerKnob),
        12.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty PointerMarginProperty = BindableProperty.Create(
        nameof(PointerMargin),
        typeof(double),
        typeof(MixerKnob),
        4.0,
        propertyChanged: OnPropertyChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public Brush KnobBackground
    {
        get => (Brush)GetValue(KnobBackgroundProperty);
        set => SetValue(KnobBackgroundProperty, value);
    }

    public Brush PointerBackground
    {
        get => (Brush)GetValue(PointerBackgroundProperty);
        set => SetValue(PointerBackgroundProperty, value);
    }

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public Color IndicatorBackgroundColor
    {
        get => (Color)GetValue(IndicatorBackgroundColorProperty);
        set => SetValue(IndicatorBackgroundColorProperty, value);
    }

    public double IndicatorWidth
    {
        get => (double)GetValue(IndicatorWidthProperty);
        set => SetValue(IndicatorWidthProperty, value);
    }

    public double IndicatorMargin
    {
        get => (double)GetValue(IndicatorMarginProperty);
        set => SetValue(IndicatorMarginProperty, value);
    }

    public double PointerWidth
    {
        get => (double)GetValue(PointerWidthProperty);
        set => SetValue(PointerWidthProperty, value);
    }

    public double PointerMargin
    {
        get => (double)GetValue(PointerMarginProperty);
        set => SetValue(PointerMarginProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MixerKnob()
    {
        Drawable = this;

        StartInteraction += (_, e) => UpdateValue(e.Touches[0]);
        DragInteraction += (_, e) => UpdateValue(e.Touches[0]);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixerKnob)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);

        var valuePercent = (Value - Minimum) / (Maximum - Minimum);
        var valueSweepAngle = (float)(SweepAngle * valuePercent);

        var indicatorWidth = (float)IndicatorWidth;
        var indicatorSize = size - indicatorWidth;
        var indicatorRadius = indicatorSize / 2;
        var indicatorRect = new RectF(cx - indicatorRadius, cy - indicatorRadius, indicatorSize, indicatorSize);

        canvas.StrokeLineCap = LineCap.Butt;
        canvas.StrokeSize = indicatorWidth;
        canvas.StrokeColor = IndicatorBackgroundColor;
        canvas.DrawArc(indicatorRect, StartAngle, StartAngle - SweepAngle, true, false);

        canvas.StrokeColor = IndicatorColor;
        canvas.DrawArc(indicatorRect, StartAngle, StartAngle - valueSweepAngle, true, false);

        var knobSize = size - (indicatorWidth * 2) - ((float)IndicatorMargin * 2);
        var knobRadius = knobSize / 2;

        canvas.SetFillPaint(KnobBackground, new RectF(cx - knobRadius, cy - knobRadius, knobSize, knobSize));
        canvas.FillCircle(cx, cy, knobRadius);

        var pointerSize = (float)PointerWidth;
        var pointerRadius = pointerSize / 2;

        var pointerDistance = indicatorRadius - pointerSize - PointerMargin;
        var pointerRadians = Math.PI * (valueSweepAngle - StartAngle) / 180.0;
        var pointerX = (float)(cx + (Math.Cos(pointerRadians) * pointerDistance));
        var pointerY = (float)(cy + (Math.Sin(pointerRadians) * pointerDistance));

        canvas.SetFillPaint(PointerBackground, new RectF(pointerX - pointerRadius, pointerY - pointerRadius, pointerSize, pointerSize));
        canvas.FillCircle(pointerX, pointerY, pointerRadius);
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void UpdateValue(PointF point)
    {
        var center = new Point(Width / 2, Height / 2);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;

        var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        angle = Math.Clamp(angle > 90 ? angle - 90 : angle + 270, GapAngle, 360 - GapAngle);
        angle -= GapAngle;

        var percent = angle / SweepAngle;

        Value = Minimum + (percent * (Maximum - Minimum));
    }
}

//--------------------------------------------------------------------------------
// Slider
//--------------------------------------------------------------------------------
public sealed class MixerSlider : GraphicsView, IDrawable
{
    private const float TickWidth = 1f;

    private const double ThumbTouchMargin = 12;

    private const float ThumbBandRate = 0.25f;

    private static readonly Point ThumbGradientStartPoint = new(0, 0);

    private static readonly Point ThumbGradientEndPoint = new(0, 1);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private bool isDragging;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(MixerSlider),
        0.0,
        propertyChanged: OnPropertyChanged, defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum),
        typeof(double),
        typeof(MixerSlider),
        0.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum),
        typeof(double),
        typeof(MixerSlider),
        100.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor),
        typeof(Color),
        typeof(MixerSlider),
        Colors.DimGray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(MixerSlider),
        Colors.OrangeRed,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbColor1Property = BindableProperty.Create(
        nameof(ThumbColor1),
        typeof(Color),
        typeof(MixerSlider),
        Colors.LightGray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbColor2Property = BindableProperty.Create(
        nameof(ThumbColor2),
        typeof(Color),
        typeof(MixerSlider),
        Colors.DarkGray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TrackWidthProperty = BindableProperty.Create(
        nameof(TrackWidth),
        typeof(double),
        typeof(MixerSlider),
        16.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbWidthProperty = BindableProperty.Create(
        nameof(ThumbWidth),
        typeof(double),
        typeof(MixerSlider), 32.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbHeightProperty = BindableProperty.Create(
        nameof(ThumbHeight),
        typeof(double),
        typeof(MixerSlider),
        12.0,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty HasTickMarksProperty = BindableProperty.Create(
        nameof(HasTickMarks),
        typeof(bool),
        typeof(MixerSlider),
        true,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TickMarkColorProperty = BindableProperty.Create(
        nameof(TickMarkColor),
        typeof(Color),
        typeof(MixerSlider),
        Colors.Gray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TickMarkCountProperty = BindableProperty.Create(
        nameof(TickMarkCount),
        typeof(int),
        typeof(MixerSlider),
        11,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TickMarkLengthProperty = BindableProperty.Create(
        nameof(TickMarkLength),
        typeof(double),
        typeof(MixerSlider),
        24.0,
        propertyChanged: OnPropertyChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public Color ThumbColor1
    {
        get => (Color)GetValue(ThumbColor1Property);
        set => SetValue(ThumbColor1Property, value);
    }

    public Color ThumbColor2
    {
        get => (Color)GetValue(ThumbColor2Property);
        set => SetValue(ThumbColor2Property, value);
    }

    public double TrackWidth
    {
        get => (double)GetValue(TrackWidthProperty);
        set => SetValue(TrackWidthProperty, value);
    }

    public double ThumbWidth
    {
        get => (double)GetValue(ThumbWidthProperty);
        set => SetValue(ThumbWidthProperty, value);
    }

    public double ThumbHeight
    {
        get => (double)GetValue(ThumbHeightProperty);
        set => SetValue(ThumbHeightProperty, value);
    }

    public bool HasTickMarks
    {
        get => (bool)GetValue(HasTickMarksProperty);
        set => SetValue(HasTickMarksProperty, value);
    }

    public Color TickMarkColor
    {
        get => (Color)GetValue(TickMarkColorProperty);
        set => SetValue(TickMarkColorProperty, value);
    }

    public int TickMarkCount
    {
        get => (int)GetValue(TickMarkCountProperty);
        set => SetValue(TickMarkCountProperty, value);
    }

    public double TickMarkLength
    {
        get => (double)GetValue(TickMarkLengthProperty);
        set => SetValue(TickMarkLengthProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MixerSlider()
    {
        Drawable = this;

        StartInteraction += (_, e) => OnStartInteraction(e.Touches[0]);
        DragInteraction += (_, e) => OnDragInteraction(e.Touches[0]);
        EndInteraction += (_, _) => OnEndInteraction();
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixerSlider)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var thumbX = dirtyRect.Width / 2;

        var valuePercentage = (Value - Minimum) / (Maximum - Minimum);

        var trackTop = (float)(ThumbHeight / 2);
        var trackBottom = dirtyRect.Height - (float)(ThumbHeight / 2);
        var trackHeight = trackBottom - trackTop;

        var thumbY = (float)(trackBottom - (valuePercentage * trackHeight));

        if (HasTickMarks && (TickMarkCount > 1))
        {
            canvas.StrokeColor = TickMarkColor;
            canvas.StrokeSize = TickWidth;

            var tickStart = thumbX - (float)TickMarkLength;
            var tickEnd = thumbX + (float)TickMarkLength;
            for (var i = 0; i < TickMarkCount; i++)
            {
                var tickPercentage = (float)i / (TickMarkCount - 1);
                var tickY = trackBottom - (tickPercentage * trackHeight);
                canvas.DrawLine(tickStart, tickY, tickEnd, tickY);
            }
        }

        canvas.StrokeSize = (float)TrackWidth;
        canvas.StrokeColor = TrackColor;
        canvas.DrawLine(thumbX, trackTop - TickWidth, thumbX, trackBottom + TickWidth);

        canvas.StrokeColor = ProgressColor;
        canvas.DrawLine(thumbX, thumbY - TickWidth, thumbX, trackBottom + TickWidth);

        var thumbHalfWidth = (float)(ThumbWidth / 2);
        var thumbHalfHeight = (float)(ThumbHeight / 2);

        var thumbRect = new RectF(thumbX - thumbHalfWidth, thumbY - thumbHalfHeight, (float)ThumbWidth, (float)ThumbHeight);

        canvas.FillColor = ThumbColor1;
        canvas.FillRectangle(new RectF(thumbRect.X, thumbRect.Y, thumbRect.Width, thumbRect.Height * ThumbBandRate));

        canvas.FillColor = ThumbColor2;
        canvas.FillRectangle(new RectF(thumbRect.X, thumbRect.Y + (thumbRect.Height * (1 - ThumbBandRate)), thumbRect.Width, thumbRect.Height * ThumbBandRate));

        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = ThumbGradientStartPoint,
            EndPoint = ThumbGradientEndPoint
        };
        gradientBrush.GradientStops.Add(new GradientStop(ThumbColor2, 0));
        gradientBrush.GradientStops.Add(new GradientStop(ThumbColor1, 1));

        var thumbMiddleRect = new RectF(thumbRect.X, thumbRect.Y + (thumbRect.Height * ThumbBandRate), thumbRect.Width, thumbRect.Height * (1 - (ThumbBandRate * 2)));
        canvas.SetFillPaint(gradientBrush, thumbMiddleRect);
        canvas.FillRectangle(thumbMiddleRect);
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void OnStartInteraction(PointF point)
    {
        if (IsPointInThumb(point))
        {
            isDragging = true;
        }
        UpdateValue(point);
    }

    private void OnDragInteraction(PointF point)
    {
        if (isDragging)
        {
            UpdateValue(point);
        }
    }

    private void OnEndInteraction()
    {
        isDragging = false;
    }

    private void UpdateValue(PointF point)
    {
        var trackHeight = Height - ThumbHeight;
        var thumbPosition = point.Y - (ThumbHeight / 2);

        var percentage = 1 - Math.Clamp(thumbPosition / trackHeight, 0, 1);
        Value = Minimum + (percentage * (Maximum - Minimum));
    }

    private bool IsPointInThumb(PointF point)
    {
        var thumbX = Width / 2;

        var valuePercentage = (Value - Minimum) / (Maximum - Minimum);
        var trackTop = ThumbHeight / 2;
        var trackBottom = Height - (ThumbHeight / 2);
        var trackHeight = trackBottom - trackTop;
        var thumbY = (float)(trackBottom - (valuePercentage * trackHeight));

        var thumbRect = new RectF(
            (float)(thumbX - (ThumbWidth / 2) - ThumbTouchMargin),
            (float)(thumbY - (ThumbHeight / 2) - ThumbTouchMargin),
            (float)(ThumbWidth + (ThumbTouchMargin * 2)),
            (float)(ThumbHeight + (ThumbTouchMargin * 2)));

        return thumbRect.Contains(point);
    }
}

//--------------------------------------------------------------------------------
// Equalizer
//--------------------------------------------------------------------------------
public sealed class MixerEqualizer : GraphicsView, IDrawable
{
    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private int[] peakValues = [];

    private long[] peakTimes = [];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RangeProperty =
        BindableProperty.Create(nameof(Range), typeof(int), typeof(MixerEqualizer), 10, propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level),
        typeof(int),
        typeof(MixerEqualizer),
        10,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ValuesProperty = BindableProperty.Create(
        nameof(Values),
        typeof(int[]),
        typeof(MixerEqualizer),
        Array.Empty<int>(),
        propertyChanged: OnValuesChanged);

    public static readonly BindableProperty PeakHoldDurationProperty = BindableProperty.Create(
        nameof(PeakHoldDuration),
        typeof(int),
        typeof(MixerEqualizer),
        1000,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.Gray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty StartColorProperty = BindableProperty.Create(
        nameof(StartColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.DarkGreen,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty EndColorProperty = BindableProperty.Create(
        nameof(EndColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.LightGreen,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty HorizontalSpacingProperty = BindableProperty.Create(
        nameof(HorizontalSpacing),
        typeof(double),
        typeof(MixerEqualizer),
        2d,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty VerticalSpacingProperty = BindableProperty.Create(
        nameof(VerticalSpacing),
        typeof(double),
        typeof(MixerEqualizer),
        2d,
        propertyChanged: OnPropertyChanged);

    public int Range
    {
        get => (int)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    public int Level
    {
        get => (int)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

#pragma warning disable CA1819
    public int[] Values
    {
        get => (int[])GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }
#pragma warning restore CA1819

    public int PeakHoldDuration
    {
        get => (int)GetValue(PeakHoldDurationProperty);
        set => SetValue(PeakHoldDurationProperty, value);
    }

    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MixerEqualizer()
    {
        Drawable = this;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixerEqualizer)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ((Level <= 0) || (Range <= 0))
        {
            return;
        }

        var horizontalSpacing = (float)HorizontalSpacing;
        var verticalSpacing = (float)VerticalSpacing;

        var totalHorizontalSpacing = horizontalSpacing * (Range - 1);
        var totalVerticalSpacing = verticalSpacing * (Level - 1);

        var cellWidth = (dirtyRect.Width - totalHorizontalSpacing) / Range;
        var cellHeight = (dirtyRect.Height - totalVerticalSpacing) / Level;

        var values = Values;
        for (var i = 0; i < Range; i++)
        {
            var value = (i < values.Length) ? Math.Clamp(values[i], 0, Level) : 0;
            var x = i * (cellWidth + horizontalSpacing);

            for (var j = 0; j < Level; j++)
            {
                var y = dirtyRect.Height - ((j + 1) * cellHeight) - (j * verticalSpacing);

                canvas.FillColor = j < value ? LerpColor(StartColor, EndColor, (float)j / (Level - 1)) : InactiveColor;
                canvas.FillRectangle(x, y, cellWidth, cellHeight);
            }

            var peak = (i < peakValues.Length) ? peakValues[i] : 0;
            if (peak > value)
            {
                var j = peak - 1;
                var y = dirtyRect.Height - ((j + 1) * cellHeight) - (j * verticalSpacing);

                canvas.FillColor = LerpColor(StartColor, EndColor, (float)j / (Level - 1));
                canvas.FillRectangle(x, y, cellWidth, cellHeight);
            }
        }
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

    //--------------------------------------------------------------------------------
    // Peak
    //--------------------------------------------------------------------------------

    private static void OnValuesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MixerEqualizer)bindable;
        control.UpdatePeaks();
        control.Invalidate();
    }

    private void UpdatePeaks()
    {
        var range = Range;
        if (peakValues.Length != range)
        {
            peakValues = new int[range];
            peakTimes = new long[range];
        }

        var values = Values;
        var level = Level;
        var now = Environment.TickCount64;
        var holdDuration = PeakHoldDuration;

        for (var i = 0; i < range; i++)
        {
            var value = (i < values.Length) ? Math.Clamp(values[i], 0, level) : 0;
            if ((value >= peakValues[i]) || (now - peakTimes[i] > holdDuration))
            {
                peakValues[i] = value;
                peakTimes[i] = now;
            }
        }
    }
}
