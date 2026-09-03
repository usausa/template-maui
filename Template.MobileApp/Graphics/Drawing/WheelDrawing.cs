namespace Template.MobileApp.Graphics.Drawing;

// 抽選ホイール。扇形+回転テキストを描画し、Spin() で減速回転して停止時に当選項目を通知する。
// アニメーションは DrawingObject.AnimateValue (単発+完了通知) を使用する
public sealed class WheelDrawing : DrawingObject
{
    private const string SpinAnimationName = "WheelSpin";

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

    private static readonly Color RimColor = Color.FromArgb("#E0E0E0");
    private static readonly Color HubColor = Colors.White;
    private static readonly Color HubBorderColor = Color.FromArgb("#BDBDBD");
    private static readonly Color PointerColor = Color.FromArgb("#37474F");

    private IReadOnlyList<string> items = [];

    // 画面上の回転角。0 で項目 0 の先頭が真上、時計回りに増加
    private float rotation;

    public bool IsSpinning { get; private set; }

    public void SetItems(IReadOnlyList<string> values)
    {
        items = values;
        rotation = 0f;
        Invalidate();
    }

    // extraRotation (度) だけ減速回転し、停止時に当選項目を通知する。実行中は false
    public bool Spin(float extraRotation, uint length, Action<string>? completed = null)
    {
        if (IsSpinning || (items.Count == 0))
        {
            return false;
        }

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
                completed?.Invoke(items[GetWinnerIndex()]);
            });
        return true;
    }

    // 画面離脱時などに回転を中断する (完了通知は行われない)
    public void CancelSpin()
    {
        if (IsSpinning)
        {
            AbortAnimation(SpinAnimationName);
            IsSpinning = false;
            rotation = Normalize(rotation);
        }
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        if (items.Count == 0)
        {
            return;
        }

        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - 24f;
        if (radius <= 0f)
        {
            return;
        }

        var sweep = 360f / items.Count;
        var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);

        canvas.Antialias = true;

        // Segments (screenStart: 真上を起点とした時計回りの画面角度。AddArc は反時計回り正のため符号反転して渡す)
        for (var i = 0; i < items.Count; i++)
        {
            var screenStart = rotation - 90f + (i * sweep);
            var startAngle = -screenStart;

            using var path = new PathF();
            path.MoveTo(cx, cy);
            path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle - sweep, true);
            path.Close();

            canvas.FillColor = SegmentColors[i % SegmentColors.Length];
            canvas.FillPath(path);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2f;
            canvas.DrawPath(path);
        }

        // Rotated labels (セグメント中央の角度に合わせてキャンバスごと回転し、半径方向に沿って描く)
        canvas.FontSize = 14f;
        canvas.FontColor = Colors.White;
        for (var i = 0; i < items.Count; i++)
        {
            var screenMid = rotation - 90f + ((i + 0.5f) * sweep);

            canvas.SaveState();
            canvas.Rotate(screenMid, cx, cy);
            canvas.DrawString(items[i], cx + radius - 14f, cy + 5f, HorizontalAlignment.Right);
            canvas.RestoreState();
        }

        // Rim
        canvas.StrokeColor = RimColor;
        canvas.StrokeSize = 4f;
        canvas.DrawCircle(cx, cy, radius);

        // Hub
        canvas.FillColor = HubColor;
        canvas.FillCircle(cx, cy, 26f);
        canvas.StrokeColor = HubBorderColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, 26f);
        canvas.FillColor = PointerColor;
        canvas.FillCircle(cx, cy, 5f);

        // Pointer (真上・下向き)
        using var pointer = new PathF();
        pointer.MoveTo(cx, cy - radius + 14f);
        pointer.LineTo(cx - 11f, cy - radius - 10f);
        pointer.LineTo(cx + 11f, cy - radius - 10f);
        pointer.Close();
        canvas.FillColor = PointerColor;
        canvas.FillPath(pointer);
    }

    // ポインタ (真上 = 画面角度 270°) が指しているセグメント
    private int GetWinnerIndex()
    {
        var sweep = 360f / items.Count;
        var local = Normalize(360f - rotation);
        return (int)(local / sweep) % items.Count;
    }

    private static float Normalize(float value) => ((value % 360f) + 360f) % 360f;
}
