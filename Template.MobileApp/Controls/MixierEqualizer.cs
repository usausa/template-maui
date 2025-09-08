namespace Template.MobileApp.Controls;

public sealed class MixierEqualizer : GraphicsView, IDrawable
{
    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    // Setting

    public static readonly BindableProperty RangeProperty =
        BindableProperty.Create(nameof(Range), typeof(int), typeof(MixierEqualizer), 10, propertyChanged: OnPropertyChanged);

    public int Range
    {
        get => (int)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level),
        typeof(int),
        typeof(MixierEqualizer),
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
        typeof(MixierEqualizer),
        Array.Empty<int>(),
        propertyChanged: OnPropertyChanged);

#pragma warning disable CA1819
    public int[] Values
    {
        get => (int[])GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }
#pragma warning restore CA1819

    // Color

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(MixierEqualizer),
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
        typeof(MixierEqualizer),
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
        typeof(MixierEqualizer),
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
        typeof(MixierEqualizer),
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
        typeof(MixierEqualizer),
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

    public MixierEqualizer()
    {
        Drawable = this;
    }

    // ------------------------------------------------------------
    // Handler
    // ------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MixierEqualizer)bindable).Invalidate();
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
