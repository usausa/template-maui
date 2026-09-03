namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.Sample.Graph;

public sealed class GraphRowSurface : SKCanvasView
{
    private const float DefaultRowHeight = 26f;
    private const float MarginLeft = 8f;
    private const float MarginRight = 8f;

    private static readonly SKColor[] LaneColors =
    [
        new SKColor(0x1F, 0x77, 0xB4),
        new SKColor(0xFF, 0x7F, 0x0E),
        new SKColor(0x2C, 0xA0, 0x2C),
        new SKColor(0xD6, 0x27, 0x28),
        new SKColor(0x94, 0x67, 0xBD),
        new SKColor(0x8C, 0x56, 0x4B),
        new SKColor(0xE3, 0x77, 0xC2),
        new SKColor(0x7F, 0x7F, 0x7F),
        new SKColor(0xBC, 0xBD, 0x22),
        new SKColor(0x17, 0xBE, 0xCF),
    ];

    public static readonly BindableProperty RowDataProperty = BindableProperty.Create(
        nameof(RowData),
        typeof(GraphRow),
        typeof(GraphRowSurface),
        defaultValue: null,
        propertyChanged: OnRowDataChanged);

    public GraphRow? RowData
    {
        get => (GraphRow?)GetValue(RowDataProperty);
        set => SetValue(RowDataProperty, value);
    }

    public static readonly BindableProperty LineWidthProperty = BindableProperty.Create(
        nameof(LineWidth),
        typeof(float),
        typeof(GraphRowSurface),
        2f,
        propertyChanged: OnVisualPropertyChanged);

    public float LineWidth
    {
        get => (float)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    public static readonly BindableProperty NodeRadiusProperty = BindableProperty.Create(
        nameof(NodeRadius),
        typeof(float),
        typeof(GraphRowSurface),
        5f,
        propertyChanged: OnVisualPropertyChanged);

    public float NodeRadius
    {
        get => (float)GetValue(NodeRadiusProperty);
        set => SetValue(NodeRadiusProperty, value);
    }

    public static readonly BindableProperty LaneWidthProperty = BindableProperty.Create(
        nameof(LaneWidth),
        typeof(float),
        typeof(GraphRowSurface),
        18f,
        propertyChanged: OnLaneWidthChanged);

    public float LaneWidth
    {
        get => (float)GetValue(LaneWidthProperty);
        set => SetValue(LaneWidthProperty, value);
    }

    // ノード中心の Y 位置(0 以下で行の中央)。行を展開して高さが変わる場合に固定位置を指定する
    public static readonly BindableProperty NodeCenterYProperty = BindableProperty.Create(
        nameof(NodeCenterY),
        typeof(float),
        typeof(GraphRowSurface),
        0f,
        propertyChanged: OnVisualPropertyChanged);

    public float NodeCenterY
    {
        get => (float)GetValue(NodeCenterYProperty);
        set => SetValue(NodeCenterYProperty, value);
    }

    // 分岐・合流を曲線で描く
    public static readonly BindableProperty UseCurvesProperty = BindableProperty.Create(
        nameof(UseCurves),
        typeof(bool),
        typeof(GraphRowSurface),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCurves
    {
        get => (bool)GetValue(UseCurvesProperty);
        set => SetValue(UseCurvesProperty, value);
    }

    public GraphRowSurface()
    {
        // 高さは親レイアウト(行)に従う。UIGraph は行 Grid の HeightRequest=26 で決まる
        IgnorePixelScaling = false;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (RowData is null)
        {
            return;
        }

        canvas.Save();
        var scaleX = (float)(e.Info.Width / Width);
        var scaleY = (float)(e.Info.Height / Height);
        if (Single.IsFinite(scaleX) && Single.IsFinite(scaleY) && (scaleX > 0) && (scaleY > 0))
        {
            canvas.Scale(scaleX, scaleY);
        }
        Render(canvas, RowData);
        canvas.Restore();
    }

    private static void OnRowDataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var element = (GraphRowSurface)bindable;
        if (newValue is GraphRow row)
        {
            element.WidthRequest = element.GetCellWidth(row.LaneCount);
        }
        element.InvalidateSurface();
    }

    private static void OnLaneWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var element = (GraphRowSurface)bindable;
        if (element.RowData is { } row)
        {
            element.WidthRequest = element.GetCellWidth(row.LaneCount);
        }
        element.InvalidateSurface();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((GraphRowSurface)bindable).InvalidateSurface();
    }

    private float GetCellWidth(int laneCount) =>
        MarginLeft + (LaneWidth * System.Math.Max(laneCount, 1)) + MarginRight;

    private void Render(SKCanvas canvas, GraphRow row)
    {
        canvas.Clear(SKColors.Transparent);

        var rowHeight = (Height > 0) && Double.IsFinite(Height) ? (float)Height : DefaultRowHeight;
        var nodeCenterY = NodeCenterY > 0f ? NodeCenterY : rowHeight / 2f;
        var lineWidth = LineWidth;

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = lineWidth,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
        };

        foreach (var seg in row.Segments)
        {
            strokePaint.Color = LaneColors[seg.ColorIndex % LaneColors.Length];
            DrawSegment(canvas, seg, strokePaint, rowHeight, nodeCenterY);
        }

        var nodeCenterX = LaneCenterX(row.Lane);
        var nodeColor = LaneColors[row.Lane % LaneColors.Length];

        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = nodeColor,
            IsAntialias = true,
        };
        using var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = SKColors.White,
            IsAntialias = true,
        };

        canvas.DrawCircle(nodeCenterX, nodeCenterY, NodeRadius, fillPaint);
        canvas.DrawCircle(nodeCenterX, nodeCenterY, NodeRadius, borderPaint);
    }

    private void DrawSegment(SKCanvas canvas, GraphSegment seg, SKPaint paint, float rowHeight, float nodeCenterY)
    {
        var laneX = LaneCenterX(seg.Lane);
        var radius = paint.StrokeWidth / 2f;
        switch (seg.Kind)
        {
            case GraphSegmentKind.Vertical:
                canvas.DrawLine(laneX, 0f, laneX, rowHeight, paint);
                break;
            case GraphSegmentKind.HalfVerticalTop:
                canvas.DrawLine(laneX, 0f, laneX, nodeCenterY, paint);
                break;
            case GraphSegmentKind.HalfVerticalBottom:
                canvas.DrawLine(laneX, nodeCenterY, laneX, rowHeight, paint);
                break;
            case GraphSegmentKind.Diagonal:
                DrawConnection(canvas, paint, laneX, nodeCenterY, LaneCenterX(seg.ToLane), rowHeight - radius);
                break;
            case GraphSegmentKind.DiagonalBranch:
                DrawConnection(canvas, paint, laneX, radius, LaneCenterX(seg.ToLane), nodeCenterY);
                break;
            default:
                break;
        }
    }

    private void DrawConnection(SKCanvas canvas, SKPaint paint, float x1, float y1, float x2, float y2)
    {
        if (!UseCurves)
        {
            canvas.DrawLine(x1, y1, x2, y2, paint);
            return;
        }

        // S 字カーブで滑らかに接続する
        var midY = (y1 + y2) / 2f;
        using var builder = new SKPathBuilder();
        builder.MoveTo(x1, y1);
        builder.CubicTo(x1, midY, x2, midY, x2, y2);
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }

    private float LaneCenterX(int lane) => MarginLeft + (lane * LaneWidth) + (LaneWidth / 2f);
}
