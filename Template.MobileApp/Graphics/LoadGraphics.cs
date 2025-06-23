namespace Template.MobileApp.Graphics;

public sealed class LoadGraphics : GraphicsObject
{
    private const int MaxBars = 60;

    private const float Gap = 2f;

    private static readonly Color BackgroundColor = Color.FromArgb("#2E2F45");

    private static readonly LinearGradientPaint GradientPaint = new(
        [
            new PaintGradientStop(0f, Colors.LimeGreen),
            new PaintGradientStop(0.5f, Colors.Gold),
            new PaintGradientStop(1f, Colors.Red)
        ],
        startPoint: new Point(0, 1),
        endPoint: new Point(0, 0));

    private readonly float[] buffer = new float[MaxBars];

    private int writeIndex;

    private int count;

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

    public LoadGraphics()
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

        var sum = 0d;
        var min = double.MaxValue;
        var max = double.MinValue;
        for (var i = 0; i < count; i++)
        {
            var index = (writeIndex - 1 - i + MaxBars) % MaxBars;
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

        return (sum / count, min, max);
    }

    public void Clear()
    {
        count = 0;
        Invalidate();
    }

    public void AddValue(float value)
    {
        buffer[writeIndex] = value;
        writeIndex = (writeIndex + 1) % MaxBars;
        if (count < MaxBars)
        {
            count++;
        }

        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        if (count == 0)
        {
            return;
        }

        var max = Max;
        var min = Min;
        var range = max - min;
        if (range <= 0)
        {
            return;
        }

        var barWidth = dirtyRect.Width / MaxBars;

        for (var i = 0; i < count; i++)
        {
            var idx = (writeIndex - 1 - i + MaxBars) % MaxBars;

            var x = dirtyRect.Right - ((i + 1) * barWidth);
            var fullHeight = dirtyRect.Height;
            var yTop = dirtyRect.Top;
            var fullRect = new RectF(x + (Gap / 2), yTop, barWidth - Gap, fullHeight);

            canvas.SaveState();
            canvas.SetFillPaint(GradientPaint, fullRect);
            canvas.FillRectangle(fullRect);
            canvas.RestoreState();

            var value = buffer[idx] - min;
            if (value > max)
            {
                value = max;
            }

            var maskHeight = fullHeight * ((max - value) / range);
            var maskRect = new RectF(x, yTop, barWidth, maskHeight);
            canvas.FillColor = BackgroundColor;
            canvas.FillRectangle(maskRect);
        }
    }
}
