namespace Template.MobileApp.Graphics.Drawing;

public sealed class LoadDrawing : DrawingObject
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const string AnimationName = "LoadFlow";

    private const int MaxBars = 60;

    private const int Capacity = MaxBars + 1;

    private const uint FlowLength = 200;

    private const float Gap = 2f;

    private const float AxisWidth = 30f;

    private const float VerticalPadding = 8f;

    private const int ScaleStep = 20;

    private static readonly Color BackgroundColor = Color.FromArgb("#2E2F45");

    private static readonly Color GridColor = Color.FromRgba(255, 255, 255, 28);

    private static readonly Color ScaleTextColor = Color.FromArgb("#9FA3C3");

    private static readonly (float Value, string Label, Color Color)[] Guides =
    [
        (50f, "NORMAL", Colors.Gold),
        (70f, "LOUD", Colors.Red)
    ];

    private static readonly LinearGradientPaint GradientPaint = new(
        [
            new PaintGradientStop(0f, Colors.LimeGreen),
            new PaintGradientStop(0.5f, Colors.Gold),
            new PaintGradientStop(1f, Colors.Red)
        ],
        startPoint: new Point(0, 1),
        endPoint: new Point(0, 0));

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private readonly float[] buffer = new float[Capacity];

    private int writeIndex;

    private int count;

    private float phase = 1f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public int Min
    {
        get;
        set
        {
            field = value;
            Invalidate();
        }
    }

    public int Max
    {
        get;
        set
        {
            field = value;
            Invalidate();
        }
    }

    //--------------------------------------------------------------------------------
    // Operations
    //--------------------------------------------------------------------------------

    public LoadDrawing()
    {
        Min = 0;
        Max = 100;
    }

    public (double Avg, double Min, double Max) CalcStatics()
    {
        if (count == 0)
        {
            return (0, 0, 0);
        }

        var length = Math.Min(count, MaxBars);
        var sum = 0d;
        var min = double.MaxValue;
        var max = double.MinValue;
        for (var i = 0; i < length; i++)
        {
            var index = (writeIndex - 1 - i + Capacity) % Capacity;
            var value = buffer[index];
            sum += value;
            if (value < min)
            {
                min = value;
            }
            if (value > max)
            {
                max = value;
            }
        }

        return (sum / length, min, max);
    }

    public void Clear()
    {
        AbortAnimation(AnimationName);
        count = 0;
        phase = 1f;
        Invalidate();
    }

    public void AddValue(float value)
    {
        buffer[writeIndex] = value;
        writeIndex = (writeIndex + 1) % Capacity;
        if (count < Capacity)
        {
            count++;
        }

        phase = 0f;
        AnimateValue(AnimationName, 0d, 1d, FlowLength, Easing.Linear, v => phase = (float)v);
    }

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        var range = (float)(Max - Min);
        if (range <= 0)
        {
            return;
        }

        var plot = new RectF(dirtyRect.Left + AxisWidth, dirtyRect.Top + VerticalPadding, dirtyRect.Width - AxisWidth - Gap, dirtyRect.Height - (VerticalPadding * 2));
        DrawBars(canvas, plot, range);
        DrawScale(canvas, dirtyRect, plot, range);
    }

    private float ToY(RectF plot, float range, float value) =>
        plot.Bottom - (plot.Height * Math.Clamp((value - Min) / range, 0f, 1f));

    private void DrawBars(ICanvas canvas, RectF plot, float range)
    {
        var barWidth = plot.Width / MaxBars;
        var shift = (1f - phase) * barWidth;
        canvas.SaveState();
        canvas.ClipRectangle(plot);
        for (var i = 0; i < count; i++)
        {
            var idx = (writeIndex - 1 - i + Capacity) % Capacity;
            var x = plot.Right - ((i + 1) * barWidth) + shift;
            var fullRect = new RectF(x + (Gap / 2), plot.Top, barWidth - Gap, plot.Height);

            canvas.SaveState();
            canvas.SetFillPaint(GradientPaint, fullRect);
            canvas.FillRectangle(fullRect);
            canvas.RestoreState();

            var maskRect = new RectF(x, plot.Top, barWidth, ToY(plot, range, buffer[idx]) - plot.Top);
            canvas.FillColor = BackgroundColor;
            canvas.FillRectangle(maskRect);
        }

        canvas.RestoreState();
    }

    private void DrawScale(ICanvas canvas, RectF area, RectF plot, float range)
    {
        canvas.StrokeSize = 1;
        canvas.FontSize = 10f;
        for (var value = Min; value <= Max; value += ScaleStep)
        {
            var y = ToY(plot, range, value);
            canvas.StrokeColor = GridColor;
            canvas.DrawLine(plot.Left, y, plot.Right, y);
            canvas.FontColor = ScaleTextColor;
            canvas.DrawString(value.ToString(CultureInfo.InvariantCulture), area.Left, y - 7f, AxisWidth - 6f, 14f, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        canvas.StrokeDashPattern = [4f, 3f];
        foreach (var (value, label, color) in Guides)
        {
            var y = ToY(plot, range, value);
            canvas.StrokeColor = color.WithAlpha(0.85f);
            canvas.DrawLine(plot.Left, y, plot.Right, y);
            canvas.FontColor = color;
            canvas.DrawString(label, plot.Left + 4f, y - 15f, 80f, 13f, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        }

        canvas.StrokeDashPattern = null;
        canvas.FontColor = Colors.LimeGreen;
        canvas.DrawString("QUIET", plot.Left + 4f, ToY(plot, range, Guides[0].Value) + 2f, 80f, 13f, HorizontalAlignment.Left, VerticalAlignment.Top);
    }
}
