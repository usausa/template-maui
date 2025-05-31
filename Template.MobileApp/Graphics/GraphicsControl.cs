namespace Template.MobileApp.Graphics;

public sealed class GraphicsControl : GraphicsView
{
    public static readonly BindableProperty GraphicsProperty = BindableProperty.Create(
        nameof(Graphics),
        typeof(IGraphics),
        typeof(GraphicsControl),
        propertyChanged: HandlePropertyChanged);

    public IGraphics Graphics
    {
        get => (IGraphics)GetValue(GraphicsProperty);
        set => SetValue(GraphicsProperty, value);
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        ((GraphicsControl)bindable).HandlePropertyChanged(oldValue as IGraphics, newValue as IGraphics);
    }

    private void HandlePropertyChanged(IGraphics? oldValue, IGraphics? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= HandlePropertyChanged;
            Drawable = null!;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged += HandlePropertyChanged;
            Drawable = newValue;
        }
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Invalidate();
    }
}
