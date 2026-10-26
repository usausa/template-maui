namespace Template.MobileApp.Graphics.Drawing;

public sealed class WaveformDrawing : DrawingObject, IInteractiveDrawing
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const float BarGapRate = 0.34f;

    private const float MinimumBarWidth = 1.5f;

    private const float HeadWidth = 2.5f;

    private static readonly Color PlayedColor = Colors.White;

    private static readonly Color RemainColor = Color.FromArgb("#59FFFFFF");

    private static readonly Color HeadColor = Color.FromArgb("#FF1744");

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private IReadOnlyList<float> values = [];

    private double progress;

    private RectF lastRect;

    public Action<double>? OnSeek { get; set; }

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(IReadOnlyList<float> source)
    {
        values = source;
        Invalidate();
    }

    public void UpdateProgress(double value)
    {
        progress = Math.Clamp(value, 0d, 1d);
        Invalidate();
    }

    public void OnInteractionStart(PointF point) => Seek(point);

    public void OnInteractionDrag(PointF point) => Seek(point);

    public void OnInteractionEnd(PointF point) => Seek(point);

    //--------------------------------------------------------------------------------
    // Drawing
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        lastRect = dirtyRect;

        if (values.Count == 0)
        {
            return;
        }

        canvas.SaveState();

        var slot = dirtyRect.Width / values.Count;
        var width = Math.Max(MinimumBarWidth, slot * (1f - BarGapRate));
        var center = dirtyRect.Center.Y;
        var head = dirtyRect.Left + (dirtyRect.Width * (float)progress);

        for (var i = 0; i < values.Count; i++)
        {
            var x = dirtyRect.Left + (i * slot) + ((slot - width) / 2f);
            var height = Math.Max(2f, dirtyRect.Height * values[i]);
            canvas.FillColor = x + width <= head ? PlayedColor : RemainColor;
            canvas.FillRoundedRectangle(x, center - (height / 2f), width, height, width / 2f);
        }

        canvas.FillColor = HeadColor;
        canvas.FillRectangle(head - (HeadWidth / 2f), dirtyRect.Top, HeadWidth, dirtyRect.Height);

        canvas.RestoreState();
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Seek(PointF point)
    {
        if (OnSeek is null)
        {
            return;
        }

        if (lastRect.Width > 0)
        {
            OnSeek(Math.Clamp((point.X - lastRect.Left) / lastRect.Width, 0f, 1f));
        }
    }
}
