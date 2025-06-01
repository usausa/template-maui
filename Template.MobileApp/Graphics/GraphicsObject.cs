namespace Template.MobileApp.Graphics;

public abstract class GraphicsObject : IGraphicsObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public event EventHandler<EventArgs>? InvalidateRequest;

    public void Invalidate()
    {
        InvalidateRequest?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Draw(ICanvas canvas, RectF dirtyRect);
}
