namespace Template.MobileApp.Graphics.Drawing;

public sealed class ActivityDrawing : DrawingObject
{
    private const string AnimationName = "ActivityStep";

    private const int Max = 10000;

    private const float StartAngle = 240f;
    private const float EndAngle = -60f;

    private const float CircleWidth = 24f;

    private const int Segments = 90;

    private static readonly Color StartColor = Color.FromArgb("#22D3EE");
    private static readonly Color EndColor = Color.FromArgb("#3B82F6");
    private static readonly Color CircleColor = Color.FromArgb("#ECEFF1");
    private static readonly Color BackgroundColor = Colors.White;

    private float shownStep;

    public int Step
    {
        get;
        set
        {
            field = value;
            AnimateValue(AnimationName, shownStep, value, 600, Easing.CubicOut, v => shownStep = (float)v);
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

        var value = Math.Min(Max, shownStep);
        if (value <= 0)
        {
            return;
        }

        var sweep = (StartAngle - EndAngle) * (value / Max);
        var count = Math.Max(1, (int)MathF.Ceiling(Segments * (value / Max)));
        for (var i = 0; i < count; i++)
        {
            var from = StartAngle - (sweep * i / count);
            var to = StartAngle - (sweep * (i + 1) / count);
            canvas.StrokeColor = Blend(StartColor, EndColor, (i + 0.5f) / count);
            canvas.DrawArc(arcRect, from, to, true, false);
        }
    }

    private static Color Blend(Color from, Color to, float amount) =>
        new(from.Red + ((to.Red - from.Red) * amount), from.Green + ((to.Green - from.Green) * amount), from.Blue + ((to.Blue - from.Blue) * amount));
}
