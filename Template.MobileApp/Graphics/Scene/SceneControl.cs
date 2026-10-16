namespace Template.MobileApp.Graphics.Scene;

using SkiaSharp.Views.Maui;

public sealed class SceneControl : SKCanvasView
{
    public static readonly BindableProperty SceneProperty = BindableProperty.Create(
        nameof(Scene),
        typeof(ISceneObject),
        typeof(SceneControl),
        propertyChanged: HandlePropertyChanged);

    public ISceneObject? Scene
    {
        get => (ISceneObject?)GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    public SceneControl()
    {
        BackgroundColor = Colors.Transparent;
        EnableTouchEvents = true;
        PaintSurface += OnPaintSurface;
        Touch += OnTouch;
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if ((e.ActionType == SKTouchAction.Pressed) && (Scene is { } scene))
        {
            e.Handled = scene.Touch(e.Location, (int)CanvasSize.Width, (int)CanvasSize.Height);
        }
    }

    private static void HandlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        ((SceneControl)bindable).HandlePropertyChanged(oldValue as ISceneObject, newValue as ISceneObject);
    }

    private void HandlePropertyChanged(ISceneObject? oldValue, ISceneObject? newValue)
    {
        oldValue?.Detach();
        newValue?.Attach(this);
        InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var scene = Scene;
        if (scene is null)
        {
            e.Surface.Canvas.Clear();
            return;
        }

        scene.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
    }
}
