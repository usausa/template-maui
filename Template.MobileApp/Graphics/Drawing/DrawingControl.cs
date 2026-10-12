namespace Template.MobileApp.Graphics.Drawing;

public sealed partial class DrawingControl : GraphicsView
{
    // タップとみなす指の移動の上限
    private const float TapSlop = 10f;

    public static readonly BindableProperty DrawingProperty = BindableProperty.Create(
        nameof(Drawing),
        typeof(IDrawingObject),
        typeof(DrawingControl),
        propertyChanged: HandlePropertyChanged);

    private PointF touchStart;

    public IDrawingObject Drawing
    {
        get => (IDrawingObject)GetValue(DrawingProperty);
        set => SetValue(DrawingProperty, value);
    }

    public DrawingControl()
    {
        // Drawing が IInteractiveDrawing のときはタッチ操作を、ITapDrawing のときはタップを転送する
        StartInteraction += (_, e) =>
        {
            if (e.Touches.Length > 0)
            {
                touchStart = e.Touches[0];
            }
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
            if ((Drawing is ITapDrawing tap) && e.IsInsideBounds && (e.Touches.Length > 0) && (e.Touches[0].Distance(touchStart) <= TapSlop))
            {
                tap.OnTap(e.Touches[0]);
            }
        };

        HandlerChanging += (_, e) => DetachPlatformTouch(e.OldHandler?.PlatformView);
        HandlerChanged += (_, _) => AttachPlatformTouch();
    }

    // スクロールの中で縦になぞると親のスクロールに指を取られるため、プラットフォーム側で描いている間は止める
    partial void AttachPlatformTouch();

    partial void DetachPlatformTouch(object? platformView);

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
            // 解除時のクリアの null 代入はコンパイラ上有効 (ReSharper のみ non-null 違反と誤検知)
            // ReSharper disable once AssignNullToNotNullAttribute
            Drawable = null;
        }
        if (newValue is not null)
        {
            newValue.Attach(this);
            Drawable = newValue;
        }
    }
}
