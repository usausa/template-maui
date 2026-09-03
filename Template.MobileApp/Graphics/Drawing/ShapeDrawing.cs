namespace Template.MobileApp.Graphics.Drawing;

public interface IShape
{
    void Draw(ICanvas canvas);
}

public sealed class Line : IShape
{
    public PointF Point1 { get; set; }

    public PointF Point2 { get; set; }

    public Color Color { get; set; } = Colors.Black;

    public float Width { get; set; } = 1;

    void IShape.Draw(ICanvas canvas)
    {
        canvas.StrokeColor = Color;
        canvas.StrokeSize = Width;
        canvas.DrawLine(Point1, Point2);
    }
}

public sealed class Rectangle : IShape
{
    public RectF Rect { get; set; }

    public Color Color { get; set; } = Colors.Black;

    void IShape.Draw(ICanvas canvas)
    {
        canvas.FillColor = Color;
        canvas.FillRectangle(Rect);
    }
}

public sealed class Circle : IShape
{
    public PointF Center { get; set; }

    public float Radius { get; set; }

    public Color Color { get; set; } = Colors.Black;

    void IShape.Draw(ICanvas canvas)
    {
        canvas.FillColor = Color;
        canvas.FillCircle(Center, Radius);
    }
}

#pragma warning disable CA1002
public sealed class ShapeDrawing : DrawingObject
{
    public SizeF Size { get; set; }

    public List<IShape> Shapes { get; } = [];

    protected override void OnDraw(ICanvas canvas, RectF dirtyRect)
    {
        if ((Size.Width != 0) && (Size.Height != 0))
        {
            canvas.Scale(dirtyRect.Width / Size.Width, dirtyRect.Height / Size.Height);
        }

        foreach (var shape in Shapes)
        {
            shape.Draw(canvas);
        }
    }
}
#pragma warning restore CA1002
