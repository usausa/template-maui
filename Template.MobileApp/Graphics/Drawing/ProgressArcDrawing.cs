namespace Template.MobileApp.Graphics.Drawing;

// ボタン背面のカウントダウンリング (mallibone の Countdown button 相当)。
// 単発アニメーション (AnimateValue) で残量を減らし、完走時のみ completed を通知する
public sealed class ProgressArcDrawing : DrawingObject
{
    private const string AnimationName = "CountdownArc";

    private static readonly Color TrackColor = Color.FromArgb("#ECEFF1");
    private static readonly Color ArcColor = Color.FromArgb("#1E88E5");

    private float remaining;

    public bool IsRunning { get; private set; }

    public bool Start(float seconds, Action? completed = null)
    {
        if (IsRunning || (seconds <= 0f))
        {
            return false;
        }

        IsRunning = true;
        AnimateValue(
            AnimationName,
            1d,
            0d,
            (uint)(seconds * 1000f),
            Easing.Linear,
            v => remaining = (float)v,
            () =>
            {
                IsRunning = false;
                completed?.Invoke();
            });
        return true;
    }

    public void Cancel()
    {
        if (IsRunning)
        {
            AbortAnimation(AnimationName);
            IsRunning = false;
            remaining = 0f;
            Invalidate();
        }
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - 6f;
        var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);

        canvas.Antialias = true;
        canvas.StrokeSize = 6f;
        canvas.StrokeLineCap = LineCap.Round;

        // トラック
        canvas.StrokeColor = TrackColor;
        canvas.DrawCircle(cx, cy, radius);

        // 残量 (上 90 度から時計回りに減っていく)
        if (remaining > 0f)
        {
            canvas.StrokeColor = ArcColor;
            canvas.DrawArc(rect, 90f, 90f - (remaining * 360f), true, false);
        }
    }
}
