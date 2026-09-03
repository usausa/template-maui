namespace Template.MobileApp.Controls;

public sealed class MixerEqualizer : GraphicsView, IDrawable
{
    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    // Setting

    public static readonly BindableProperty RangeProperty =
        BindableProperty.Create(nameof(Range), typeof(int), typeof(MixerEqualizer), 10, propertyChanged: OnPropertyChanged);

    public int Range
    {
        get => (int)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level),
        typeof(int),
        typeof(MixerEqualizer),
        10,
        propertyChanged: OnPropertyChanged);

    public int Level
    {
        get => (int)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    // Value

    public static readonly BindableProperty ValuesProperty = BindableProperty.Create(
        nameof(Values),
        typeof(int[]),
        typeof(MixerEqualizer),
        Array.Empty<int>(),
        propertyChanged: OnValuesChanged);

#pragma warning disable CA1819
    public int[] Values
    {
        get => (int[])GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }
#pragma warning restore CA1819

    // Peak hold

    public static readonly BindableProperty PeakHoldDurationProperty = BindableProperty.Create(
        nameof(PeakHoldDuration),
        typeof(int),
        typeof(MixerEqualizer),
        1000,
        propertyChanged: OnPropertyChanged);

    public int PeakHoldDuration
    {
        get => (int)GetValue(PeakHoldDurationProperty);
        set => SetValue(PeakHoldDurationProperty, value);
    }

    private int[] peakValues = [];

    private long[] peakTimes = [];

    // Color

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.Gray,
        propertyChanged: OnPropertyChanged);

    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    public static readonly BindableProperty StartColorProperty = BindableProperty.Create(
        nameof(StartColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.DarkGreen,
        propertyChanged: OnPropertyChanged);

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public static readonly BindableProperty EndColorProperty = BindableProperty.Create(
        nameof(EndColor),
        typeof(Color),
        typeof(MixerEqualizer),
        Colors.LightGreen,
        propertyChanged: OnPropertyChanged);

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    // Size

    public static readonly BindableProperty HorizontalSpacingProperty = BindableProperty.Create(
        nameof(HorizontalSpacing),
        typeof(double),
        typeof(MixerEqualizer),
        2d,
        propertyChanged: OnPropertyChanged);

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public static readonly BindableProperty VerticalSpacingProperty = BindableProperty.Create(
        nameof(VerticalSpacing),
        typeof(double),
        typeof(MixerEqualizer),
        2d,
        propertyChanged: OnPropertyChanged);

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public MixerEqualizer()
    {
        Drawable = this;
    }

    // ------------------------------------------------------------
    // Handler
    // ------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixerEqualizer)bindable).Invalidate();
    }

    private static void OnValuesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MixerEqualizer)bindable;
        control.UpdatePeaks();
        control.Invalidate();
    }

    // 各列のピーク値を一定時間ホールドする
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

    // ------------------------------------------------------------
    // Draw
    // ------------------------------------------------------------

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

                canvas.FillColor = j < value ? InterpolateColor(StartColor, EndColor, (float)j / (Level - 1)) : InactiveColor;
                canvas.FillRectangle(x, y, cellWidth, cellHeight);
            }

            // Peak hold cell
            var peak = (i < peakValues.Length) ? peakValues[i] : 0;
            if ((peak > value) && (peak > 0))
            {
                var j = peak - 1;
                var y = dirtyRect.Height - ((j + 1) * cellHeight) - (j * verticalSpacing);

                canvas.FillColor = InterpolateColor(StartColor, EndColor, (float)j / (Level - 1));
                canvas.FillRectangle(x, y, cellWidth, cellHeight);
            }
        }
    }

    private static Color InterpolateColor(Color c1, Color c2, float factor)
    {
        return new Color(
            c1.Red + ((c2.Red - c1.Red) * factor),
            c1.Green + ((c2.Green - c1.Green) * factor),
            c1.Blue + ((c2.Blue - c1.Blue) * factor),
            c1.Alpha + ((c2.Alpha - c1.Alpha) * factor));
    }
}
