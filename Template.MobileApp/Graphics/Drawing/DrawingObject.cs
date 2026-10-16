namespace Template.MobileApp.Graphics.Drawing;

public interface IDrawingObject : IDrawable
{
    void Attach(DrawingControl view);

    void Detach();
}

public interface IInteractiveDrawing
{
    void OnInteractionStart(PointF point);

    void OnInteractionDrag(PointF point);

    void OnInteractionEnd(PointF point);
}

public interface ITapDrawing
{
    void OnTap(PointF point);
}

#pragma warning disable CA1033
public abstract class DrawingObject : IDrawingObject
{
    private DrawingControl? control;

    void IDrawingObject.Attach(DrawingControl view)
    {
        control = view;
    }

    void IDrawingObject.Detach()
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
                control.Dispatcher.Dispatch(control.Invalidate);
            }
            else
            {
                control.Invalidate();
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    protected void AnimateValue(string name, double start, double end, uint length, Easing easing, Action<double> frame, Action? completed = null)
    {
        if (control is null)
        {
            frame(end);
            completed?.Invoke();
            return;
        }

        control.Animate(
            name,
            v =>
            {
                frame(v);
                Invalidate();
            },
            start,
            end,
            16,
            length,
            easing,
            (_, cancelled) =>
            {
                if (!cancelled)
                {
                    completed?.Invoke();
                }
            });
    }

    protected void AbortAnimation(string name) => control?.AbortAnimation(name);

    protected bool AnimationIsRunning(string name) => (control is not null) && control.AnimationIsRunning(name);

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        OnDraw(canvas, dirtyRect);
    }

    protected abstract void OnDraw(ICanvas canvas, RectF dirtyRect);

    public void ExportPng(Stream stream, int width, int height)
    {
        using var context = new Microsoft.Maui.Graphics.Platform.PlatformBitmapExportContext(width, height);
        OnDraw(context.Canvas, new RectF(0f, 0f, width, height));
        context.WriteToStream(stream);
    }
}
#pragma warning restore CA1033
