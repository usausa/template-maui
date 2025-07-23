namespace Template.MobileApp.Graphics;

public sealed class ActivityGraphics : GraphicsObject
{
    private const int Max = 10000;

    private const float StartAngle = 240f;
    private const float EndAngle = -60f;

    private const float CircleWidth = 24f;

    private static readonly Color ActiveColor = Color.FromArgb("#03A9F4");
    private static readonly Color CircleColor = Color.FromArgb("#ECEFF1");
    private static readonly Color BackgroundColor = Colors.White;

    public int Step
    {
        get;
        set
        {
            field = value;
            Invalidate();
        }
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2) - (CircleWidth / 2);

        var arcRect = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);

        // Arc
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeSize = CircleWidth;
        canvas.StrokeColor = CircleColor;
        canvas.DrawArc(arcRect, StartAngle, EndAngle, true, false);

        var value = Math.Min(Max, Step);
        var valueAngle = StartAngle - ((StartAngle - EndAngle) * ((float)value / Max));

        canvas.StrokeColor = ActiveColor;
        canvas.DrawArc(arcRect, StartAngle, valueAngle, true, false);
    }
}
