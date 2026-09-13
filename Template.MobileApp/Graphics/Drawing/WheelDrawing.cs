namespace Template.MobileApp.Graphics.Drawing;

// 停止時に再生する演出の種類 (項目ごとに指定する)
public enum WheelEffect
{
    None,
    Sparkle,
    Confetti
}

public sealed record WheelItem(string Label, WheelEffect Effect = WheelEffect.Sparkle);

// 抽選ホイール。扇形+回転テキストを描画し、Spin() で減速回転して停止時に当選項目を通知する。
// アニメーションは DrawingObject.AnimateValue (単発+完了通知) を使用する
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

    // 扇形は中心が明るく外周に向けて本来の色になる放射グラデーション (SetFillPaint でホイール矩形に対して塗る)
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

    // 画面上の回転角。0 で項目 0 の先頭が真上、時計回りに増加
    private float rotation;

    // 停止時の演出 (0〜1 で進行。対象は winnerIndex の項目)
    private float effectProgress;
    private int winnerIndex = -1;

    public bool IsSpinning { get; private set; }

    public void SetItems(IReadOnlyList<WheelItem> values)
    {
        items = values;
        rotation = 0f;
        winnerIndex = -1;
        effectProgress = 0f;
        Invalidate();
    }

    // extraRotation (度) だけ減速回転し、停止時に当選項目を通知する。実行中は false
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

    // 画面離脱時などに回転を中断する (完了通知は行われない)
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

    // 外輪: 影付きの暗い環に金縁と電球。電球は回転中に流れ、演出中は点滅する
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

    // 扇形 (screenStart: 真上を起点とした時計回りの画面角度。AddArc は反時計回り正のため符号反転して渡す)
    // グラデーションのシェーダは FillColor の変更では解除されないため、SaveState / RestoreState で囲んで後続の塗りに残さない
    private void DrawSegments(ICanvas canvas, float cx, float cy, float radius)
    {
        var sweep = 360f / items.Count;
        var rect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);

        canvas.SaveState();
        for (var i = 0; i < items.Count; i++)
        {
            using var path = CreateSegmentPath(cx, cy, rect, i, sweep);

            canvas.SetFillPaint(SegmentPaints[i % SegmentPaints.Length], rect);
            canvas.FillPath(path);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2f;
            canvas.DrawPath(path);
        }
        canvas.RestoreState();

        // 当選セグメントは演出中に明滅させる
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

    private PathF CreateSegmentPath(float cx, float cy, RectF rect, int index, float sweep)
    {
        var startAngle = -(rotation - 90f + (index * sweep));
        var path = new PathF();
        path.MoveTo(cx, cy);
        path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle - sweep, true);
        path.Close();
        return path;
    }

    // ラベル (セグメント中央の角度に合わせてキャンバスごと回転し、半径方向に沿って描く)
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

            // 左半分のセグメントは 180 度回し、外周から中心へ向けて描いて読める向きにする
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

    // ポインタ (真上・下向き)
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

    // 停止後の演出。Sparkle=ポインタ周辺のきらめき / Confetti=上から舞い落ちる紙吹雪
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

            // ポインタを中心とした扇状に散らす
            var angle = (-90f + ((Frac(seed * 7.13f) - 0.5f) * 80f)) * MathF.PI / 180f;
            var distance = radius + 20f + (Frac(seed * 3.71f) * 60f);
            var x = cx + (distance * MathF.Cos(angle));
            var y = cy + (distance * MathF.Sin(angle));
            var size = 4f + (Frac(seed * 5.29f) * 6f);

            DrawStar(canvas, x, y, size * twinkle, SparkleColor.WithAlpha(twinkle));
        }
    }

    // 紙吹雪: 画面上端から落ちる小さな長方形。落下速度・横揺れ・回転は seed から決めて毎フレーム同じ軌道を描く
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

    // ポインタ (真上 = 画面角度 270°) が指しているセグメント
    private int GetWinnerIndex()
    {
        var sweep = 360f / items.Count;
        var local = Normalize(360f - rotation);
        return (int)(local / sweep) % items.Count;
    }

    private static float Normalize(float value) => ((value % 360f) + 360f) % 360f;

    // 演出の散らばりに使う決定的な疑似乱数 (毎フレーム同じ配置になるよう seed から計算する)
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
