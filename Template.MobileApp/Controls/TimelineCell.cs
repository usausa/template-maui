namespace Template.MobileApp.Controls;

public sealed class TimelineCell : GraphicsView, IDrawable
{
    private const float BaseWidth = 24;
    private const float BaseHeight = 32;
    private const float Radius = 5;
    private const float RadiusMargin = 1;

    private static readonly Color[] ColorTable =
    [
        Color.FromRgb(0xEE, 0x37, 0x6C),
        Color.FromRgb(0x56, 0x77, 0xCB),
        Color.FromRgb(0x51, 0xC6, 0xBF),
        Color.FromRgb(0xEE, 0xB6, 0x11)
    ];

    public static readonly BindableProperty RowProperty = BindableProperty.Create(
        nameof(Row),
        typeof(TimelineRow),
        typeof(TimelineCell),
        null,
        BindingMode.TwoWay,
        propertyChanged: HandlePropertyChanged);

    public TimelineRow Row
    {
        get => (TimelineRow)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    public TimelineCell()
    {
        Drawable = this;
        HeightRequest = BaseHeight;
        WidthRequest = BaseWidth * 3;
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        var control = (TimelineCell)bindable;
        control.Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var row = Row;

        var color = ColorTable[row.No % ColorTable.Length];
        var halfX = BaseWidth / 2;
        var halfY = BaseHeight / 2;
        var centerX = (BaseWidth * row.No) + halfX;

        canvas.Antialias = true;

        // Lines
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;
        for (var i = 0; i < row.LineNos.Length; i++)
        {
            canvas.StrokeColor = ColorTable[i % ColorTable.Length];
            var x = (BaseWidth * i) + halfX;
            canvas.DrawLine(x, 0, x, BaseHeight);
        }

        // Out/Out
        if (row.Out.HasValue)
        {
            canvas.StrokeColor = ColorTable[row.Out.Value % ColorTable.Length];
            var x = (BaseWidth * row.Out.Value) + halfX;
            canvas.DrawLine(centerX, halfY, x, 1);
            canvas.DrawLine(x, 1, x, -1);
        }
        if (row.In.HasValue)
        {
            canvas.StrokeColor = ColorTable[row.In.Value % ColorTable.Length];
            var x = (BaseWidth * row.In.Value) + halfX;
            canvas.DrawLine(centerX, halfY, x, BaseHeight - 1);
            canvas.DrawLine(x, BaseHeight - 1, x, BaseHeight + 1);
        }

        // Circle
        canvas.FillColor = Colors.White;
        canvas.FillCircle(centerX, halfY, Radius + RadiusMargin);

        canvas.FillColor = color;
        canvas.FillCircle(centerX, halfY, Radius);
    }
}
