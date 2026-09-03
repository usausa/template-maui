namespace Template.MobileApp.Graphics.Drawing;

using System.Timers;

public enum ChartKind
{
    Line,
    Bar,
    Donut,
    Candle,
    Stacked,
    Scatter,
    Heat
}

public readonly record struct CandlePoint(double Open, double High, double Low, double Close);

public sealed class ChartDrawing : DrawingObject, IDisposable
{
    private const float AnimationDuration = 600f;
    private const float Padding = 24f;

    private static readonly Color BackgroundColor = Colors.White;
    private static readonly Color AxisColor = Color.FromArgb("#E0E0E0");
    private static readonly Color TextColor = Color.FromArgb("#757575");
    private static readonly Color LineColor = Color.FromArgb("#2196F3");
    private static readonly Color BarColor = Color.FromArgb("#42A5F5");
    private static readonly Color UpColor = Color.FromArgb("#43A047");
    private static readonly Color DownColor = Color.FromArgb("#E53935");
    private static readonly Color HighColor = Color.FromArgb("#E53935");
    private static readonly Color HeatLowColor = Color.FromArgb("#42A5F5");
    private static readonly Color HeatMidColor = Color.FromArgb("#FFEE58");
    private static readonly Color HeatHighColor = Color.FromArgb("#E53935");

    private static readonly Color[] DonutColors =
    [
        Color.FromArgb("#2196F3"),
        Color.FromArgb("#4CAF50"),
        Color.FromArgb("#FFB300"),
        Color.FromArgb("#EC407A"),
        Color.FromArgb("#26C6DA"),
    ];

    private readonly System.Timers.Timer animationTimer = new(1000d / 60);

    private ChartKind kind = ChartKind.Line;

    private IReadOnlyList<double> values = [];

    private IReadOnlyList<CandlePoint> candles = [];

    private IReadOnlyList<double[]> series = [];

    private IReadOnlyList<PointF> points = [];

    private double[][] heatCells = [];

    private float progress = 1f;

    private long animationStart;

    public ChartDrawing()
    {
        animationTimer.Elapsed += TimerElapsed;
    }

    public void Dispose()
    {
        animationTimer.Dispose();
    }

    public void ShowLine(IReadOnlyList<double> data)
    {
        kind = ChartKind.Line;
        values = data;
        StartAnimation();
    }

    public void ShowBar(IReadOnlyList<double> data)
    {
        kind = ChartKind.Bar;
        values = data;
        StartAnimation();
    }

    public void ShowDonut(IReadOnlyList<double> data)
    {
        kind = ChartKind.Donut;
        values = data;
        StartAnimation();
    }

    public void ShowCandle(IReadOnlyList<CandlePoint> data)
    {
        kind = ChartKind.Candle;
        candles = data;
        StartAnimation();
    }

    // 積み上げ棒。data[カテゴリ][系列]
    public void ShowStacked(IReadOnlyList<double[]> data)
    {
        kind = ChartKind.Stacked;
        series = data;
        StartAnimation();
    }

    public void ShowScatter(IReadOnlyList<PointF> data)
    {
        kind = ChartKind.Scatter;
        points = data;
        StartAnimation();
    }

    // ヒートマップ。data[行][列]
    public void ShowHeat(double[][] data)
    {
        kind = ChartKind.Heat;
        heatCells = data;
        StartAnimation();
    }

    private float animationDuration = AnimationDuration;

    private void StartAnimation()
    {
        // ディレイ出現系は要素ごとの開始ずらし分だけ全体を長くする
        animationDuration = kind is ChartKind.Stacked or ChartKind.Scatter or ChartKind.Heat ? 1000f : AnimationDuration;
        progress = 0f;
        animationStart = Environment.TickCount64;
        animationTimer.Start();
        SafeInvalidate();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var elapsed = Environment.TickCount64 - animationStart;
        progress = Math.Min(1f, elapsed / animationDuration);
        if (progress >= 1f)
        {
            animationTimer.Stop();
        }

        SafeInvalidate();
    }

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);
        canvas.Antialias = true;

        // CubicOut
        var t = progress;
        var eased = 1f - ((1f - t) * (1f - t) * (1f - t));

        switch (kind)
        {
            case ChartKind.Line:
                DrawLineChart(canvas, dirtyRect, eased);
                break;
            case ChartKind.Bar:
                DrawBarChart(canvas, dirtyRect, eased);
                break;
            case ChartKind.Donut:
                DrawDonutChart(canvas, dirtyRect, eased);
                break;
            case ChartKind.Stacked:
                // 要素ごとにディレイをかけるため未イージングの進行値を渡す
                DrawStackedChart(canvas, dirtyRect, t);
                break;
            case ChartKind.Scatter:
                DrawScatterChart(canvas, dirtyRect, t);
                break;
            case ChartKind.Heat:
                DrawHeatChart(canvas, dirtyRect, t);
                break;
            default:
                DrawCandleChart(canvas, dirtyRect, eased);
                break;
        }

        canvas.RestoreState();
    }

    //--------------------------------------------------------------------------------
    // Area
    //--------------------------------------------------------------------------------

    private static RectF GetPlotArea(RectF dirtyRect) =>
        new(dirtyRect.Left + Padding, dirtyRect.Top + Padding, dirtyRect.Width - (Padding * 2), dirtyRect.Height - (Padding * 2));

    //--------------------------------------------------------------------------------
    // Line
    //--------------------------------------------------------------------------------

    private void DrawLineChart(ICanvas canvas, RectF dirtyRect, float eased)
    {
        var area = GetPlotArea(dirtyRect);
        DrawGrid(canvas, area);

        if (values.Count < 2)
        {
            return;
        }

        var min = values.Min();
        var max = values.Max();
        var range = Math.Max(1e-6, max - min);

        PointF GetPoint(int i) =>
            new(
                area.Left + (area.Width * i / (values.Count - 1)),
                area.Bottom - (float)((values[i] - min) / range * area.Height));

        // 左から右へ描画をクリップして伸ばす
        canvas.SaveState();
        canvas.ClipRectangle(area.Left, dirtyRect.Top, area.Width * eased, dirtyRect.Height);

        // 線下のグラデーション
        using (var fillPath = new PathF())
        {
            fillPath.MoveTo(area.Left, area.Bottom);
            for (var i = 0; i < values.Count; i++)
            {
                fillPath.LineTo(GetPoint(i));
            }
            fillPath.LineTo(area.Right, area.Bottom);
            fillPath.Close();

            var gradient = new LinearGradientPaint(
                [
                    new PaintGradientStop(0f, LineColor.WithAlpha(0.30f)),
                    new PaintGradientStop(1f, LineColor.WithAlpha(0.02f))
                ],
                startPoint: new Point(0, 0),
                endPoint: new Point(0, 1));
            canvas.SetFillPaint(gradient, area);
            canvas.FillPath(fillPath);
        }

        // 折れ線 (値の高さで色を補間し、線分ごとに塗り分けるグラデーション線。
        // ICanvas のストロークは単色のみのため、シェーダの代わりに区間単位の色補間で表現する)
        canvas.StrokeSize = 3f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeLineCap = LineCap.Round;
        for (var i = 1; i < values.Count; i++)
        {
            var ratio = (float)((((values[i - 1] + values[i]) / 2d) - min) / range);
            canvas.StrokeColor = Lerp(LineColor, HighColor, ratio);
            var p0 = GetPoint(i - 1);
            var p1 = GetPoint(i);
            canvas.DrawLine(p0.X, p0.Y, p1.X, p1.Y);
        }

        // 点 (枠線も値の色に合わせる)
        canvas.FillColor = Colors.White;
        canvas.StrokeSize = 2f;
        for (var i = 0; i < values.Count; i++)
        {
            var p = GetPoint(i);
            canvas.StrokeColor = Lerp(LineColor, HighColor, (float)((values[i] - min) / range));
            canvas.FillCircle(p.X, p.Y, 4f);
            canvas.DrawCircle(p.X, p.Y, 4f);
        }

        canvas.RestoreState();
    }

    //--------------------------------------------------------------------------------
    // Bar
    //--------------------------------------------------------------------------------

    private void DrawBarChart(ICanvas canvas, RectF dirtyRect, float eased)
    {
        var area = GetPlotArea(dirtyRect);
        DrawGrid(canvas, area);

        if (values.Count == 0)
        {
            return;
        }

        var max = Math.Max(1e-6, values.Max());
        var barWidth = area.Width / values.Count * 0.6f;
        var step = area.Width / values.Count;

        canvas.FillColor = BarColor;
        for (var i = 0; i < values.Count; i++)
        {
            var height = (float)(values[i] / max * area.Height) * eased;
            var x = area.Left + (step * i) + ((step - barWidth) / 2f);
            canvas.FillRoundedRectangle(x, area.Bottom - height, barWidth, height, 4f);
        }
    }

    //--------------------------------------------------------------------------------
    // Donut
    //--------------------------------------------------------------------------------

    private void DrawDonutChart(ICanvas canvas, RectF dirtyRect, float eased)
    {
        if (values.Count == 0)
        {
            return;
        }

        var total = values.Sum();
        if (total <= 0)
        {
            return;
        }

        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f) - Padding - 16f;
        const float thickness = 32f;
        var rect = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);

        canvas.StrokeSize = thickness;

        // 上(90 度)から時計回りに全体の eased 分までスイープ
        var startAngle = 90f;
        var remaining = 360f * eased;
        for (var i = 0; i < values.Count; i++)
        {
            var sweep = (float)(values[i] / total * 360f);
            var draw = Math.Min(sweep, remaining);
            if (draw <= 0f)
            {
                break;
            }

            canvas.StrokeColor = DonutColors[i % DonutColors.Length];
            canvas.DrawArc(rect, startAngle, startAngle - draw, true, false);
            startAngle -= sweep;
            remaining -= draw;
        }

        // 中央の合計値
        canvas.FontSize = 26f;
        canvas.FontColor = Color.FromArgb("#424242");
        canvas.DrawString($"{total * eased:N0}", cx, cy - 2f, HorizontalAlignment.Center);
        canvas.FontSize = 12f;
        canvas.FontColor = TextColor;
        canvas.DrawString("TOTAL", cx, cy + 18f, HorizontalAlignment.Center);
    }

    //--------------------------------------------------------------------------------
    // Candle
    //--------------------------------------------------------------------------------

    private void DrawCandleChart(ICanvas canvas, RectF dirtyRect, float eased)
    {
        var area = GetPlotArea(dirtyRect);
        DrawGrid(canvas, area);

        if (candles.Count == 0)
        {
            return;
        }

        var min = candles.Min(static x => x.Low);
        var max = candles.Max(static x => x.High);
        var range = Math.Max(1e-6, max - min);

        float ToY(double value) => area.Bottom - (float)((value - min) / range * area.Height);

        var step = area.Width / candles.Count;
        var bodyWidth = step * 0.55f;

        // 左から順に出現させる
        var visible = (int)Math.Ceiling(candles.Count * eased);
        for (var i = 0; i < visible; i++)
        {
            var candle = candles[i];
            var cx = area.Left + (step * i) + (step / 2f);
            var color = candle.Close >= candle.Open ? UpColor : DownColor;

            canvas.StrokeColor = color;
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(cx, ToY(candle.High), cx, ToY(candle.Low));

            var top = ToY(Math.Max(candle.Open, candle.Close));
            var bottom = ToY(Math.Min(candle.Open, candle.Close));
            canvas.FillColor = color;
            canvas.FillRoundedRectangle(cx - (bodyWidth / 2f), top, bodyWidth, Math.Max(2f, bottom - top), 2f);
        }
    }

    //--------------------------------------------------------------------------------
    // Stacked / Scatter / Heat (要素ごとのディレイ出現)
    //--------------------------------------------------------------------------------

    private void DrawStackedChart(ICanvas canvas, RectF dirtyRect, float t)
    {
        var area = GetPlotArea(dirtyRect);
        DrawGrid(canvas, area);

        if (series.Count == 0)
        {
            return;
        }

        var max = Math.Max(1e-6, series.Max(static x => x.Sum()));
        var step = area.Width / series.Count;
        var barWidth = step * 0.6f;

        for (var i = 0; i < series.Count; i++)
        {
            // カテゴリ (棒) ごとに開始を遅らせる
            var eased = EaseOut(ElementProgress(t, i, series.Count));
            if (eased <= 0f)
            {
                continue;
            }

            var x = area.Left + (step * i) + ((step - barWidth) / 2f);
            var y = area.Bottom;
            var row = series[i];
            for (var s = 0; s < row.Length; s++)
            {
                var height = (float)(row[s] / max * area.Height) * eased;
                canvas.FillColor = DonutColors[s % DonutColors.Length];
                canvas.FillRectangle(x, y - height, barWidth, height);
                y -= height;
            }
        }
    }

    private void DrawScatterChart(ICanvas canvas, RectF dirtyRect, float t)
    {
        var area = GetPlotArea(dirtyRect);
        DrawGrid(canvas, area);

        if (points.Count == 0)
        {
            return;
        }

        var minX = points.Min(static p => p.X);
        var maxX = points.Max(static p => p.X);
        var minY = points.Min(static p => p.Y);
        var maxY = points.Max(static p => p.Y);
        var rangeX = Math.Max(1e-6f, maxX - minX);
        var rangeY = Math.Max(1e-6f, maxY - minY);

        for (var i = 0; i < points.Count; i++)
        {
            // 点ごとに開始を遅らせて拡大しながら出現させる
            var eased = EaseOut(ElementProgress(t, i, points.Count));
            if (eased <= 0f)
            {
                continue;
            }

            var x = area.Left + ((points[i].X - minX) / rangeX * area.Width);
            var y = area.Bottom - ((points[i].Y - minY) / rangeY * area.Height);
            var radius = 6f * eased;

            canvas.FillColor = LineColor.WithAlpha(0.35f);
            canvas.FillCircle(x, y, radius + 3f);
            canvas.FillColor = LineColor;
            canvas.FillCircle(x, y, radius);
        }
    }

    private void DrawHeatChart(ICanvas canvas, RectF dirtyRect, float t)
    {
        if (heatCells.Length == 0)
        {
            return;
        }

        var area = GetPlotArea(dirtyRect);
        var rows = heatCells.Length;
        var cols = heatCells[0].Length;
        var min = heatCells.SelectMany(static x => x).Min();
        var max = heatCells.SelectMany(static x => x).Max();
        var range = Math.Max(1e-6, max - min);

        var cellWidth = area.Width / cols;
        var cellHeight = area.Height / rows;

        for (var r = 0; r < rows; r++)
        {
            // 行ごとに開始を遅らせてフェードイン
            var alpha = EaseOut(ElementProgress(t, r, rows));
            if (alpha <= 0f)
            {
                continue;
            }

            for (var c = 0; c < cols; c++)
            {
                var ratio = (float)((heatCells[r][c] - min) / range);
                var color = ratio < 0.5f
                    ? Lerp(HeatLowColor, HeatMidColor, ratio * 2f)
                    : Lerp(HeatMidColor, HeatHighColor, (ratio - 0.5f) * 2f);
                canvas.FillColor = color.WithAlpha(alpha);
                canvas.FillRoundedRectangle(
                    area.Left + (c * cellWidth) + 1f,
                    area.Top + (r * cellHeight) + 1f,
                    cellWidth - 2f,
                    cellHeight - 2f,
                    2f);
            }
        }
    }

    // 全体の進行 0..1 を、要素 index の開始をずらした 0..1 に変換する (LiveCharts2 の Delayed animations 相当)
    private static float ElementProgress(float t, int index, int count)
    {
        // 各要素は全体の 60% の長さで動き、残り 40% を開始のずらしに使う
        const float span = 0.6f;
        if (count <= 1)
        {
            return Math.Clamp(t / span, 0f, 1f);
        }

        var start = (1f - span) * index / (count - 1);
        return Math.Clamp((t - start) / span, 0f, 1f);
    }

    private static float EaseOut(float t) => 1f - ((1f - t) * (1f - t) * (1f - t));

    private static Color Lerp(Color from, Color to, float t) =>
        Color.FromRgba(
            from.Red + ((to.Red - from.Red) * t),
            from.Green + ((to.Green - from.Green) * t),
            from.Blue + ((to.Blue - from.Blue) * t),
            1f);

    private static void DrawGrid(ICanvas canvas, RectF area)
    {
        canvas.StrokeColor = AxisColor;
        canvas.StrokeSize = 1f;
        for (var i = 0; i <= 4; i++)
        {
            var y = area.Top + (area.Height * i / 4f);
            canvas.DrawLine(area.Left, y, area.Right, y);
        }
    }
}
