namespace Template.MobileApp.Graphics.Drawing;

public sealed class CompassDrawing : DrawingObject
{
    private static readonly Color DialColor = Color.FromArgb("#B0BEC5");
    private static readonly Color LetterColor = Color.FromArgb("#546E7A");
    private static readonly Color NorthColor = Color.FromArgb("#F44336");
    private static readonly Color SouthColor = Color.FromArgb("#90A4AE");

    public float Heading { get; set; }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - 4f;
        if (radius <= 0)
        {
            return;
        }

        // Outer circle and 30° marks
        canvas.StrokeColor = DialColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, radius);

        canvas.StrokeSize = 1.5f;
        for (var deg = 0; deg < 360; deg += 30)
        {
            var rad = (float)(deg * Math.PI / 180d);
            var sin = (float)Math.Sin(rad);
            var cos = (float)Math.Cos(rad);
            var len = deg % 90 == 0 ? 8f : 4f;
            canvas.DrawLine(
                cx + ((radius - len) * sin),
                cy - ((radius - len) * cos),
                cx + (radius * sin),
                cy - (radius * cos));
        }

        // Center circle
        canvas.FontSize = 12f;
        canvas.FontColor = NorthColor;
        canvas.DrawString("N", cx, cy - radius + 18f, HorizontalAlignment.Center);
        canvas.FontColor = LetterColor;
        canvas.DrawString("E", cx + radius - 14f, cy + 4f, HorizontalAlignment.Center);
        canvas.DrawString("S", cx, cy + radius - 10f, HorizontalAlignment.Center);
        canvas.DrawString("W", cx - radius + 14f, cy + 4f, HorizontalAlignment.Center);

        // Needle
        canvas.SaveState();
        canvas.Translate(cx, cy);
        canvas.Rotate(-Heading);

        var needle = radius - 16f;

        using var north = new PathF();
        north.MoveTo(0f, -needle);
        north.LineTo(5f, 0f);
        north.LineTo(-5f, 0f);
        north.Close();
        canvas.FillColor = NorthColor;
        canvas.FillPath(north);

        using var south = new PathF();
        south.MoveTo(0f, needle);
        south.LineTo(5f, 0f);
        south.LineTo(-5f, 0f);
        south.Close();
        canvas.FillColor = SouthColor;
        canvas.FillPath(south);

        canvas.RestoreState();

        canvas.FillColor = LetterColor;
        canvas.FillCircle(cx, cy, 3f);
    }
}

public sealed class LevelDrawing : DrawingObject
{
    private static readonly Color RingColor = Color.FromArgb("#B0BEC5");
    private static readonly Color CrossColor = Color.FromArgb("#CFD8DC");
    private static readonly Color LevelColor = Color.FromArgb("#4CAF50");
    private static readonly Color BubbleColor = Color.FromArgb("#2196F3");

    public float GravityX { get; set; }

    public float GravityY { get; set; }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - 4f;
        if (radius <= 0)
        {
            return;
        }

        // Outer circle, cross lines, and center target
        canvas.StrokeColor = CrossColor;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(cx - radius, cy, cx + radius, cy);
        canvas.DrawLine(cx, cy - radius, cx, cy + radius);

        canvas.StrokeColor = RingColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, radius);
        canvas.StrokeSize = 1f;
        canvas.DrawCircle(cx, cy, 10f);

        // Bubble
        var px = -GravityX * radius;
        var py = GravityY * radius;
        var max = radius - 10f;
        var dist = MathF.Sqrt((px * px) + (py * py));
        if (dist > max)
        {
            px = px / dist * max;
            py = py / dist * max;
        }

        canvas.FillColor = dist < 6f ? LevelColor : BubbleColor;
        canvas.FillCircle(cx + px, cy + py, 8f);
    }
}
