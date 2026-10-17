namespace Template.MobileApp.Graphics.Drawing;

public enum WheelEffect
{
    None,
    Sparkle,
    Confetti
}

public sealed record WheelItem(string Label, WheelEffect Effect = WheelEffect.Sparkle);

public sealed class WheelDrawing : DrawingObject
{
    private const string SpinAnimationName = "WheelSpin";
    private const string EffectAnimationName = "WheelEffect";

    private const int BulbCount = 24;
    private const float RimWidth = 18f;
    private const float HubRadius = 28f;

    private const int ConfettiCount = 110;
    private const int SparkleCount = 12;

    private static readonly Color[] SegmentColors =
    [
        Color.FromArgb("#EF5350"),
        Color.FromArgb("#FFB300"),
        Color.FromArgb("#66BB6A"),
        Color.FromArgb("#42A5F5"),
        Color.FromArgb("#AB47BC"),
        Color.FromArgb("#FF7043"),
        Color.FromArgb("#26C6DA"),
        Color.FromArgb("#EC407A")
    ];

    private static readonly RadialGradientPaint[] SegmentPaints = SegmentColors
        .Select(static c => new RadialGradientPaint(
            [
                new PaintGradientStop(0f, c.AddLuminosity(0.18f)),
                new PaintGradientStop(0.75f, c),
                new PaintGradientStop(1f, c.AddLuminosity(-0.12f))
            ],
            new Point(0.5, 0.5),
            0.5))
        .ToArray();

    private static readonly RadialGradientPaint HubPaint = new(
        [
            new PaintGradientStop(0f, Colors.White),
            new PaintGradientStop(0.7f, Color.FromArgb("#ECEFF1")),
            new PaintGradientStop(1f, Color.FromArgb("#B0BEC5"))
        ],
        new Point(0.35, 0.35),
        0.7);

    private static readonly Color RimColor = Color.FromArgb("#37474F");
    private static readonly Color RimEdgeColor = Color.FromArgb("#FFD54F");
    private static readonly Color BulbOnColor = Color.FromArgb("#FFF59D");
    private static readonly Color BulbOffColor = Color.FromArgb("#8D6E63");
    private static readonly Color HubBorderColor = Color.FromArgb("#78909C");
    private static readonly Color PointerColor = Color.FromArgb("#E53935");
    private static readonly Color PointerEdgeColor = Color.FromArgb("#B71C1C");
    private static readonly Color SparkleColor = Color.FromArgb("#FFEE58");

    private IReadOnlyList<WheelItem> items = [];

    private float rotation;

    private float effectProgress;
    private int winnerIndex = -1;

    public bool IsSpinning { get; private set; }

    public static Color ColorOf(int index, int count) => SegmentColors[ColorIndex(index, count)];

    public void SetItems(IReadOnlyList<WheelItem> values)
    {
        items = values;
        rotation = 0f;
        winnerIndex = -1;
        effectProgress = 0f;
        Invalidate();
    }

    public bool Spin(float extraRotation, uint length, Action<WheelItem>? completed = null)
    {
        if (IsSpinning || (items.Count == 0))
        {
            return false;
        }

        AbortAnimation(EffectAnimationName);
        effectProgress = 0f;
        winnerIndex = -1;

        IsSpinning = true;
        AnimateValue(
            SpinAnimationName,
            rotation,
            rotation + extraRotation,
            length,
            Easing.CubicOut,
            v => rotation = (float)v,
            () =>
            {
                IsSpinning = false;
                rotation = Normalize(rotation);
                winnerIndex = GetWinnerIndex();
                StartEffect();
                completed?.Invoke(items[winnerIndex]);
            });
        return true;
    }

    public void CancelSpin()
    {
        AbortAnimation(EffectAnimationName);
        effectProgress = 0f;
        if (IsSpinning)
        {
            AbortAnimation(SpinAnimationName);
            IsSpinning = false;
            rotation = Normalize(rotation);
        }
    }

    private void StartEffect()
    {
        if (items[winnerIndex].Effect == WheelEffect.None)
        {
            return;
        }

        AnimateValue(
            EffectAnimationName,
            0d,
            1d,
            items[winnerIndex].Effect == WheelEffect.Confetti ? 3200u : 2400u,
            Easing.Linear,
            v => effectProgress = (float)v,
            () => effectProgress = 0f);
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        if (items.Count == 0)
        {
            return;
        }

        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - RimWidth - 20f;
        if (radius <= 0f)
        {
            return;
        }

        canvas.Antialias = true;

        DrawRim(canvas, cx, cy, radius);
        DrawSegments(canvas, cx, cy, radius);
        DrawLabels(canvas, cx, cy, radius);
        DrawHub(canvas, cx, cy);
        DrawPointer(canvas, cx, cy, radius);
        DrawEffect(canvas, dirtyRect, cx, cy, radius);
    }

    private void DrawRim(ICanvas canvas, float cx, float cy, float radius)
    {
        var outer = radius + RimWidth;

        canvas.SaveState();
        canvas.SetShadow(new SizeF(0f, 6f), 14f, Colors.Black.WithAlpha(0.35f));
        canvas.FillColor = RimColor;
        canvas.FillCircle(cx, cy, outer);
        canvas.RestoreState();

        canvas.StrokeColor = RimEdgeColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, outer - 1f);
        canvas.DrawCircle(cx, cy, radius + 1f);

        var bulbRadius = radius + (RimWidth / 2f);
        var chase = (int)(rotation / 5f);
        var blink = (int)(effectProgress * 12f) % 2;
        for (var i = 0; i < BulbCount; i++)
        {
            var lit = IsSpinning
                ? ((i + chase) % 3) == 0
                : (effectProgress <= 0f) || (((i % 2) == blink) && (winnerIndex >= 0));

            var angle = (i * 360f / BulbCount) * MathF.PI / 180f;
            var x = cx + (bulbRadius * MathF.Cos(angle));
            var y = cy + (bulbRadius * MathF.Sin(angle));

            if (lit)
            {
                canvas.FillColor = BulbOnColor.WithAlpha(0.35f);
                canvas.FillCircle(x, y, 7f);
            }

            canvas.FillColor = lit ? BulbOnColor : BulbOffColor;
            canvas.FillCircle(x, y, 3.5f);
        }
    }

    private void DrawSegments(ICanvas canvas, float cx, float cy, float radius)
    {
        var sweep = 360f / items.Count;
        var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);

        canvas.SaveState();
        for (var i = 0; i < items.Count; i++)
        {
            using var path = CreateSegmentPath(cx, cy, rect, i, sweep);

            canvas.SetFillPaint(SegmentPaints[ColorIndex(i, items.Count)], rect);
            canvas.FillPath(path);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2f;
            canvas.DrawPath(path);
        }
        canvas.RestoreState();

        if ((winnerIndex >= 0) && (effectProgress > 0f))
        {
            using var path = CreateSegmentPath(cx, cy, rect, winnerIndex, sweep);
            var pulse = 0.5f + (0.5f * MathF.Sin(effectProgress * MathF.PI * 6f));
            canvas.FillColor = Colors.White.WithAlpha(0.15f + (0.25f * pulse));
            canvas.FillPath(path);
            canvas.StrokeColor = RimEdgeColor;
            canvas.StrokeSize = 3f;
            canvas.DrawPath(path);
        }
    }

    private static int ColorIndex(int index, int count) =>
        ((index > 0) && (index == count - 1) && ((index % SegmentColors.Length) == 0)) ? 1 : (index % SegmentColors.Length);

    private PathF CreateSegmentPath(float cx, float cy, RectF rect, int index, float sweep)
    {
        var startAngle = -(rotation - 90f + (index * sweep));
        var path = new PathF();
        path.MoveTo(cx, cy);
        path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle - sweep, true);
        path.Close();
        return path;
    }

    private void DrawLabels(ICanvas canvas, float cx, float cy, float radius)
    {
        var sweep = 360f / items.Count;

        canvas.SaveState();
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = 15f;
        canvas.FontColor = Colors.White;
        canvas.SetShadow(new SizeF(1f, 1f), 3f, Colors.Black.WithAlpha(0.5f));
        for (var i = 0; i < items.Count; i++)
        {
            var screenMid = rotation - 90f + ((i + 0.5f) * sweep);

            var flip = Normalize(screenMid) is > 90f and < 270f;
            canvas.SaveState();
            canvas.Rotate(flip ? screenMid + 180f : screenMid, cx, cy);
            if (flip)
            {
                canvas.DrawString(items[i].Label, cx - radius + 16f, cy + 5f, HorizontalAlignment.Left);
            }
            else
            {
                canvas.DrawString(items[i].Label, cx + radius - 16f, cy + 5f, HorizontalAlignment.Right);
            }
            canvas.RestoreState();
        }
        canvas.RestoreState();
    }

    private static void DrawHub(ICanvas canvas, float cx, float cy)
    {
        var rect = new RectF(cx - HubRadius, cy - HubRadius, HubRadius * 2f, HubRadius * 2f);

        canvas.SaveState();
        canvas.SetShadow(new SizeF(0f, 2f), 6f, Colors.Black.WithAlpha(0.35f));
        canvas.SetFillPaint(HubPaint, rect);
        canvas.FillCircle(cx, cy, HubRadius);
        canvas.RestoreState();

        canvas.StrokeColor = HubBorderColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, HubRadius);
        canvas.StrokeColor = RimEdgeColor;
        canvas.StrokeSize = 1.5f;
        canvas.DrawCircle(cx, cy, HubRadius - 6f);
        canvas.FillColor = RimColor;
        canvas.FillCircle(cx, cy, 5f);
    }

    private static void DrawPointer(ICanvas canvas, float cx, float cy, float radius)
    {
        var top = cy - radius - RimWidth - 6f;
        var tip = cy - radius + 16f;

        using var pointer = new PathF();
        pointer.MoveTo(cx, tip);
        pointer.LineTo(cx - 13f, top);
        pointer.LineTo(cx + 13f, top);
        pointer.Close();

        canvas.SaveState();
        canvas.SetShadow(new SizeF(0f, 3f), 6f, Colors.Black.WithAlpha(0.4f));
        canvas.FillColor = PointerColor;
        canvas.FillPath(pointer);
        canvas.RestoreState();

        canvas.StrokeColor = PointerEdgeColor;
        canvas.StrokeSize = 2f;
        canvas.DrawPath(pointer);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx, top + 6f, 3.5f);
    }

    private void DrawEffect(ICanvas canvas, RectF dirtyRect, float cx, float cy, float radius)
    {
        if ((winnerIndex < 0) || (effectProgress <= 0f))
        {
            return;
        }

        switch (items[winnerIndex].Effect)
        {
            case WheelEffect.Sparkle:
                DrawSparkles(canvas, cx, cy, radius);
                break;
            case WheelEffect.Confetti:
                DrawConfetti(canvas, dirtyRect);
                break;
        }
    }

    private void DrawSparkles(ICanvas canvas, float cx, float cy, float radius)
    {
        for (var i = 0; i < SparkleCount; i++)
        {
            var seed = Hash(i + 1);
            var phase = Frac(seed);
            var twinkle = MathF.Sin(((effectProgress * 3f) + phase) * MathF.PI);
            if (twinkle <= 0f)
            {
                continue;
            }

            var angle = (-90f + ((Frac(seed * 7.13f) - 0.5f) * 80f)) * MathF.PI / 180f;
            var distance = radius + 20f + (Frac(seed * 3.71f) * 60f);
            var x = cx + (distance * MathF.Cos(angle));
            var y = cy + (distance * MathF.Sin(angle));
            var size = 4f + (Frac(seed * 5.29f) * 6f);

            DrawStar(canvas, x, y, size * twinkle, SparkleColor.WithAlpha(twinkle));
        }
    }

    private void DrawConfetti(ICanvas canvas, RectF dirtyRect)
    {
        for (var i = 0; i < ConfettiCount; i++)
        {
            var seed = Hash(i + 1);
            var delay = Frac(seed * 3.7f) * 0.35f;
            var t = (effectProgress - delay) / 0.65f;
            if ((t <= 0f) || (t >= 1f))
            {
                continue;
            }

            var x = dirtyRect.Left + (Frac(seed) * dirtyRect.Width) + (24f * MathF.Sin((t * 7f) + (seed * 10f)));
            var y = dirtyRect.Top - 16f + ((dirtyRect.Height + 32f) * t * (0.8f + (Frac(seed * 5.3f) * 0.4f)));
            if (y > dirtyRect.Bottom)
            {
                continue;
            }

            var angle = (t * 540f * (Frac(seed * 2.1f) < 0.5f ? 1f : -1f)) + (seed * 360f);
            var width = 7f + (Frac(seed * 8.9f) * 7f);
            var height = 4f + (Frac(seed * 6.1f) * 3f);
            var alpha = t < 0.85f ? 1f : (1f - t) / 0.15f;
            var color = SegmentColors[(int)(Frac(seed * 9.7f) * SegmentColors.Length)];

            canvas.SaveState();
            canvas.Translate(x, y);
            canvas.Rotate(angle);
            canvas.FillColor = color.WithAlpha(alpha);
            canvas.FillRectangle(-width / 2f, -height / 2f, width, height);
            canvas.RestoreState();
        }
    }

    private static void DrawStar(ICanvas canvas, float x, float y, float size, Color color)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = 2f;
        canvas.DrawLine(x - size, y, x + size, y);
        canvas.DrawLine(x, y - size, x, y + size);
        var half = size * 0.45f;
        canvas.DrawLine(x - half, y - half, x + half, y + half);
        canvas.DrawLine(x - half, y + half, x + half, y - half);
    }

    private int GetWinnerIndex()
    {
        var sweep = 360f / items.Count;
        var local = Normalize(360f - rotation);
        return (int)(local / sweep) % items.Count;
    }

    private static float Normalize(float value) => ((value % 360f) + 360f) % 360f;

    private static float Hash(int seed)
    {
        var value = unchecked((uint)seed * 2654435761u);
        value ^= value >> 15;
        value = unchecked(value * 2246822519u);
        value ^= value >> 13;
        return (value & 0xFFFFFF) / (float)0x1000000;
    }

    private static float Frac(float value) => value - MathF.Floor(value);
}
