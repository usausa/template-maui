namespace Template.MobileApp.Graphics;

public interface IGraphicsObject : IDrawable
{
    void Attach(GraphicsControl view);

    void Detach();
}

#pragma warning disable CA1033
public abstract class GraphicsObject : IGraphicsObject
{
    private GraphicsControl? control;

    void IGraphicsObject.Attach(GraphicsControl view)
    {
        control = view;
    }

    void IGraphicsObject.Detach()
    {
        control = null;
    }

    public void Invalidate()
    {
        control?.Invalidate();
    }

    public void SafeInvalidate()
    {
        if (control is not null)
        {
            if (control.Dispatcher.IsDispatchRequired)
            {
                control.Dispatcher.Dispatch(() => control.Invalidate());
            }
            else
            {
                control.Invalidate();
            }
        }
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        OnDraw(canvas, dirtyRect);
    }

    protected abstract void OnDraw(ICanvas canvas, RectF dirtyRect);
}
#pragma warning restore CA1033
