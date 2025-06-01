namespace Template.MobileApp.Graphics;

public sealed class GraphicsControl : GraphicsView
{
    public static readonly BindableProperty GraphicsProperty = BindableProperty.Create(
        nameof(Graphics),
        typeof(IGraphicsObject),
        typeof(GraphicsControl),
        propertyChanged: HandlePropertyChanged);

    public IGraphicsObject Graphics
    {
        get => (IGraphicsObject)GetValue(GraphicsProperty);
        set => SetValue(GraphicsProperty, value);
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        ((GraphicsControl)bindable).HandlePropertyChanged(oldValue as IGraphicsObject, newValue as IGraphicsObject);
    }

    private void HandlePropertyChanged(IGraphicsObject? oldValue, IGraphicsObject? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.InvalidateRequest -= HandlePropertyChanged;
            Drawable = null!;
        }
        if (newValue is not null)
        {
            newValue.InvalidateRequest += HandlePropertyChanged;
            Drawable = newValue;
        }
    }

    private void HandlePropertyChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }
}
