namespace Template.MobileApp.Modules.UI;

using ClamChart;

using ClamGrid;

using SkiaSharp.Views.Maui;

using Template.MobileApp.Models.Sample;

internal static class UIGridStyles
{
    private const float TrendInsetX = 6;

    private const float TrendInsetY = 10;

    private const float TrendThickness = 1.5f;

    private const float TrendAreaOpacity = 0.25f;

    private static readonly GridColors OpenColors = new(Color.FromArgb("#616161"), Color.FromArgb("#F5F5F5"));

    private static readonly GridColors ProcessingColors = new(Color.FromArgb("#1565C0"), Color.FromArgb("#E3F2FD"));

    private static readonly GridColors HoldColors = new(Color.FromArgb("#EF6C00"), Color.FromArgb("#FFF3E0"));

    private static readonly GridColors CompletedColors = new(Color.FromArgb("#2E7D32"), Color.FromArgb("#E8F5E9"));

    private static readonly Color OverdueText = Color.FromArgb("#C62828");

    private static readonly Color OverdueBackground = Color.FromArgb("#FFEBEE");

    private static readonly Color DueSoonText = Color.FromArgb("#EF6C00");

    private static readonly Color DueSoonBackground = Color.FromArgb("#FFF3E0");

    private static readonly Color TopRankBackground = Color.FromArgb("#FFF8E1");

    private static readonly Color LargeAmountText = Color.FromArgb("#2E7D32");

    private static readonly Color BulkText = Color.FromArgb("#1565C0");

    private static readonly Color StoreText = Color.FromArgb("#6A1B9A");

    private static readonly Color WebText = Color.FromArgb("#1565C0");

    private static readonly Color PhoneText = Color.FromArgb("#2E7D32");

    private static readonly Color FaxText = Color.FromArgb("#6D4C41");

    private static readonly SKColor TrendColor = new(0x15, 0x65, 0xC0);

    private static readonly ChartDrawingCache TrendCache = new();

    public static Func<GridCellColorContext, GridColors> CellColors { get; } = SelectCellColors;

    public static Action<SKCanvas, GridCellPaintContext> TrendPainter { get; } = PaintTrend;

    private static GridColors SelectCellColors(GridCellColorContext context)
    {
        if (context.IsSelected || context.Item is not OrderInfo order)
        {
            return context.DefaultColors;
        }

        return context.Column.Key switch
        {
            nameof(OrderInfo.Status) => order.Status switch
            {
                OrderStatus.Open => OpenColors,
                OrderStatus.Processing => ProcessingColors,
                OrderStatus.Hold => HoldColors,
                _ => CompletedColors
            },
            nameof(OrderInfo.Marks) when order.IsOverdue => context.DefaultColors with { Background = OverdueBackground },
            nameof(OrderInfo.Marks) when order.IsDueSoon => context.DefaultColors with { Background = DueSoonBackground },
            nameof(OrderInfo.Rank) when order.Rank >= 3 => context.DefaultColors with { Background = TopRankBackground },
            nameof(OrderInfo.Amount) when order.IsLargeAmount => context.DefaultColors with { TextColor = LargeAmountText },
            nameof(OrderInfo.Quantity) when order.IsBulk => context.DefaultColors with { TextColor = BulkText },
            nameof(OrderInfo.DueDate) when order.IsOverdue => context.DefaultColors with { TextColor = OverdueText },
            nameof(OrderInfo.DueDate) when order.IsDueSoon => context.DefaultColors with { TextColor = DueSoonText },
            nameof(OrderInfo.Channel) => context.DefaultColors with
            {
                TextColor = order.Channel switch
                {
                    OrderChannel.Store => StoreText,
                    OrderChannel.Web => WebText,
                    OrderChannel.Phone => PhoneText,
                    _ => FaxText
                }
            },
            _ => context.DefaultColors
        };
    }

    private static void PaintTrend(SKCanvas canvas, GridCellPaintContext context)
    {
        if (context.Value is not double[] values)
        {
            return;
        }

        var options = new SparklineDrawOptions
        {
            Kind = ChartSparklineKind.Area,
            Color = (context.IsSelected ? context.Colors.TextColor?.ToSKColor() : null) ?? TrendColor,
            Thickness = TrendThickness,
            AreaOpacity = TrendAreaOpacity
        };
        ChartDrawing.DrawSparkline(canvas, SKRect.Inflate(context.Bounds, -TrendInsetX, -TrendInsetY), values, options, TrendCache);
    }
}
