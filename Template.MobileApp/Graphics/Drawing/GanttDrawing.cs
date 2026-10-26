namespace Template.MobileApp.Graphics.Drawing;

using Font = Microsoft.Maui.Graphics.Font;

public enum GanttPart
{
    Header,
    Chart
}

// One class draws both bands so the date header and the bars share the same day metrics
public sealed class GanttDrawing : DrawingObject
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const float BarInsetRate = 0.26f;

    private const float BarRadius = 4f;

    private const float RemainAlpha = 0.28f;

    private const float LabelFontSize = 11f;

    private const float HeaderDayFontSize = 14f;

    private const float HeaderWeekFontSize = 10f;

    private const float MinimumLabelWidth = 34f;

    private const float MinimumHeaderWidth = 26f;

    private const float MinimumWeekdayWidth = 20f;

    private const float LinkInset = 8f;

    private const float ArrowSize = 4f;

    private const float MilestoneRate = 0.34f;

    private static readonly Color BackColor = Colors.White;

    private static readonly Color StripeColor = Color.FromArgb("#FAFAFA");

    private static readonly Color GridColor = Color.FromArgb("#EEEEEE");

    private static readonly Color HolidayColor = Color.FromArgb("#F2F4F6");

    private static readonly Color TodayColor = Color.FromArgb("#F50057");

    private static readonly Color TextColor = Color.FromArgb("#616161");

    private static readonly Color SaturdayColor = Color.FromArgb("#1E88E5");

    private static readonly Color SundayColor = Color.FromArgb("#E53935");

    private static readonly Color LinkColor = Color.FromArgb("#B0BEC5");

    private static readonly Color BarTextColor = Color.FromArgb("#424242");

    private static readonly Color WaitingColor = Color.FromArgb("#9E9E9E");

    public Color CompletedColor { get; set; } = Color.FromArgb("#43A047");

    public Color RunningColor { get; set; } = Color.FromArgb("#1E88E5");

    public Color DelayedColor { get; set; } = Color.FromArgb("#FB8C00");

    public Color MilestoneColor { get; set; } = Color.FromArgb("#8E24AA");

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private ProjectSchedule? schedule;

    private float dayWidth = 28f;

    public double RowHeight { get; set; } = 56;

    public GanttPart Part { get; init; }

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(ProjectSchedule? value, double width)
    {
        schedule = value;
        dayWidth = (float)width;
        Invalidate();
    }

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = BackColor;
        canvas.FillRectangle(dirtyRect);

        if (schedule is not null)
        {
            if (Part == GanttPart.Header)
            {
                DrawHeader(canvas, dirtyRect, schedule);
            }
            else
            {
                DrawChart(canvas, dirtyRect, schedule);
            }
        }

        canvas.RestoreState();
    }

    //--------------------------------------------------------------------------------
    // Header
    //--------------------------------------------------------------------------------

    private void DrawHeader(ICanvas canvas, RectF dirtyRect, ProjectSchedule value)
    {
        canvas.Font = Font.DefaultBold;

        // A narrow day cannot hold a two digit label, so thin them out and drop the weekday row
        var step = Math.Max(1, (int)Math.Ceiling(MinimumHeaderWidth / dayWidth));
        var labelWidth = dayWidth * step;
        var weekday = dayWidth >= MinimumWeekdayWidth;

        // The bands go down first, a thinned label spans several days and would be painted over
        canvas.FillColor = HolidayColor;
        for (var i = 0; i < value.Days; i++)
        {
            if (IsHoliday(value.Start.AddDays(i)))
            {
                canvas.FillRectangle(i * dayWidth, dirtyRect.Top, dayWidth, dirtyRect.Height);
            }
        }

        for (var i = 0; i < value.Days; i += step)
        {
            var date = value.Start.AddDays(i);
            var x = i * dayWidth;
            if ((x > dirtyRect.Right) || (x + labelWidth < dirtyRect.Left))
            {
                continue;
            }

            var today = date == value.Today;
            canvas.FontColor = today ? TodayColor : ColorOf(date);
            canvas.FontSize = HeaderDayFontSize;
            canvas.DrawString(
                date.Day.ToString(CultureInfo.InvariantCulture),
                x,
                dirtyRect.Top + 2f,
                labelWidth,
                weekday ? dirtyRect.Height * 0.55f : dirtyRect.Height - 4f,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);

            if (weekday)
            {
                canvas.FontSize = HeaderWeekFontSize;
                canvas.DrawString(
                    WeekOf(date),
                    x,
                    dirtyRect.Top + (dirtyRect.Height * 0.52f),
                    labelWidth,
                    dirtyRect.Height * 0.42f,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center);
            }
        }

        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.DrawLine(dirtyRect.Left, dirtyRect.Bottom - 0.5f, dirtyRect.Right, dirtyRect.Bottom - 0.5f);
    }

    //--------------------------------------------------------------------------------
    // Chart
    //--------------------------------------------------------------------------------

    private void DrawChart(ICanvas canvas, RectF dirtyRect, ProjectSchedule value)
    {
        DrawBackground(canvas, dirtyRect, value);
        DrawLinks(canvas, value);
        DrawBars(canvas, value);
        DrawToday(canvas, dirtyRect, value);
    }

    private void DrawBackground(ICanvas canvas, RectF dirtyRect, ProjectSchedule value)
    {
        for (var i = 0; i < value.Tasks.Count; i++)
        {
            if ((i % 2) == 1)
            {
                canvas.FillColor = StripeColor;
                canvas.FillRectangle(dirtyRect.Left, i * (float)RowHeight, dirtyRect.Width, (float)RowHeight);
            }
        }

        for (var i = 0; i < value.Days; i++)
        {
            var x = i * dayWidth;
            if (IsHoliday(value.Start.AddDays(i)))
            {
                canvas.FillColor = HolidayColor;
                canvas.FillRectangle(x, dirtyRect.Top, dayWidth, dirtyRect.Height);
            }
        }

        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        for (var i = 0; i <= value.Tasks.Count; i++)
        {
            var y = (i * (float)RowHeight) - 0.5f;
            canvas.DrawLine(dirtyRect.Left, y, dirtyRect.Right, y);
        }
    }

    private void DrawBars(ICanvas canvas, ProjectSchedule value)
    {
        canvas.Font = Font.DefaultBold;
        canvas.FontSize = LabelFontSize;

        for (var i = 0; i < value.Tasks.Count; i++)
        {
            var task = value.Tasks[i];
            var rect = BarOf(value, task, i);

            if (task.State == ProjectTaskState.Milestone)
            {
                DrawMilestone(canvas, rect, ColorOf(task.State));
                continue;
            }

            canvas.FillColor = ColorOf(task.State).WithAlpha(RemainAlpha);
            canvas.FillRoundedRectangle(rect, BarRadius);

            var done = rect.Width * (float)task.Progress;
            if (done > 0)
            {
                canvas.FillColor = ColorOf(task.State);
                canvas.FillRoundedRectangle(new RectF(rect.X, rect.Y, done, rect.Height), BarRadius);
            }

            if (rect.Width >= MinimumLabelWidth)
            {
                var full = task.Progress >= 1;
                canvas.FontColor = full ? Colors.White : BarTextColor;
                canvas.DrawString(
                    task.Progress.ToString("P0", CultureInfo.InvariantCulture),
                    rect.X,
                    rect.Y,
                    rect.Width - 6f,
                    rect.Height,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }
        }
    }

    private static void DrawMilestone(ICanvas canvas, RectF rect, Color color)
    {
        var size = rect.Height * MilestoneRate * 2f;
        var cx = rect.Center.X;
        var cy = rect.Center.Y;

        using var path = new PathF();
        path.MoveTo(cx, cy - size);
        path.LineTo(cx + size, cy);
        path.LineTo(cx, cy + size);
        path.LineTo(cx - size, cy);
        path.Close();

        canvas.FillColor = color;
        canvas.FillPath(path);
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2f;
        canvas.DrawPath(path);
    }

    private void DrawLinks(ICanvas canvas, ProjectSchedule value)
    {
        canvas.StrokeColor = LinkColor;
        canvas.StrokeSize = 1.5f;

        for (var i = 0; i < value.Tasks.Count; i++)
        {
            var task = value.Tasks[i];
            if ((task.DependsOn < 0) || (task.DependsOn >= value.Tasks.Count))
            {
                continue;
            }

            var from = BarOf(value, value.Tasks[task.DependsOn], task.DependsOn);
            var to = BarOf(value, task, i);
            var startX = from.Right;
            var startY = from.Center.Y;
            var endX = to.Left;
            var endY = to.Center.Y;
            var turnX = Math.Min(startX + LinkInset, endX - LinkInset);

            canvas.DrawLine(startX, startY, turnX, startY);
            canvas.DrawLine(turnX, startY, turnX, endY);
            canvas.DrawLine(turnX, endY, endX - ArrowSize, endY);

            using var arrow = new PathF();
            arrow.MoveTo(endX, endY);
            arrow.LineTo(endX - (ArrowSize * 1.6f), endY - ArrowSize);
            arrow.LineTo(endX - (ArrowSize * 1.6f), endY + ArrowSize);
            arrow.Close();
            canvas.FillColor = LinkColor;
            canvas.FillPath(arrow);
        }
    }

    private void DrawToday(ICanvas canvas, RectF dirtyRect, ProjectSchedule value)
    {
        var x = (value.OffsetOf(value.Today) * dayWidth) + (dayWidth / 2f);
        canvas.StrokeColor = TodayColor;
        canvas.StrokeSize = 2f;
        canvas.DrawLine(x, dirtyRect.Top, x, dirtyRect.Bottom);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private RectF BarOf(ProjectSchedule value, ProjectTask task, int index)
    {
        var inset = (float)RowHeight * BarInsetRate;
        var x = value.OffsetOf(task.Start) * dayWidth;
        var width = Math.Max(dayWidth * 0.6f, task.Days * dayWidth);
        return new RectF(x, (index * (float)RowHeight) + inset, width, (float)RowHeight - (inset * 2f));
    }

    private static bool IsHoliday(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static Color ColorOf(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => SaturdayColor,
        DayOfWeek.Sunday => SundayColor,
        _ => TextColor
    };

    private static string WeekOf(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Sunday => "日",
        DayOfWeek.Monday => "月",
        DayOfWeek.Tuesday => "火",
        DayOfWeek.Wednesday => "水",
        DayOfWeek.Thursday => "木",
        DayOfWeek.Friday => "金",
        _ => "土"
    };

    private Color ColorOf(ProjectTaskState state) => state switch
    {
        ProjectTaskState.Completed => CompletedColor,
        ProjectTaskState.Running => RunningColor,
        ProjectTaskState.Delayed => DelayedColor,
        ProjectTaskState.Milestone => MilestoneColor,
        _ => WaitingColor
    };
}
