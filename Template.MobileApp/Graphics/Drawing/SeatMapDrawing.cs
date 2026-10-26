namespace Template.MobileApp.Graphics.Drawing;

using Font = Microsoft.Maui.Graphics.Font;

public sealed class SeatMapDrawing : DrawingObject, ITapDrawing, IZoomDrawing
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const float AisleRate = 0.6f;

    private const float LabelRate = 0.9f;

    private const float ScreenRate = 2.6f;

    private const float FooterRate = 1.0f;

    private const float SeatInsetRate = 0.1f;

    private const float SeatRadiusRate = 0.18f;

    private const float SeatStrokeRate = 0.07f;

    private const float SeatStrokeMinimum = 1.5f;

    private const float LabelFontRate = 0.42f;

    private const float MinimapRate = 0.22f;

    private const float MinimapMargin = 12f;

    private static readonly Color BackColor = Color.FromArgb("#1C2733");

    private static readonly Color ScreenColor = Color.FromArgb("#B0BEC5");

    private static readonly Color LabelColor = Color.FromArgb("#90A4AE");

    private static readonly Color AvailableColor = Color.FromArgb("#CFD8DC");

    private static readonly Color SoldColor = Color.FromArgb("#46596B");

    private static readonly Color SelectedColor = Color.FromArgb("#2196F3");

    private static readonly Color PremiumColor = Color.FromArgb("#FFC107");

    private static readonly Color WheelchairColor = Color.FromArgb("#009688");

    private static readonly Color MinimapBackColor = Color.FromArgb("#55000000");

    private static readonly Color MinimapSeatColor = Color.FromArgb("#78909C");

    private static readonly Color MinimapFrameColor = Color.FromArgb("#64B5F6");

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private SeatMap? map;

    private float zoomScale = 1f;

    private PointF zoomOffset;

    private RectF lastRect;

    public Action<SeatPlace>? SeatTapped { get; set; }

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(SeatMap? value)
    {
        map = value;
        Invalidate();
    }

    public void OnZoom(float scale, PointF offset)
    {
        zoomScale = scale;
        zoomOffset = offset;
        Invalidate();
    }

    public void OnTap(PointF point)
    {
        if ((map is null) || (SeatTapped is null))
        {
            return;
        }

        var layout = new SeatLayout(lastRect, map);
        var content = Invert(point, lastRect);
        if ((layout.Locate(content) is { } cell) && (map.Find(cell.Row, cell.Column) is { Kind: not SeatKind.None } place))
        {
            SeatTapped(place);
        }
    }

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        lastRect = dirtyRect;

        canvas.SaveState();
        canvas.FillColor = BackColor;
        canvas.FillRectangle(dirtyRect);

        if (map is not null)
        {
            var layout = new SeatLayout(dirtyRect, map);
            var shift = ClampOffset(dirtyRect);

            canvas.SaveState();
            canvas.Translate(dirtyRect.Center.X + shift.X, dirtyRect.Center.Y + shift.Y);
            canvas.Scale(zoomScale, zoomScale);
            canvas.Translate(-dirtyRect.Center.X, -dirtyRect.Center.Y);

            DrawScreen(canvas, layout);
            DrawSeats(canvas, layout, map);
            DrawLabels(canvas, layout, map);

            canvas.RestoreState();

            if (zoomScale > 1f)
            {
                DrawMinimap(canvas, dirtyRect, layout, map);
            }
        }

        canvas.RestoreState();
    }

    private static void DrawScreen(ICanvas canvas, SeatLayout layout)
    {
        var rect = layout.Screen;
        using var path = new PathF();
        path.MoveTo(rect.Left, rect.Bottom);
        path.CurveTo(
            rect.Left + (rect.Width * 0.25f), rect.Top,
            rect.Left + (rect.Width * 0.75f), rect.Top,
            rect.Right, rect.Bottom);

        canvas.StrokeColor = ScreenColor;
        canvas.StrokeSize = Math.Max(2f, layout.CellSize * 0.1f);
        canvas.DrawPath(path);

        canvas.Font = Font.Default;
        canvas.FontSize = layout.CellSize * LabelFontRate;
        canvas.FontColor = LabelColor;
        canvas.DrawString("S C R E E N", rect.Left, rect.Bottom, rect.Width, layout.CellSize, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private static void DrawSeats(ICanvas canvas, SeatLayout layout, SeatMap value)
    {
        var inset = layout.CellSize * SeatInsetRate;
        var radius = layout.CellSize * SeatRadiusRate;
        var stroke = Math.Max(SeatStrokeMinimum, layout.CellSize * SeatStrokeRate);

        for (var row = 0; row < value.Rows; row++)
        {
            for (var column = 0; column < value.Columns; column++)
            {
                if (value.Find(row, column) is not { Kind: not SeatKind.None } place)
                {
                    continue;
                }

                var rect = layout.GetSeat(row, column);
                rect = new RectF(rect.X + inset, rect.Y + inset, rect.Width - (inset * 2), rect.Height - (inset * 2));

                switch (place.Status)
                {
                    case SeatStatus.Selected:
                        canvas.FillColor = SelectedColor;
                        canvas.FillRoundedRectangle(rect, radius);
                        DrawCheck(canvas, rect, stroke);
                        break;
                    case SeatStatus.Sold:
                        canvas.FillColor = SoldColor;
                        canvas.FillRoundedRectangle(rect, radius);
                        break;
                    default:
                        if (place.Kind == SeatKind.Wheelchair)
                        {
                            canvas.FillColor = WheelchairColor;
                            canvas.FillRoundedRectangle(rect, radius);
                        }
                        else
                        {
                            canvas.StrokeColor = place.Kind == SeatKind.Premium ? PremiumColor : AvailableColor;
                            canvas.StrokeSize = stroke;
                            canvas.DrawRoundedRectangle(rect, radius);
                        }

                        break;
                }
            }
        }
    }

    private static void DrawCheck(ICanvas canvas, RectF rect, float stroke)
    {
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(
            rect.Left + (rect.Width * 0.26f), rect.Top + (rect.Height * 0.52f),
            rect.Left + (rect.Width * 0.44f), rect.Top + (rect.Height * 0.70f));
        canvas.DrawLine(
            rect.Left + (rect.Width * 0.44f), rect.Top + (rect.Height * 0.70f),
            rect.Left + (rect.Width * 0.74f), rect.Top + (rect.Height * 0.32f));
    }

    private static void DrawLabels(ICanvas canvas, SeatLayout layout, SeatMap value)
    {
        canvas.Font = Font.Default;
        canvas.FontSize = layout.CellSize * LabelFontRate;
        canvas.FontColor = LabelColor;

        for (var row = 0; row < value.Rows; row++)
        {
            var text = ((char)('A' + row)).ToString();
            var y = layout.Grid.Top + (row * layout.CellSize);
            canvas.DrawString(text, layout.Grid.Left - layout.LabelWidth, y, layout.LabelWidth, layout.CellSize, HorizontalAlignment.Center, VerticalAlignment.Center);
            canvas.DrawString(text, layout.Grid.Right, y, layout.LabelWidth, layout.CellSize, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        for (var column = 0; column < value.Columns; column++)
        {
            var rect = layout.GetSeat(value.Rows - 1, column);
            canvas.DrawString(
                (column + 1).ToString(CultureInfo.InvariantCulture),
                rect.Left,
                layout.Grid.Bottom,
                layout.CellSize,
                layout.CellSize,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    private void DrawMinimap(ICanvas canvas, RectF dirtyRect, SeatLayout layout, SeatMap value)
    {
        var size = dirtyRect.Width * MinimapRate;
        var height = size * layout.Grid.Height / layout.Grid.Width;
        var frame = new RectF(dirtyRect.Right - size - MinimapMargin, dirtyRect.Bottom - height - MinimapMargin, size, height);

        canvas.FillColor = MinimapBackColor;
        canvas.FillRoundedRectangle(frame, 6f);

        var rowHeight = frame.Height / value.Rows;
        canvas.FillColor = MinimapSeatColor;
        for (var row = 0; row < value.Rows; row++)
        {
            canvas.FillRectangle(frame.Left + 2f, frame.Top + (row * rowHeight), frame.Width - 4f, Math.Max(1f, rowHeight - 1f));
        }

        // Visible area in content space, mapped onto the minimap
        var origin = Invert(new PointF(dirtyRect.Left, dirtyRect.Top), dirtyRect);
        var x = frame.Left + ((origin.X - layout.Grid.Left) / layout.Grid.Width * frame.Width);
        var y = frame.Top + ((origin.Y - layout.Grid.Top) / layout.Grid.Height * frame.Height);
        var w = dirtyRect.Width / zoomScale / layout.Grid.Width * frame.Width;
        var h = dirtyRect.Height / zoomScale / layout.Grid.Height * frame.Height;
        var left = Math.Clamp(x, frame.Left, frame.Right);
        var top = Math.Clamp(y, frame.Top, frame.Bottom);

        canvas.StrokeColor = MinimapFrameColor;
        canvas.StrokeSize = 2f;
        canvas.DrawRectangle(
            left,
            top,
            Math.Min(w, frame.Right - left),
            Math.Min(h, frame.Bottom - top));
    }

    //--------------------------------------------------------------------------------
    // Transform
    //--------------------------------------------------------------------------------

    private PointF ClampOffset(RectF rect)
    {
        var limitX = Math.Max(0f, (rect.Width * zoomScale) - rect.Width) / 2f;
        var limitY = Math.Max(0f, (rect.Height * zoomScale) - rect.Height) / 2f;
        return new PointF(Math.Clamp(zoomOffset.X, -limitX, limitX), Math.Clamp(zoomOffset.Y, -limitY, limitY));
    }

    private PointF Invert(PointF point, RectF rect)
    {
        var shift = ClampOffset(rect);
        var center = rect.Center;
        return new PointF(
            ((point.X - center.X - shift.X) / zoomScale) + center.X,
            ((point.Y - center.Y - shift.Y) / zoomScale) + center.Y);
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    private readonly struct SeatLayout
    {
        private readonly IReadOnlyList<int> aisles;

        private readonly int columns;

        public float CellSize { get; }

        public float LabelWidth { get; }

        public RectF Grid { get; }

        public RectF Screen { get; }

        public SeatLayout(RectF rect, SeatMap map)
        {
            aisles = map.Aisles;
            columns = map.Columns;

            var units = map.Columns + (map.Aisles.Count * AisleRate) + (LabelRate * 2);
            var lines = map.Rows + ScreenRate + FooterRate;
            CellSize = Math.Min(rect.Width / units, rect.Height / lines);
            LabelWidth = CellSize * LabelRate;

            var width = (map.Columns * CellSize) + (map.Aisles.Count * AisleRate * CellSize);
            var height = map.Rows * CellSize;
            var left = rect.Left + ((rect.Width - width) / 2f);
            var top = rect.Top + (ScreenRate * CellSize) + ((rect.Height - (lines * CellSize)) / 2f);
            Grid = new RectF(left, top, width, height);
            Screen = new RectF(left, rect.Top + (CellSize * 0.4f), width, ScreenRate * CellSize * 0.5f);
        }

        public RectF GetSeat(int row, int column) =>
            new(Grid.Left + OffsetOf(column), Grid.Top + (row * CellSize), CellSize, CellSize);

        public (int Row, int Column)? Locate(PointF point)
        {
            if (!Grid.Contains(point))
            {
                return null;
            }

            var row = (int)((point.Y - Grid.Top) / CellSize);
            for (var column = 0; column < columns; column++)
            {
                var x = Grid.Left + OffsetOf(column);
                if ((point.X >= x) && (point.X < x + CellSize))
                {
                    return (row, column);
                }
            }

            return null;
        }

        private float OffsetOf(int column)
        {
            var passed = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < aisles.Count; i++)
            {
                if (column >= aisles[i])
                {
                    passed++;
                }
            }

            return (column * CellSize) + (passed * AisleRate * CellSize);
        }
    }
}
