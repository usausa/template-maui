namespace Template.MobileApp.Graphics;

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

public sealed class ShapeGraphics : GraphicsObject
{
    public SizeF Size { get; set; }

    public List<IShape> Shapes { get; } = new();

    public override void Draw(ICanvas canvas, RectF dirtyRect)
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
