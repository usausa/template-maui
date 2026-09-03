namespace Template.MobileApp.Graphics.Drawing;

public sealed class DrawingControl : GraphicsView
{
    public static readonly BindableProperty DrawingProperty = BindableProperty.Create(
        nameof(Drawing),
        typeof(IDrawingObject),
        typeof(DrawingControl),
        propertyChanged: HandlePropertyChanged);

    public IDrawingObject Drawing
    {
        get => (IDrawingObject)GetValue(DrawingProperty);
        set => SetValue(DrawingProperty, value);
    }

    public DrawingControl()
    {
        // Drawing が IInteractiveDrawing のときだけタッチ操作を転送する
        StartInteraction += (_, e) =>
        {
            if ((Drawing is IInteractiveDrawing interactive) && (e.Touches.Length > 0))
            {
                interactive.OnInteractionStart(e.Touches[0]);
            }
        };
        DragInteraction += (_, e) =>
        {
            if ((Drawing is IInteractiveDrawing interactive) && (e.Touches.Length > 0))
            {
                interactive.OnInteractionDrag(e.Touches[0]);
            }
        };
        EndInteraction += (_, e) =>
        {
            if ((Drawing is IInteractiveDrawing interactive) && (e.Touches.Length > 0))
            {
                interactive.OnInteractionEnd(e.Touches[0]);
            }
        };
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        ((DrawingControl)bindable).HandlePropertyChanged(oldValue as IDrawingObject, newValue as IDrawingObject);
    }

    private void HandlePropertyChanged(IDrawingObject? oldValue, IDrawingObject? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.Detach();
            Drawable = null;
        }
        if (newValue is not null)
        {
            newValue.Attach(this);
            Drawable = newValue;
        }
    }
}
