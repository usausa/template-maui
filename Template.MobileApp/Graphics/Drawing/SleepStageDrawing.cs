namespace Template.MobileApp.Graphics.Drawing;

using Font = Microsoft.Maui.Graphics.Font;

public sealed class SleepStageDrawing : DrawingObject
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const int LaneCount = 4;

    private const float AxisHeight = 18f;

    private const float LaneGapRate = 0.26f;

    private const float BarRadius = 3f;

    private const float LinkWidth = 1.5f;

    private const float AxisFontSize = 10f;

    private const int AxisStepMinutes = 60;

    private static readonly Color AxisColor = Color.FromArgb("#9E9E9E");

    private static readonly Color GridColor = Color.FromArgb("#EEEEEE");

    private static readonly Color LinkColor = Color.FromArgb("#CFD8DC");

    // Ordered from the top lane down, matching the SleepStage values
    private static readonly Color[] StageColors =
    [
        Color.FromArgb("#FB8C00"),
        Color.FromArgb("#AB47BC"),
        Color.FromArgb("#4FC3F7"),
        Color.FromArgb("#1565C0")
    ];

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private IReadOnlyList<SleepSpan> spans = [];

    private TimeOnly bedTime;

    private int total;

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(SleepNight? night)
    {
        spans = night?.Spans ?? [];
        bedTime = night?.BedTime ?? default;
        total = night?.TotalMinutes ?? 0;
        Invalidate();
    }

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        if ((spans.Count == 0) || (total <= 0))
        {
            return;
        }

        canvas.SaveState();

        var area = new RectF(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height - AxisHeight);
        var laneHeight = area.Height / LaneCount;
        var barHeight = laneHeight * (1f - LaneGapRate);

        DrawLanes(canvas, area, laneHeight);
        DrawLinks(canvas, area, laneHeight);
        DrawBars(canvas, area, laneHeight, barHeight);
        DrawAxis(canvas, dirtyRect, area);

        canvas.RestoreState();
    }

    private static void DrawLanes(ICanvas canvas, RectF area, float laneHeight)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        for (var i = 1; i < LaneCount; i++)
        {
            var y = area.Top + (i * laneHeight);
            canvas.DrawLine(area.Left, y, area.Right, y);
        }
    }

    // The thin riser between two stages, so the band reads as one continuous night
    private void DrawLinks(ICanvas canvas, RectF area, float laneHeight)
    {
        canvas.StrokeColor = LinkColor;
        canvas.StrokeSize = LinkWidth;

        for (var i = 1; i < spans.Count; i++)
        {
            var x = area.Left + (area.Width * spans[i].Offset / total);
            var from = CenterOf(area, laneHeight, spans[i - 1].Stage);
            var to = CenterOf(area, laneHeight, spans[i].Stage);
            canvas.DrawLine(x, from, x, to);
        }
    }

    private void DrawBars(ICanvas canvas, RectF area, float laneHeight, float barHeight)
    {
        foreach (var span in spans)
        {
            var x = area.Left + (area.Width * span.Offset / total);
            var width = Math.Max(2f, area.Width * span.Minutes / total);
            var y = CenterOf(area, laneHeight, span.Stage) - (barHeight / 2f);
            canvas.FillColor = StageColors[(int)span.Stage];
            canvas.FillRoundedRectangle(x, y, width, barHeight, BarRadius);
        }
    }

    private void DrawAxis(ICanvas canvas, RectF dirtyRect, RectF area)
    {
        canvas.Font = Font.Default;
        canvas.FontSize = AxisFontSize;
        canvas.FontColor = AxisColor;

        // Ticks land on the hour, the first one is the first full hour after lights out
        var first = (AxisStepMinutes - (bedTime.Minute % AxisStepMinutes)) % AxisStepMinutes;
        for (var minutes = first; minutes <= total; minutes += AxisStepMinutes)
        {
            var x = area.Left + (area.Width * minutes / total);
            canvas.DrawString(
                bedTime.AddMinutes(minutes).ToString("HH", CultureInfo.InvariantCulture),
                x - 12f,
                dirtyRect.Bottom - AxisHeight,
                24f,
                AxisHeight,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    private static float CenterOf(RectF area, float laneHeight, SleepStage stage) =>
        area.Top + ((int)stage * laneHeight) + (laneHeight / 2f);
}
