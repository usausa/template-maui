namespace Template.MobileApp.Graphics.Drawing;

using System.Timers;

using Font = Microsoft.Maui.Graphics.Font;

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

// 値の目盛りの範囲と間隔
public readonly record struct ChartScale(double Min, double Max, double Step);

public sealed class ChartDrawing : DrawingObject, ITapDrawing, IDisposable
{
    private const float AnimationDuration = 600f;

    private const float Padding = 12f;

    // 左の値の目盛りの幅と、下のラベルの高さ
    private const float AxisWidth = 40f;

    private const float LabelHeight = 22f;

    private const float LegendHeight = 24f;

    private const float LegendRowHeight = 22f;

    private const float FontSize = 11f;

    private const float TooltipFontSize = 12f;

    private const float TooltipLineHeight = 16f;

    // 折れ線の点(半径 4・枠線 2)とにじみが収まる幅
    private const float PointExtent = 6f;

    // 選んでいない棒・区画の濃さ
    private const float DimAlpha = 0.4f;

    // 散布図で点を選べる距離
    private const float HitDistance = 24f;

    private const float DonutThickness = 32f;

    private static readonly ChartScale DefaultScale = new(0, 1, 0.25);

    private static readonly Color GridColor = Color.FromArgb("#E0E0E0");
    private static readonly Color TextColor = Color.FromArgb("#757575");
    private static readonly Color StrongTextColor = Color.FromArgb("#424242");
    private static readonly Color TooltipColor = Color.FromArgb("#37474F");
    private static readonly Color UpColor = Color.FromArgb("#43A047");
    private static readonly Color DownColor = Color.FromArgb("#E53935");
    private static readonly Color HeatLowColor = Color.FromArgb("#42A5F5");
    private static readonly Color HeatMidColor = Color.FromArgb("#FFEE58");
    private static readonly Color HeatHighColor = Color.FromArgb("#E53935");

    private static readonly Color[] SeriesColors =
    [
        Color.FromArgb("#2196F3"),
        Color.FromArgb("#4CAF50"),
        Color.FromArgb("#FFB300"),
        Color.FromArgb("#EC407A"),
        Color.FromArgb("#26C6DA")
    ];

    private readonly Timer animationTimer = new(1000d / 60);

    private ChartKind kind = ChartKind.Line;

    private double[] values = [];

    // 前の値から動かすときの開始の値と目盛り
    private double[] startValues = [];

    private ChartScale startScale = DefaultScale;

    private bool morph;

    private string[] labels = [];

    private string[] rowLabels = [];

    private CandlePoint[] candles = [];

    private double[][] series = [];

    private PointF[] points = [];

    private double[][] heatCells = [];

    private ChartScale scale = DefaultScale;

    private ChartScale xScale = DefaultScale;

    private float progress = 1f;

    private float animationDuration = AnimationDuration;

    private long animationStart;

    // 最後に描いた位置 (タップの判定に使う)
    private RectF lastRect;

    private RectF plotArea;

    private PointF donutCenter;

    private float donutRadius;

    public Color Background { get; set; } = Colors.White;

    public Color LineColor { get; set; } = Color.FromArgb("#2196F3");

    // 値の高い所の線の色。LineColor と同じなら 1 色
    public Color LineHighColor { get; set; } = Color.FromArgb("#E53935");

    // 棒の上と下の色
    public Color BarColor { get; set; } = Color.FromArgb("#64B5F6");

    public Color BarEndColor { get; set; } = Color.FromArgb("#1E88E5");

    // 棒の後ろの、目盛りの最大までの溝
    public Color TrackColor { get; set; } = Color.FromArgb("#EEF1F5");

    public bool ShowTrack { get; set; } = true;

    // 左の値の目盛りと横の線
    public bool ShowAxis { get; set; } = true;

    public string AxisFormat { get; set; } = "{0:N0}";

    public string ValueFormat { get; set; } = "{0:N0}";

    // 積み上げ棒の系列の名前 (上の凡例)
    public IReadOnlyList<string> SeriesNames { get; set; } = [];

    public int SelectedIndex { get; private set; } = -1;

    public ChartDrawing()
    {
        animationTimer.Elapsed += TimerElapsed;
    }

    public void Dispose()
    {
        animationTimer.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public void ShowLine(IReadOnlyList<double> data, IReadOnlyList<string>? names = null) =>
        ShowValues(ChartKind.Line, data, names);

    public void ShowBar(IReadOnlyList<double> data, IReadOnlyList<string>? names = null) =>
        ShowValues(ChartKind.Bar, data, names);

    // 名前は下の凡例
    public void ShowDonut(IReadOnlyList<double> data, IReadOnlyList<string>? names = null) =>
        ShowValues(ChartKind.Donut, data, names);

    public void ShowCandle(IReadOnlyList<CandlePoint> data, IReadOnlyList<string>? names = null)
    {
        candles = [.. data];
        scale = candles.Length > 0 ? NiceScale(candles.Min(static x => x.Low), candles.Max(static x => x.High), false) : DefaultScale;
        Start(ChartKind.Candle, names, false);
    }

    // 積み上げ棒。data[カテゴリ][系列]
    public void ShowStacked(IReadOnlyList<double[]> data, IReadOnlyList<string>? names = null)
    {
        series = [.. data];
        scale = series.Length > 0 ? NiceScale(0, series.Max(static x => x.Sum()), true) : DefaultScale;
        Start(ChartKind.Stacked, names, false);
    }

    public void ShowScatter(IReadOnlyList<PointF> data)
    {
        points = [.. data];
        xScale = points.Length > 0 ? NiceScale(points.Min(static p => p.X), points.Max(static p => p.X), false) : DefaultScale;
        scale = points.Length > 0 ? NiceScale(points.Min(static p => p.Y), points.Max(static p => p.Y), false) : DefaultScale;
        Start(ChartKind.Scatter, null, false);
    }

    // ヒートマップ。data[行][列]。行の名前は左、列の名前は下
    public void ShowHeat(double[][] data, IReadOnlyList<string>? rows = null, IReadOnlyList<string>? columns = null)
    {
        heatCells = data;
        rowLabels = rows is null ? [] : [.. rows];
        Start(ChartKind.Heat, columns, false);
    }

    public void Select(int index)
    {
        SelectedIndex = index;
        Invalidate();
    }

    public void OnTap(PointF point)
    {
        var index = HitTest(point);
        SelectedIndex = index == SelectedIndex ? -1 : index;
        Invalidate();
    }

    private void ShowValues(ChartKind next, IReadOnlyList<double> data, IReadOnlyList<string>? names)
    {
        // 同じ種類・同じ数の値に変えるときは、今の表示の値と目盛りから動かす
        var follow = (next == kind) && (next is ChartKind.Line or ChartKind.Bar) && (data.Count == values.Length) && (data.Count > 0);
        if (follow)
        {
            var eased = Ease(progress);
            startValues = [.. Enumerable.Range(0, values.Length).Select(i => ValueAt(i, eased))];
            startScale = ScaleAt(eased);
        }

        values = [.. data];
        scale = next switch
        {
            ChartKind.Bar => NiceScale(0, values.DefaultIfEmpty(0).Max(), true),
            ChartKind.Line when values.Length > 0 => NiceScale(values.Min(), values.Max(), false),
            _ => DefaultScale
        };
        Start(next, names, follow);
    }

    private void Start(ChartKind next, IReadOnlyList<string>? names, bool follow)
    {
        kind = next;
        labels = names is null ? [] : [.. names];
        morph = follow;
        SelectedIndex = -1;

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

    // 表示する値 (前の値から動かしているときは途中の値。棒は 0 から伸ばす)
    private double ValueAt(int index, float eased)
    {
        if (morph)
        {
            return startValues[index] + ((values[index] - startValues[index]) * eased);
        }

        return kind == ChartKind.Bar ? values[index] * eased : values[index];
    }

    private ChartScale ScaleAt(float eased) =>
        morph
            ? new ChartScale(
                startScale.Min + ((scale.Min - startScale.Min) * eased),
                startScale.Max + ((scale.Max - startScale.Max) * eased),
                scale.Step)
            : scale;

    //--------------------------------------------------------------------------------
    // Draw
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = Background;
        canvas.FillRectangle(dirtyRect);
        canvas.Antialias = true;
        canvas.Font = Font.Default;
        lastRect = dirtyRect;

        var t = progress;
        var eased = Ease(t);

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
    // Line
    //--------------------------------------------------------------------------------

    private void DrawLineChart(ICanvas canvas, RectF rect, float eased)
    {
        var area = PlotArea(rect, ShowAxis ? AxisWidth : 0f, labels.Length > 0, false);
        var current = ScaleAt(eased);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, area, current);
        }
        DrawLabels(canvas, area, values.Length, false);

        if (values.Length < 2)
        {
            return;
        }

        var min = values.Min();
        var range = Math.Max(1e-6, values.Max() - min);

        PointF GetPoint(int i) =>
            new(area.Left + (area.Width * i / (values.Length - 1)), ToY(area, current, ValueAt(i, eased)));

        Color ColorOf(double value) => Lerp(LineColor, LineHighColor, (float)((value - min) / range));

        canvas.SaveState();
        if (!morph)
        {
            // 左から右へ描画をクリップして伸ばす (両端の点が切れないよう、点の大きさの分だけ左右に広げる)
            canvas.ClipRectangle(area.Left - PointExtent, rect.Top, (area.Width * eased) + (PointExtent * 2), rect.Height);
        }

        // 線下のグラデーション
        using (var fillPath = new PathF())
        {
            fillPath.MoveTo(area.Left, area.Bottom);
            for (var i = 0; i < values.Length; i++)
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
            canvas.SaveState();
            canvas.SetFillPaint(gradient, area);
            canvas.FillPath(fillPath);
            canvas.RestoreState();
        }

        // 折れ線 (値の高さで色を補間し、線分ごとに塗り分けるグラデーション線。
        // ICanvas のストロークは単色のみのため、シェーダの代わりに区間単位の色補間で表現する)
        canvas.StrokeSize = 3f;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeLineCap = LineCap.Round;
        for (var i = 1; i < values.Length; i++)
        {
            canvas.StrokeColor = ColorOf((values[i - 1] + values[i]) / 2d);
            var p0 = GetPoint(i - 1);
            var p1 = GetPoint(i);
            canvas.DrawLine(p0.X, p0.Y, p1.X, p1.Y);
        }

        // 点 (枠線も値の色に合わせる)
        canvas.FillColor = Colors.White;
        canvas.StrokeSize = 2f;
        for (var i = 0; i < values.Length; i++)
        {
            var p = GetPoint(i);
            canvas.StrokeColor = ColorOf(values[i]);
            canvas.FillCircle(p.X, p.Y, 4f);
            canvas.DrawCircle(p.X, p.Y, 4f);
        }

        canvas.RestoreState();

        if (IsSelected(values.Length))
        {
            var p = GetPoint(SelectedIndex);
            canvas.StrokeColor = TextColor;
            canvas.StrokeSize = 1f;
            canvas.StrokeDashPattern = [4f, 3f];
            canvas.DrawLine(p.X, area.Top, p.X, area.Bottom);
            canvas.StrokeDashPattern = null;

            canvas.FillColor = ColorOf(values[SelectedIndex]);
            canvas.FillCircle(p.X, p.Y, 7f);
            canvas.FillColor = Colors.White;
            canvas.FillCircle(p.X, p.Y, 3f);

            DrawTooltip(canvas, new PointF(p.X, p.Y - 6f), [LabelOf(SelectedIndex), Format(ValueFormat, values[SelectedIndex])]);
        }
    }

    //--------------------------------------------------------------------------------
    // Bar
    //--------------------------------------------------------------------------------

    private void DrawBarChart(ICanvas canvas, RectF rect, float eased)
    {
        var area = PlotArea(rect, ShowAxis ? AxisWidth : 0f, labels.Length > 0, false);
        var current = ScaleAt(eased);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, area, current);
        }

        if (values.Length == 0)
        {
            return;
        }

        var step = area.Width / values.Length;
        var barWidth = step * 0.6f;
        for (var i = 0; i < values.Length; i++)
        {
            var x = area.Left + (step * i) + ((step - barWidth) / 2f);
            if (ShowTrack)
            {
                canvas.FillColor = TrackColor;
                canvas.FillRoundedRectangle(x, area.Top, barWidth, area.Height, 4f);
            }

            var top = ToY(area, current, ValueAt(i, eased));
            var height = area.Bottom - top;
            if (height > 0.5f)
            {
                // 上から下へのグラデーション。選んだ棒があるときは、ほかの棒を薄くする
                var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;
                var paint = new LinearGradientPaint(
                    [
                        new PaintGradientStop(0f, BarColor.WithAlpha(alpha)),
                        new PaintGradientStop(1f, BarEndColor.WithAlpha(alpha))
                    ],
                    startPoint: new Point(0, 0),
                    endPoint: new Point(0, 1));
                var bar = new RectF(x, top, barWidth, height);

                // グラデーションは FillColor を変えても残るため、状態ごと戻す
                canvas.SaveState();
                canvas.SetFillPaint(paint, bar);
                canvas.FillRoundedRectangle(bar, Math.Min(4f, height / 2f));
                canvas.RestoreState();
            }
        }

        DrawLabels(canvas, area, values.Length, true);

        if (IsSelected(values.Length))
        {
            var x = area.Left + (step * (SelectedIndex + 0.5f));
            DrawTooltip(canvas, new PointF(x, ToY(area, current, values[SelectedIndex]) - 2f), [LabelOf(SelectedIndex), Format(ValueFormat, values[SelectedIndex])]);
        }
    }

    //--------------------------------------------------------------------------------
    // Donut
    //--------------------------------------------------------------------------------

    private void DrawDonutChart(ICanvas canvas, RectF rect, float eased)
    {
        var total = values.Sum();
        if ((values.Length == 0) || (total <= 0))
        {
            return;
        }

        // 凡例は下に 2 列
        var legendRows = labels.Length > 0 ? (values.Length + 1) / 2 : 0;
        var legendHeight = legendRows > 0 ? (legendRows * LegendRowHeight) + Padding : 0f;
        var chart = new RectF(rect.Left, rect.Top, rect.Width, rect.Height - legendHeight);
        var cx = chart.Center.X;
        var cy = chart.Center.Y;
        var radius = Math.Max(DonutThickness, (Math.Min(chart.Width, chart.Height) / 2f) - Padding - 12f);
        donutCenter = new PointF(cx, cy);
        donutRadius = radius;
        var arc = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);

        // 上(90 度)から時計回りに全体の eased 分までスイープ。選んだ区画は太く、ほかは薄く
        var startAngle = 90f;
        var remaining = 360f * eased;
        for (var i = 0; i < values.Length; i++)
        {
            var sweep = (float)(values[i] / total * 360f);
            var draw = Math.Min(sweep, remaining);
            if (draw <= 0f)
            {
                break;
            }

            var selected = i == SelectedIndex;
            canvas.StrokeSize = selected ? DonutThickness + 10f : DonutThickness;
            canvas.StrokeColor = SeriesColors[i % SeriesColors.Length].WithAlpha((SelectedIndex < 0) || selected ? 1f : DimAlpha);
            canvas.DrawArc(arc, startAngle, startAngle - draw, true, false);
            startAngle -= sweep;
            remaining -= draw;
        }

        // 中央は合計。区画を選んだときは、その名前・値・割合
        var inner = radius - (DonutThickness / 2f);
        if (IsSelected(values.Length))
        {
            DrawCenterText(canvas, cx, cy, inner, LabelOf(SelectedIndex), Format(ValueFormat, values[SelectedIndex]), (values[SelectedIndex] / total).ToString("P0", CultureInfo.CurrentCulture));
        }
        else
        {
            DrawCenterText(canvas, cx, cy, inner, string.Empty, Format(ValueFormat, total * eased), "TOTAL");
        }

        // 凡例: 色・名前・割合
        if (legendRows > 0)
        {
            var columnWidth = (rect.Width - (Padding * 2)) / 2f;
            var top = rect.Bottom - legendHeight;
            canvas.FontSize = FontSize;
            for (var i = 0; i < values.Length; i++)
            {
                var row = i / 2;
                var x = rect.Left + Padding + ((i % 2) * columnWidth);
                var y = top + (row * LegendRowHeight);
                canvas.FillColor = SeriesColors[i % SeriesColors.Length];
                canvas.FillRoundedRectangle(x + 8f, y + 6f, 10f, 10f, 2f);
                canvas.Font = i == SelectedIndex ? Font.DefaultBold : Font.Default;
                canvas.FontColor = StrongTextColor;
                canvas.DrawString(LabelOf(i), x + 24f, y, columnWidth - 80f, LegendRowHeight, HorizontalAlignment.Left, VerticalAlignment.Center);
                canvas.FontColor = TextColor;
                canvas.DrawString((values[i] / total).ToString("P0", CultureInfo.CurrentCulture), x, y, columnWidth - 12f, LegendRowHeight, HorizontalAlignment.Right, VerticalAlignment.Center);
            }
            canvas.Font = Font.Default;
        }
    }

    private static void DrawCenterText(ICanvas canvas, float cx, float cy, float inner, string caption, string value, string footer)
    {
        var width = inner * 2f;
        canvas.Font = Font.Default;
        canvas.FontSize = 12f;
        canvas.FontColor = TextColor;
        canvas.DrawString(caption, cx - inner, cy - 34f, width, 16f, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Font.DefaultBold;
        canvas.FontSize = 24f;
        canvas.FontColor = StrongTextColor;
        canvas.DrawString(value, cx - inner, cy - 18f, width, 32f, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.Font = Font.Default;
        canvas.FontSize = 12f;
        canvas.FontColor = TextColor;
        canvas.DrawString(footer, cx - inner, cy + 14f, width, 16f, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    //--------------------------------------------------------------------------------
    // Candle
    //--------------------------------------------------------------------------------

    private void DrawCandleChart(ICanvas canvas, RectF rect, float eased)
    {
        var area = PlotArea(rect, ShowAxis ? AxisWidth : 0f, labels.Length > 0, false);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, area, scale);
        }
        DrawLabels(canvas, area, candles.Length, true);

        if (candles.Length == 0)
        {
            return;
        }

        var step = area.Width / candles.Length;
        var bodyWidth = step * 0.55f;

        // 左から順に出現させる
        var visible = (int)Math.Ceiling(candles.Length * eased);
        for (var i = 0; i < visible; i++)
        {
            var candle = candles[i];
            var cx = area.Left + (step * i) + (step / 2f);
            var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;
            var color = (candle.Close >= candle.Open ? UpColor : DownColor).WithAlpha(alpha);

            canvas.StrokeColor = color;
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(cx, ToY(area, scale, candle.High), cx, ToY(area, scale, candle.Low));

            var top = ToY(area, scale, Math.Max(candle.Open, candle.Close));
            var bottom = ToY(area, scale, Math.Min(candle.Open, candle.Close));
            canvas.FillColor = color;
            canvas.FillRoundedRectangle(cx - (bodyWidth / 2f), top, bodyWidth, Math.Max(2f, bottom - top), 2f);
        }

        if (IsSelected(candles.Length))
        {
            var candle = candles[SelectedIndex];
            var x = area.Left + (step * (SelectedIndex + 0.5f));
            DrawTooltip(
                canvas,
                new PointF(x, ToY(area, scale, candle.High) - 2f),
                [
                    LabelOf(SelectedIndex),
                    $"始 {Format(ValueFormat, candle.Open)}  終 {Format(ValueFormat, candle.Close)}",
                    $"高 {Format(ValueFormat, candle.High)}  安 {Format(ValueFormat, candle.Low)}"
                ]);
        }
    }

    //--------------------------------------------------------------------------------
    // Stacked / Scatter / Heat (要素ごとのディレイ出現)
    //--------------------------------------------------------------------------------

    private void DrawStackedChart(ICanvas canvas, RectF rect, float t)
    {
        var legend = SeriesNames.Count > 0;
        var area = PlotArea(rect, ShowAxis ? AxisWidth : 0f, labels.Length > 0, legend);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, area, scale);
        }
        if (legend)
        {
            DrawLegend(canvas, rect);
        }
        DrawLabels(canvas, area, series.Length, true);

        if (series.Length == 0)
        {
            return;
        }

        var step = area.Width / series.Length;
        var barWidth = step * 0.6f;

        for (var i = 0; i < series.Length; i++)
        {
            // カテゴリ (棒) ごとに開始を遅らせる
            var eased = Ease(ElementProgress(t, i, series.Length));
            if (eased <= 0f)
            {
                continue;
            }

            var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;
            var x = area.Left + (step * i) + ((step - barWidth) / 2f);
            var y = area.Bottom;
            var row = series[i];
            for (var s = 0; s < row.Length; s++)
            {
                var height = (float)(row[s] / scale.Max * area.Height) * eased;
                canvas.FillColor = SeriesColors[s % SeriesColors.Length].WithAlpha(alpha);
                canvas.FillRectangle(x, y - height, barWidth, height);
                y -= height;
            }
        }

        if (IsSelected(series.Length))
        {
            var row = series[SelectedIndex];
            var lines = new List<string> { LabelOf(SelectedIndex) };
            for (var s = 0; s < row.Length; s++)
            {
                var name = s < SeriesNames.Count ? SeriesNames[s] : $"{s + 1}";
                lines.Add($"{name} {Format(ValueFormat, row[s])}");
            }
            lines.Add($"合計 {Format(ValueFormat, row.Sum())}");

            var x = area.Left + (step * (SelectedIndex + 0.5f));
            DrawTooltip(canvas, new PointF(x, ToY(area, scale, row.Sum()) - 2f), lines);
        }
    }

    private void DrawScatterChart(ICanvas canvas, RectF rect, float t)
    {
        var area = PlotArea(rect, ShowAxis ? AxisWidth : 0f, true, false);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, area, scale);
        }
        DrawHorizontalAxis(canvas, area);

        for (var i = 0; i < points.Length; i++)
        {
            // 点ごとに開始を遅らせて拡大しながら出現させる
            var eased = Ease(ElementProgress(t, i, points.Length));
            if (eased <= 0f)
            {
                continue;
            }

            var p = ScatterPoint(area, i);
            var radius = 6f * eased;
            var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;

            canvas.FillColor = LineColor.WithAlpha(0.35f * alpha);
            canvas.FillCircle(p.X, p.Y, radius + 3f);
            canvas.FillColor = LineColor.WithAlpha(alpha);
            canvas.FillCircle(p.X, p.Y, radius);
        }

        if (IsSelected(points.Length))
        {
            var p = ScatterPoint(area, SelectedIndex);
            canvas.StrokeColor = LineColor;
            canvas.StrokeSize = 2f;
            canvas.DrawCircle(p.X, p.Y, 11f);
            DrawTooltip(canvas, new PointF(p.X, p.Y - 12f), [$"X {Format(ValueFormat, points[SelectedIndex].X)}", $"Y {Format(ValueFormat, points[SelectedIndex].Y)}"]);
        }
    }

    private PointF ScatterPoint(RectF area, int index) =>
        new(
            area.Left + (float)((points[index].X - xScale.Min) / (xScale.Max - xScale.Min) * area.Width),
            ToY(area, scale, points[index].Y));

    private void DrawHeatChart(ICanvas canvas, RectF rect, float t)
    {
        if (heatCells.Length == 0)
        {
            return;
        }

        var area = PlotArea(rect, rowLabels.Length > 0 ? 24f : 0f, labels.Length > 0, false);
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
            var alpha = Ease(ElementProgress(t, r, rows));
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
                var selected = (SelectedIndex < 0) || (SelectedIndex == (r * cols) + c);
                canvas.FillColor = color.WithAlpha(alpha * (selected ? 1f : 0.55f));
                canvas.FillRoundedRectangle(
                    area.Left + (c * cellWidth) + 1f,
                    area.Top + (r * cellHeight) + 1f,
                    cellWidth - 2f,
                    cellHeight - 2f,
                    2f);
            }
        }

        // 左の行の名前
        canvas.FontSize = FontSize;
        canvas.FontColor = TextColor;
        for (var r = 0; r < Math.Min(rows, rowLabels.Length); r++)
        {
            canvas.DrawString(rowLabels[r], area.Left - 28f, area.Top + (r * cellHeight), 24f, cellHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        DrawLabels(canvas, area, cols, true);

        if (IsSelected(rows * cols))
        {
            var r = SelectedIndex / cols;
            var c = SelectedIndex % cols;
            var x = area.Left + ((c + 0.5f) * cellWidth);
            var y = area.Top + (r * cellHeight);
            canvas.StrokeColor = StrongTextColor;
            canvas.StrokeSize = 2f;
            canvas.DrawRectangle(area.Left + (c * cellWidth), y, cellWidth, cellHeight);

            var name = $"{(r < rowLabels.Length ? rowLabels[r] : $"{r + 1}")} {(c < labels.Length ? labels[c] : $"{c + 1}")}";
            DrawTooltip(canvas, new PointF(x, y - 2f), [name, Format(ValueFormat, heatCells[r][c])]);
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

    //--------------------------------------------------------------------------------
    // Axis / Label / Legend / Tooltip
    //--------------------------------------------------------------------------------

    private RectF PlotArea(RectF rect, float left, bool bottomLabels, bool legend)
    {
        var x = rect.Left + Padding + left;
        var y = rect.Top + Padding + (legend ? LegendHeight : 0f);
        var width = rect.Right - Padding - x;
        var height = rect.Bottom - Padding - (bottomLabels ? LabelHeight : 0f) - y;
        plotArea = new RectF(x, y, Math.Max(1f, width), Math.Max(1f, height));
        return plotArea;
    }

    // 横の線と左の値
    private void DrawValueAxis(ICanvas canvas, RectF area, ChartScale current)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.Font = Font.Default;
        canvas.FontSize = FontSize;
        canvas.FontColor = TextColor;

        var first = Math.Ceiling((current.Min / current.Step) - 1e-6) * current.Step;
        var count = (int)Math.Floor(((current.Max - first) / current.Step) + 1e-6);
        for (var i = 0; i <= count; i++)
        {
            var value = first + (i * current.Step);
            var y = ToY(area, current, value);
            canvas.DrawLine(area.Left, y, area.Right, y);
            canvas.DrawString(Format(AxisFormat, value), area.Left - AxisWidth - 6f, y - 8f, AxisWidth, 16f, HorizontalAlignment.Right, VerticalAlignment.Center);
        }
    }

    // 散布図の下の値
    private void DrawHorizontalAxis(ICanvas canvas, RectF area)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.FontSize = FontSize;
        canvas.FontColor = TextColor;

        var count = (int)Math.Floor(((xScale.Max - xScale.Min) / xScale.Step) + 1e-6);
        for (var i = 0; i <= count; i++)
        {
            var value = xScale.Min + (i * xScale.Step);
            var x = area.Left + (float)((value - xScale.Min) / (xScale.Max - xScale.Min) * area.Width);
            canvas.DrawLine(x, area.Top, x, area.Bottom);
            canvas.DrawString(Format(AxisFormat, value), x - 20f, area.Bottom + 4f, 40f, LabelHeight - 4f, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }

    // 下のラベル。重なるときは間引く。選んだ項目は太字
    private void DrawLabels(ICanvas canvas, RectF area, int count, bool centered)
    {
        if ((labels.Length == 0) || (count == 0))
        {
            return;
        }

        canvas.FontSize = FontSize;
        var step = centered ? area.Width / count : area.Width / Math.Max(1, count - 1);
        var widest = labels.Max(x => canvas.GetStringSize(x, Font.Default, FontSize).Width) + 8f;
        var skip = Math.Max(1, (int)Math.Ceiling(widest / step));
        var width = Math.Max(step, widest);
        for (var i = 0; i < Math.Min(count, labels.Length); i += skip)
        {
            var x = centered ? area.Left + (step * (i + 0.5f)) : area.Left + (step * i);
            var left = Math.Clamp(x - (width / 2f), lastRect.Left, lastRect.Right - width);
            var selected = i == SelectedIndex;
            canvas.Font = selected ? Font.DefaultBold : Font.Default;
            canvas.FontColor = selected ? StrongTextColor : TextColor;
            canvas.DrawString(labels[i], left, area.Bottom + 4f, width, LabelHeight - 4f, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
        canvas.Font = Font.Default;
    }

    private void DrawLegend(ICanvas canvas, RectF rect)
    {
        var x = rect.Left + Padding;
        var y = rect.Top + Padding;
        canvas.Font = Font.Default;
        canvas.FontSize = FontSize;
        canvas.FontColor = TextColor;
        for (var i = 0; i < SeriesNames.Count; i++)
        {
            var width = canvas.GetStringSize(SeriesNames[i], Font.Default, FontSize).Width + 4f;
            canvas.FillColor = SeriesColors[i % SeriesColors.Length];
            canvas.FillRoundedRectangle(x, y + 5f, 10f, 10f, 2f);
            canvas.DrawString(SeriesNames[i], x + 14f, y, width, 20f, HorizontalAlignment.Left, VerticalAlignment.Center);
            x += 14f + width + 12f;
        }
    }

    // 点の上の吹き出し (上に入らないときは下)
    private void DrawTooltip(ICanvas canvas, PointF anchor, List<string> lines)
    {
        const float pointer = 6f;
        canvas.FontSize = TooltipFontSize;
        var width = lines.Max(x => canvas.GetStringSize(x, Font.DefaultBold, TooltipFontSize).Width) + 16f;
        var height = (lines.Count * TooltipLineHeight) + 8f;
        var x = Math.Clamp(anchor.X - (width / 2f), lastRect.Left + 2f, lastRect.Right - width - 2f);
        var above = anchor.Y - pointer - height >= lastRect.Top + 2f;
        var y = above ? anchor.Y - pointer - height : anchor.Y + pointer + 8f;

        canvas.FillColor = TooltipColor;
        canvas.FillRoundedRectangle(x, y, width, height, 4f);
        using (var path = new PathF())
        {
            var px = Math.Clamp(anchor.X, x + pointer + 2f, x + width - pointer - 2f);
            var edge = above ? y + height : y;
            path.MoveTo(px - pointer, edge);
            path.LineTo(px + pointer, edge);
            path.LineTo(px, above ? edge + pointer : edge - pointer);
            path.Close();
            canvas.FillPath(path);
        }

        canvas.FontColor = Colors.White;
        for (var i = 0; i < lines.Count; i++)
        {
            // 1 行目は名前、2 行目からは値 (太字)
            canvas.Font = i == 0 ? Font.Default : Font.DefaultBold;
            canvas.DrawString(lines[i], x, y + 4f + (i * TooltipLineHeight), width, TooltipLineHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        canvas.Font = Font.Default;
    }

    //--------------------------------------------------------------------------------
    // Hit test
    //--------------------------------------------------------------------------------

    private int HitTest(PointF point)
    {
        if (!lastRect.Contains(point))
        {
            return -1;
        }

        switch (kind)
        {
            case ChartKind.Line:
                if ((values.Length < 2) || (point.X < plotArea.Left - 16f) || (point.X > plotArea.Right + 16f))
                {
                    return -1;
                }
                return Math.Clamp((int)Math.Round((point.X - plotArea.Left) / (plotArea.Width / (values.Length - 1))), 0, values.Length - 1);
            case ChartKind.Bar:
                return CategoryAt(point, values.Length);
            case ChartKind.Candle:
                return CategoryAt(point, candles.Length);
            case ChartKind.Stacked:
                return CategoryAt(point, series.Length);
            case ChartKind.Donut:
                return DonutAt(point);
            case ChartKind.Scatter:
                return ScatterAt(point);
            default:
                return HeatAt(point);
        }
    }

    private int CategoryAt(PointF point, int count)
    {
        if ((count == 0) || (point.X < plotArea.Left) || (point.X >= plotArea.Right))
        {
            return -1;
        }

        return Math.Clamp((int)((point.X - plotArea.Left) / (plotArea.Width / count)), 0, count - 1);
    }

    private int DonutAt(PointF point)
    {
        var total = values.Sum();
        var dx = point.X - donutCenter.X;
        var dy = point.Y - donutCenter.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        if ((total <= 0) || (Math.Abs(distance - donutRadius) > (DonutThickness / 2f) + 12f))
        {
            return -1;
        }

        // 上から時計回りの角度
        var angle = ((MathF.Atan2(dx, -dy) * 180f / MathF.PI) + 360f) % 360f;
        var end = 0d;
        for (var i = 0; i < values.Length; i++)
        {
            end += values[i] / total * 360d;
            if (angle < end)
            {
                return i;
            }
        }
        return values.Length - 1;
    }

    private int ScatterAt(PointF point)
    {
        var index = -1;
        var nearest = HitDistance;
        for (var i = 0; i < points.Length; i++)
        {
            var p = ScatterPoint(plotArea, i);
            var distance = MathF.Sqrt(((p.X - point.X) * (p.X - point.X)) + ((p.Y - point.Y) * (p.Y - point.Y)));
            if (distance < nearest)
            {
                nearest = distance;
                index = i;
            }
        }
        return index;
    }

    private int HeatAt(PointF point)
    {
        if ((heatCells.Length == 0) || !plotArea.Contains(point))
        {
            return -1;
        }

        var cols = heatCells[0].Length;
        var r = Math.Clamp((int)((point.Y - plotArea.Top) / (plotArea.Height / heatCells.Length)), 0, heatCells.Length - 1);
        var c = Math.Clamp((int)((point.X - plotArea.Left) / (plotArea.Width / cols)), 0, cols - 1);
        return (r * cols) + c;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private bool IsSelected(int count) => (SelectedIndex >= 0) && (SelectedIndex < count) && (progress >= 1f);

    private string LabelOf(int index) => index < labels.Length ? labels[index] : $"{index + 1}";

    private static string Format(string format, double value) => String.Format(CultureInfo.CurrentCulture, format, value);

    private static float ToY(RectF area, ChartScale s, double value) =>
        area.Bottom - (float)((value - s.Min) / Math.Max(1e-9, s.Max - s.Min) * area.Height);

    // 目盛りが切りのよい値になる範囲 (目盛りは 4 本前後)
    private static ChartScale NiceScale(double min, double max, bool fromZero)
    {
        if (fromZero)
        {
            min = Math.Min(0, min);
        }
        if (max - min < 1e-9)
        {
            max = min + 1;
        }

        var step = NiceNumber((max - min) / 4);
        return new ChartScale(Math.Floor(min / step) * step, Math.Ceiling(max / step) * step, step);
    }

    private static double NiceNumber(double value)
    {
        var exponent = Math.Floor(Math.Log10(value));
        var fraction = value / Math.Pow(10, exponent);
        var nice = fraction switch
        {
            <= 1 => 1d,
            <= 2 => 2d,
            <= 2.5 => 2.5d,
            <= 5 => 5d,
            _ => 10d
        };
        return nice * Math.Pow(10, exponent);
    }

    // CubicOut
    private static float Ease(float t) => 1f - ((1f - t) * (1f - t) * (1f - t));

    private static Color Lerp(Color from, Color to, float t) =>
        Color.FromRgba(
            from.Red + ((to.Red - from.Red) * t),
            from.Green + ((to.Green - from.Green) * t),
            from.Blue + ((to.Blue - from.Blue) * t),
            1f);
}
