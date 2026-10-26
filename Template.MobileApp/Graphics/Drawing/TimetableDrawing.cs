namespace Template.MobileApp.Graphics.Drawing;

using Font = Microsoft.Maui.Graphics.Font;

public sealed class TimetableDrawing : DrawingObject, ISizedDrawing
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const int Columns = 6;

    private const float HourWidth = 52f;

    private const float LineHeight = 42f;

    private const float RowPadding = 5f;

    private const float CellInset = 20f;

    private const float MinuteWidth = 22f;

    private const float MarkSpacing = 2f;

    private const float MinuteBaseline = 28f;

    private const float HourCapRate = 0.36f;

    private const float HourFontSize = 22f;

    private const float MinuteFontSize = 19f;

    private const float MarkFontSize = 11f;

    private static readonly Color BackColor = Colors.White;

    private static readonly Color LineColor = Color.FromArgb("#E0E0E0");

    private static readonly Color HourBackColor = Color.FromArgb("#E3F2FD");

    private static readonly Color HourTextColor = Color.FromArgb("#1976D2");

    private static readonly Color CurrentColor = Color.FromArgb("#D32F2F");

    private static readonly Color CurrentBackColor = Color.FromArgb("#FFEBEE");

    private static readonly Color LocalColor = Color.FromArgb("#212121");

    private static readonly Color RapidColor = Color.FromArgb("#1E88E5");

    private static readonly Color SpecialColor = Color.FromArgb("#F57C00");

    private static readonly Color ExpressColor = Color.FromArgb("#E53935");

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private readonly List<float> offsets = [];

    private StationTimetable? timetable;

    private int currentHour = -1;

    public double ContentHeight { get; private set; }

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(StationTimetable? value, TimeOnly now)
    {
        timetable = value;
        currentHour = now.Hour;

        offsets.Clear();
        var y = 0f;
        if (value is not null)
        {
            foreach (var hour in value.Hours)
            {
                offsets.Add(y);
                y += HeightOf(hour);
            }
        }
        offsets.Add(y);
        ContentHeight = y;

        Invalidate();
    }

    public double OffsetOf(int hour)
    {
        if (timetable is null)
        {
            return 0;
        }

        var index = 0;
        while ((index < timetable.Hours.Count) && (timetable.Hours[index].Hour < hour))
        {
            index++;
        }

        return offsets[index];
    }

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = BackColor;
        canvas.FillRectangle(dirtyRect);

        if (timetable is not null)
        {
            var cellWidth = (dirtyRect.Width - HourWidth - CellInset) / Columns;
            for (var i = 0; i < timetable.Hours.Count; i++)
            {
                var top = offsets[i];
                var bottom = offsets[i + 1];
                if ((bottom < dirtyRect.Top) || (top > dirtyRect.Bottom))
                {
                    continue;
                }

                DrawRow(canvas, dirtyRect, timetable.Hours[i], top, bottom - top, cellWidth);
            }
        }

        canvas.RestoreState();
    }

    private void DrawRow(ICanvas canvas, RectF dirtyRect, StationHour hour, float top, float height, float cellWidth)
    {
        var current = hour.Hour == currentHour;

        if (current)
        {
            canvas.FillColor = CurrentBackColor;
            canvas.FillRectangle(dirtyRect.Left, top, dirtyRect.Width, height);
        }

        canvas.FillColor = current ? CurrentColor : HourBackColor;
        canvas.FillRectangle(dirtyRect.Left, top, HourWidth, height);

        canvas.Font = Font.DefaultBold;
        canvas.FontSize = HourFontSize;
        canvas.FontColor = current ? Colors.White : HourTextColor;
        canvas.DrawString(
            hour.Hour.ToString(CultureInfo.InvariantCulture),
            dirtyRect.Left + (HourWidth / 2f),
            top + (height / 2f) + (HourFontSize * HourCapRate),
            HorizontalAlignment.Center);

        canvas.FontSize = MinuteFontSize;
        for (var i = 0; i < hour.Departures.Count; i++)
        {
            var departure = hour.Departures[i];
            canvas.FontColor = ColorOf(departure.Kind);
            canvas.DrawString(
                departure.Minute.ToString("D2", CultureInfo.InvariantCulture),
                LeftOf(dirtyRect, i, cellWidth) + MinuteWidth,
                BaselineOf(top, i),
                HorizontalAlignment.Right);
        }

        canvas.Font = Font.Default;
        canvas.FontSize = MarkFontSize;
        for (var i = 0; i < hour.Departures.Count; i++)
        {
            var departure = hour.Departures[i];
            canvas.FontColor = ColorOf(departure.Kind);
            canvas.DrawString(
                MarkOf(departure.Kind),
                LeftOf(dirtyRect, i, cellWidth) + MinuteWidth + MarkSpacing,
                BaselineOf(top, i),
                HorizontalAlignment.Left);
        }

        canvas.StrokeColor = LineColor;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(dirtyRect.Left, top + height - 0.5f, dirtyRect.Right, top + height - 0.5f);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static float LeftOf(RectF dirtyRect, int index, float cellWidth) =>
        dirtyRect.Left + HourWidth + CellInset + ((index % Columns) * cellWidth);

    private static float BaselineOf(float top, int index)
    {
        var line = index / Columns;
        return top + RowPadding + (line * LineHeight) + MinuteBaseline;
    }

    private static float HeightOf(StationHour hour)
    {
        var lines = Math.Max(1, (hour.Departures.Count + Columns - 1) / Columns);
        return (lines * LineHeight) + (RowPadding * 2);
    }

    private static Color ColorOf(TrainKind kind) => kind switch
    {
        TrainKind.Rapid => RapidColor,
        TrainKind.SpecialRapid => SpecialColor,
        TrainKind.LimitedExpress => ExpressColor,
        _ => LocalColor
    };

    private static string MarkOf(TrainKind kind) => kind switch
    {
        TrainKind.Rapid => "快",
        TrainKind.SpecialRapid => "新",
        TrainKind.LimitedExpress => "特",
        _ => "普"
    };
}
