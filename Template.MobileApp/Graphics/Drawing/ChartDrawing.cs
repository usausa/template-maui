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

public readonly record struct CandlePoint(
    double Open,
    double High,
    double Low,
    double Close,
    double Volume = 0);

public readonly record struct ChartScale(
    double Min,
    double Max,
    double Step);

public sealed class ChartDrawing : DrawingObject, ITapDrawing, IDisposable
{
    private const float AnimationDuration = 600f;

    private const float Padding = 12f;

    private const float AxisWidth = 40f;

    private const float LabelHeight = 22f;

    private const float LegendHeight = 24f;

    private const float LegendRowHeight = 22f;

    private const float FontSize = 11f;

    private const float TooltipFontSize = 12f;

    private const float TooltipLineHeight = 16f;

    private const float PointExtent = 6f;

    private const float DimAlpha = 0.4f;

    private const float HitDistance = 24f;

    private const float DonutThickness = 32f;

    private const float MarkerGap = 16f;

    private static readonly ChartScale DefaultScale = new(0, 1, 0.25);

    private static readonly Color TooltipColor = Color.FromArgb("#37474F");
    private static readonly Color MarkerTextColor = Color.FromArgb("#212121");
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

    private RectF lastRect;

    private RectF plotArea;

    private PointF donutCenter;

    private float donutRadius;

    public Color Background { get; set; } = Colors.White;

    public Color GridColor { get; set; } = Color.FromArgb("#E0E0E0");

    public Color TextColor { get; set; } = Color.FromArgb("#757575");

    public Color StrongTextColor { get; set; } = Color.FromArgb("#424242");

    public Color LineColor { get; set; } = Color.FromArgb("#2196F3");

    public Color LineHighColor { get; set; } = Color.FromArgb("#E53935");

    public Color BarColor { get; set; } = Color.FromArgb("#64B5F6");

    public Color BarEndColor { get; set; } = Color.FromArgb("#1E88E5");

    public Color UpColor { get; set; } = Color.FromArgb("#43A047");

    public Color DownColor { get; set; } = Color.FromArgb("#E53935");

    public Color MarkerColor { get; set; } = Color.FromArgb("#FFC107");

    public Color TrackColor { get; set; } = Color.FromArgb("#EEF1F5");

    public bool ShowTrack { get; set; } = true;

    public bool ShowAxis { get; set; } = true;

    // Ratio of the plot taken by the volume bars below the candles, 0 hides them
    public float VolumeRatio { get; set; }

    // Horizontal guide drawn over the candles, used for the last traded price
    public double? MarkerValue { get; set; }

    public string AxisFormat { get; set; } = "{0:N0}";

    public string ValueFormat { get; set; } = "{0:N0}";

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

    public void ShowDonut(IReadOnlyList<double> data, IReadOnlyList<string>? names = null) =>
        ShowValues(ChartKind.Donut, data, names);

    public void ShowCandle(IReadOnlyList<CandlePoint> data, IReadOnlyList<string>? names = null)
    {
        candles = [.. data];
        UpdateCandleScale();
        Start(ChartKind.Candle, names, false);
    }

    public void UpdateCandle(CandlePoint value)
    {
        if (kind != ChartKind.Candle)
        {
            return;
        }

        if (candles.Length > 0)
        {
            candles[^1] = value;
        }
        else
        {
            candles = [value];
        }

        UpdateCandleScale();
        SafeInvalidate();
    }

    public void AddCandle(CandlePoint value)
    {
        if (kind != ChartKind.Candle)
        {
            return;
        }

        candles = [.. candles, value];
        UpdateCandleScale();
        SafeInvalidate();
    }

    private void UpdateCandleScale() =>
        scale = candles.Length > 0 ? NiceScale(candles.Min(static x => x.Low), candles.Max(static x => x.High), false) : DefaultScale;

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

        Color ColorOf(double value) => GraphicsHelper.Lerp(LineColor, LineHighColor, (float)((value - min) / range));

        canvas.SaveState();
        if (!morph)
        {
            canvas.ClipRectangle(area.Left - PointExtent, rect.Top, (area.Width * eased) + (PointExtent * 2), rect.Height);
        }

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
                var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;
                var paint = new LinearGradientPaint(
                    [
                        new PaintGradientStop(0f, BarColor.WithAlpha(alpha)),
                        new PaintGradientStop(1f, BarEndColor.WithAlpha(alpha))
                    ],
                    startPoint: new Point(0, 0),
                    endPoint: new Point(0, 1));
                var bar = new RectF(x, top, barWidth, height);

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

        var legendRows = labels.Length > 0 ? (values.Length + 1) / 2 : 0;
        var legendHeight = legendRows > 0 ? (legendRows * LegendRowHeight) + Padding : 0f;
        var chart = new RectF(rect.Left, rect.Top, rect.Width, rect.Height - legendHeight);
        var cx = chart.Center.X;
        var cy = chart.Center.Y;
        var radius = Math.Max(DonutThickness, (Math.Min(chart.Width, chart.Height) / 2f) - Padding - 12f);
        donutCenter = new PointF(cx, cy);
        donutRadius = radius;
        var arc = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);

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

        var inner = radius - (DonutThickness / 2f);
        if (IsSelected(values.Length))
        {
            DrawCenterText(canvas, cx, cy, inner, LabelOf(SelectedIndex), Format(ValueFormat, values[SelectedIndex]), (values[SelectedIndex] / total).ToString("P0", CultureInfo.CurrentCulture));
        }
        else
        {
            DrawCenterText(canvas, cx, cy, inner, string.Empty, Format(ValueFormat, total * eased), "TOTAL");
        }

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

    private void DrawCenterText(ICanvas canvas, float cx, float cy, float inner, string caption, string value, string footer)
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
        var volumeHeight = area.Height * Math.Clamp(VolumeRatio, 0f, 0.5f);
        var priceArea = new RectF(area.X, area.Y, area.Width, area.Height - volumeHeight);
        if (ShowAxis)
        {
            DrawValueAxis(canvas, priceArea, scale);
        }
        DrawLabels(canvas, area, candles.Length, true);

        if (candles.Length == 0)
        {
            return;
        }

        var step = area.Width / candles.Length;
        var bodyWidth = step * 0.55f;
        var volumeMax = volumeHeight > 0f ? candles.Max(static x => x.Volume) : 0d;

        var visible = (int)Math.Ceiling(candles.Length * eased);
        for (var i = 0; i < visible; i++)
        {
            var candle = candles[i];
            var cx = area.Left + (step * i) + (step / 2f);
            var alpha = (SelectedIndex < 0) || (i == SelectedIndex) ? 1f : DimAlpha;
            var color = (candle.Close >= candle.Open ? UpColor : DownColor).WithAlpha(alpha);

            canvas.StrokeColor = color;
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(cx, ToY(priceArea, scale, candle.High), cx, ToY(priceArea, scale, candle.Low));

            var top = ToY(priceArea, scale, Math.Max(candle.Open, candle.Close));
            var bottom = ToY(priceArea, scale, Math.Min(candle.Open, candle.Close));
            canvas.FillColor = color;
            canvas.FillRoundedRectangle(cx - (bodyWidth / 2f), top, bodyWidth, Math.Max(2f, bottom - top), 2f);

            if (volumeMax > 0)
            {
                var height = (float)(candle.Volume / volumeMax) * (volumeHeight - 4f);
                canvas.FillColor = color.WithAlpha(alpha * 0.6f);
                canvas.FillRectangle(cx - (bodyWidth / 2f), area.Bottom - height, bodyWidth, height);
            }
        }

        if (MarkerValue.HasValue)
        {
            DrawMarker(canvas, priceArea, MarkerValue.Value);
        }

        if (IsSelected(candles.Length))
        {
            var candle = candles[SelectedIndex];
            var x = area.Left + (step * (SelectedIndex + 0.5f));
            DrawTooltip(
                canvas,
                new PointF(x, ToY(priceArea, scale, candle.High) - 2f),
                [
                    LabelOf(SelectedIndex),
                    $"始 {Format(ValueFormat, candle.Open)}  終 {Format(ValueFormat, candle.Close)}",
                    $"高 {Format(ValueFormat, candle.High)}  安 {Format(ValueFormat, candle.Low)}"
                ]);
        }
    }

    private void DrawMarker(ICanvas canvas, RectF area, double value)
    {
        var y = ToY(area, scale, value);

        canvas.StrokeColor = MarkerColor;
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = [4f, 3f];
        canvas.DrawLine(area.Left, y, area.Right, y);
        canvas.StrokeDashPattern = null;

        if (!ShowAxis)
        {
            return;
        }

        canvas.FillColor = MarkerColor;
        canvas.FillRoundedRectangle(area.Left - AxisWidth - 6f, y - 8f, AxisWidth, 16f, 3f);
        canvas.Font = Font.Default;
        canvas.FontSize = FontSize;
        canvas.FontColor = MarkerTextColor;
        canvas.DrawString(Format(AxisFormat, value), area.Left - AxisWidth - 6f, y - 8f, AxisWidth, 16f, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    //--------------------------------------------------------------------------------
    // Stacked / Scatter / Heat
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
        new(area.Left + (float)((points[index].X - xScale.Min) / (xScale.Max - xScale.Min) * area.Width), ToY(area, scale, points[index].Y));

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
            var alpha = Ease(ElementProgress(t, r, rows));
            if (alpha <= 0f)
            {
                continue;
            }

            for (var c = 0; c < cols; c++)
            {
                var ratio = (float)((heatCells[r][c] - min) / range);
                var color = ratio < 0.5f
                    ? GraphicsHelper.Lerp(HeatLowColor, HeatMidColor, ratio * 2f)
                    : GraphicsHelper.Lerp(HeatMidColor, HeatHighColor, (ratio - 0.5f) * 2f);
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

    private static float ElementProgress(float t, int index, int count)
    {
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

    private void DrawValueAxis(ICanvas canvas, RectF area, ChartScale current)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.Font = Font.Default;
        canvas.FontSize = FontSize;
        canvas.FontColor = TextColor;

        // The marker badge takes the place of the axis label it would sit on
        var markerY = MarkerValue.HasValue ? ToY(area, current, MarkerValue.Value) : float.MinValue;

        var first = Math.Ceiling((current.Min / current.Step) - 1e-6) * current.Step;
        var count = (int)Math.Floor(((current.Max - first) / current.Step) + 1e-6);
        for (var i = 0; i <= count; i++)
        {
            var value = first + (i * current.Step);
            var y = ToY(area, current, value);
            canvas.DrawLine(area.Left, y, area.Right, y);
            if (Math.Abs(y - markerY) >= MarkerGap)
            {
                canvas.DrawString(Format(AxisFormat, value), area.Left - AxisWidth - 6f, y - 8f, AxisWidth, 16f, HorizontalAlignment.Right, VerticalAlignment.Center);
            }
        }
    }

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

        var angle = GraphicsHelper.NormalizeDegrees(MathF.Atan2(dx, -dy) * 180f / MathF.PI);
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
}
