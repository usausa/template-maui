namespace Template.MobileApp.Modules.Basic;

using Template.MobileApp.Graphics;

public sealed class BasicGraphicsViewModel : AppViewModelBase
{
    public ShapeGraphics Graphics { get; } = new();

    public BasicGraphicsViewModel()
    {
        Graphics.Size = new SizeF(100, 100);
        Graphics.Shapes.Add(new Line { Color = Colors.Blue, Point1 = new PointF(10, 10), Point2 = new PointF(90, 90) });
        Graphics.Shapes.Add(new Line { Color = Colors.Blue, Point1 = new PointF(10, 90), Point2 = new PointF(90, 10) });
        Graphics.Shapes.Add(new Rectangle { Color = Colors.Red, Rect = new RectF(40, 40, 20, 20) });
        Graphics.Invalidate();
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
