namespace Template.MobileApp.Graphics.Drawing;

public sealed class PulseRingDrawing : DrawingObject
{
    private const string AnimationName = "PulseRing";

    private const int RingCount = 3;

    private static readonly Color AccentColor = Color.FromArgb("#1E88E5");

    private float phase;

    private bool running;

    public void Start()
    {
        if (running)
        {
            return;
        }

        running = true;
        Loop();
    }

    public void Stop()
    {
        running = false;
        AbortAnimation(AnimationName);
    }

    private void Loop()
    {
        AnimateValue(
            AnimationName,
            0d,
            1d,
            1800,
            Easing.Linear,
            v => phase = (float)v,
            () =>
            {
                if (running)
                {
                    Loop();
                }
            });
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var maxRadius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - 4f;
        const float coreRadius = 22f;

        canvas.Antialias = true;

        // Pulse
        for (var i = 0; i < RingCount; i++)
        {
            var p = (phase + ((float)i / RingCount)) % 1f;
            var radius = coreRadius + ((maxRadius - coreRadius) * p);
            canvas.StrokeColor = AccentColor.WithAlpha(0.45f * (1f - p));
            canvas.StrokeSize = 3f;
            canvas.DrawCircle(cx, cy, radius);
        }

        // Center circle
        canvas.FillColor = AccentColor;
        canvas.FillCircle(cx, cy, coreRadius);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx, cy, 6f);
    }
}
