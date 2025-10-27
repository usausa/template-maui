namespace Template.MobileApp.Controls;

public sealed class StatControl : GraphicsView, IDrawable
{
    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    public static readonly BindableProperty GraphColorProperty = BindableProperty.Create(
        nameof(GraphColor),
        typeof(Color),
        typeof(StatControl),
        Colors.Blue,
        propertyChanged: OnPropertyChanged);

    public Color GraphColor
    {
        get => (Color)GetValue(GraphColorProperty);
        set => SetValue(GraphColorProperty, value);
    }

    public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(
        nameof(HeaderHeight),
        typeof(int),
        typeof(StatControl),
        32,
        propertyChanged: OnPropertyChanged);

    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly BindableProperty HeaderPaddingProperty = BindableProperty.Create(
        nameof(HeaderPadding),
        typeof(float),
        typeof(StatControl),
        4f,
        propertyChanged: OnPropertyChanged);

    public float HeaderPadding
    {
        get => (float)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public static readonly BindableProperty LabelFontSizeProperty = BindableProperty.Create(
        nameof(LabelFontSize),
        typeof(float),
        typeof(StatControl),
        16f,
        propertyChanged: OnPropertyChanged);

    public float LabelFontSize
    {
        get => (float)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly BindableProperty ValueFontSizeProperty = BindableProperty.Create(
        nameof(ValueFontSize),
        typeof(float),
        typeof(StatControl),
        24f,
        propertyChanged: OnPropertyChanged);

    public float ValueFontSize
    {
        get => (float)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label),
        typeof(string),
        typeof(StatControl),
        "Usage",
        propertyChanged: OnPropertyChanged);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly BindableProperty UnitProperty = BindableProperty.Create(
        nameof(Unit),
        typeof(string),
        typeof(StatControl),
        "%",
        propertyChanged: OnPropertyChanged);

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
        nameof(MaxValue),
        typeof(float),
        typeof(StatControl),
        100.0f,
        propertyChanged: OnPropertyChanged);

    public float MaxValue
    {
        get => (float)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly BindableProperty DataSetProperty = BindableProperty.Create(
        nameof(DataSet),
        typeof(StatDataSet),
        typeof(StatControl),
        propertyChanged: OnDataSetChanged);

    public StatDataSet DataSet
    {
        get => (StatDataSet)GetValue(DataSetProperty);
        set => SetValue(DataSetProperty, value);
    }

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public StatControl()
    {
        Drawable = this;
    }

    // ------------------------------------------------------------
    // Handler
    // ------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((StatControl)bindable).Invalidate();
    }

    private static void OnDataSetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (StatControl)bindable;

        if (oldValue is StatDataSet oldDataSet)
        {
            oldDataSet.Updated -= control.OnDataSetUpdated;
        }

        if (newValue is StatDataSet newDataSet)
        {
            newDataSet.Updated += control.OnDataSetUpdated;
        }
    }

    private void OnDataSetUpdated(object? sender, EventArgs e)
    {
        Invalidate();
    }

    // ------------------------------------------------------------
    // Draw
    // ------------------------------------------------------------

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var statHeight = height - HeaderHeight;
        var headerPadding = HeaderPadding;
        var headerRect = new RectF(headerPadding, 0, width - (headerPadding * 2), HeaderHeight);

        var values = DataSet;
        var pointWidth = width / (values.Capacity - 1);
        var maxValueForScale = MaxValue > 0 ? MaxValue : 100f;
        var color = GraphColor;

        canvas.SaveState();
        canvas.Antialias = true;

        // Background
        var backgroundPaint = new LinearGradientPaint
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = [new PaintGradientStop(0.0f, color.WithAlpha(1.0f)), new PaintGradientStop(1.0f, color.WithAlpha(192f / 255f))]
        };
        canvas.SetFillPaint(backgroundPaint, dirtyRect);
        canvas.FillRectangle(dirtyRect);

        // Path
        var wavePath = new PathF();
        wavePath.MoveTo(0, height);

        for (var i = 0; i < values.Capacity; i++)
        {
            var x = i * pointWidth;
            var normalizedValue = values.GetValue(i) / maxValueForScale;
            var y = height - (normalizedValue * statHeight);
            wavePath.LineTo(x, y);
        }

        wavePath.LineTo(width, height);
        wavePath.Close();

        // Gradation
        var waveGradient = new LinearGradientPaint
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = [new PaintGradientStop(0.0f, Colors.White.WithAlpha(192f / 255f)), new PaintGradientStop(1.0f, Colors.White.WithAlpha(64f / 255f))]
        };
        canvas.SetFillPaint(waveGradient, dirtyRect);
        canvas.FillPath(wavePath);

        // Line
        using var linePath = new PathF();
        linePath.MoveTo(0, height - (values.GetValue(0) / maxValueForScale * statHeight));

        for (var i = 1; i < values.Capacity; i++)
        {
            var x = i * pointWidth;
            var y = height - (values.GetValue(i) / maxValueForScale * statHeight);
            linePath.LineTo(x, y);
        }

        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 1.5f;
        canvas.DrawPath(linePath);

        // Label
        canvas.FontColor = Colors.White;
        canvas.FontSize = LabelFontSize;
        canvas.DrawString(Label, headerRect, HorizontalAlignment.Left, VerticalAlignment.Top);

        // Value
        var currentValue = values.LastValue;
        var unit = Unit;
        var valueText = String.IsNullOrEmpty(unit) ? $"{currentValue:F1}" : $"{currentValue:F1} {unit}";

        canvas.FontColor = Colors.White;
        canvas.FontSize = ValueFontSize;
        canvas.DrawString(valueText, headerRect, HorizontalAlignment.Right, VerticalAlignment.Top);

        canvas.RestoreState();
    }
}
