namespace Template.MobileApp.Graphics.Drawing;

public sealed partial class DrawingControl
{
    partial void AttachPlatformTouch()
    {
        if (Handler?.PlatformView is Android.Views.View view)
        {
            view.Touch += OnPlatformTouch;
        }
    }

    partial void DetachPlatformTouch(object? platformView)
    {
        if (platformView is Android.Views.View view)
        {
            view.Touch -= OnPlatformTouch;
        }
    }

    private void OnPlatformTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if ((Drawing is IInteractiveDrawing) && (e.Event?.ActionMasked == Android.Views.MotionEventActions.Down))
        {
            (sender as Android.Views.View)?.Parent?.RequestDisallowInterceptTouchEvent(true);
        }

        // GraphicsView のタッチ処理にはそのまま渡す
        e.Handled = false;
    }
}
