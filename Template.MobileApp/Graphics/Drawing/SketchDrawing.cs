namespace Template.MobileApp.Graphics.Drawing;

// フリーハンド描画 (IInteractiveDrawing の実装例)。
// ストロークは色ごとに保持し、Undo は末尾削除・Export は表示と同じ OnDraw を使う
public sealed class SketchDrawing : DrawingObject, IInteractiveDrawing
{
    private static readonly Color[] Palette =
    [
        Color.FromArgb("#1E88E5"),
        Color.FromArgb("#43A047"),
        Color.FromArgb("#FB8C00"),
        Color.FromArgb("#8E24AA"),
        Color.FromArgb("#E53935")
    ];

    private sealed record Stroke(Color Color, List<PointF> Points);

    private readonly List<Stroke> strokes = [];

    private Stroke? current;

    public event EventHandler? StrokesChanged;

    public int StrokeCount => strokes.Count;

    public void Undo()
    {
        if (strokes.Count > 0)
        {
            strokes.RemoveAt(strokes.Count - 1);
            StrokesChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public void Clear()
    {
        if (strokes.Count > 0)
        {
            strokes.Clear();
            StrokesChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    //--------------------------------------------------------------------------------
    // Interaction
    //--------------------------------------------------------------------------------

    public void OnInteractionStart(PointF point)
    {
        current = new Stroke(Palette[strokes.Count % Palette.Length], [point]);
        strokes.Add(current);
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    public void OnInteractionDrag(PointF point)
    {
        current?.Points.Add(point);
        Invalidate();
    }

    public void OnInteractionEnd(PointF point)
    {
        current?.Points.Add(point);
        current = null;
        Invalidate();
    }

    //--------------------------------------------------------------------------------
    // Draw
    //--------------------------------------------------------------------------------

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);
        canvas.Antialias = true;
        canvas.StrokeSize = 4f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        foreach (var stroke in strokes)
        {
            if (stroke.Points.Count < 2)
            {
                canvas.FillColor = stroke.Color;
                canvas.FillCircle(stroke.Points[0], 2f);
                continue;
            }

            using var path = new PathF();
            path.MoveTo(stroke.Points[0]);
            for (var i = 1; i < stroke.Points.Count; i++)
            {
                path.LineTo(stroke.Points[i]);
            }

            canvas.StrokeColor = stroke.Color;
            canvas.DrawPath(path);
        }
    }
}
