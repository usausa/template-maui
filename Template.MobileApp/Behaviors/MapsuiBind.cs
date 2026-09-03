namespace Template.MobileApp.Behaviors;

using System.Linq;

using Mapsui.UI.Maui;

using Smart.Maui.Interactivity;

public static class MapsuiBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IMapsuiController),
        typeof(MapsuiBind),
        null,
        propertyChanged: BindChanged);

    public static IMapsuiController? GetController(BindableObject bindable) =>
        (IMapsuiController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IMapsuiController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not MapControl view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is MapsuiBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new MapsuiBindBehavior());
        }
    }

    // SkiaSharp オーバーレイ (SKCanvasView) を同じコントローラへ結線する
    public static readonly BindableProperty OverlayProperty = BindableProperty.CreateAttached(
        "Overlay",
        typeof(IMapsuiController),
        typeof(MapsuiBind),
        null,
        propertyChanged: OverlayChanged);

    public static IMapsuiController? GetOverlay(BindableObject bindable) =>
        (IMapsuiController)bindable.GetValue(OverlayProperty);

    public static void SetOverlay(BindableObject bindable, IMapsuiController? value) =>
        bindable.SetValue(OverlayProperty, value);

    private static void OverlayChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not SkiaSharp.Views.Maui.Controls.SKCanvasView view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is MapsuiOverlayBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new MapsuiOverlayBehavior());
        }
    }

    private sealed class MapsuiOverlayBehavior : BehaviorBase<SkiaSharp.Views.Maui.Controls.SKCanvasView>
    {
        private IMapsuiController? controller;

        protected override void OnAttachedTo(SkiaSharp.Views.Maui.Controls.SKCanvasView bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetOverlay(bindable);
            controller?.AttachOverlay(bindable);
        }

        protected override void OnDetachingFrom(SkiaSharp.Views.Maui.Controls.SKCanvasView bindable)
        {
            controller?.DetachOverlay(bindable);
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }

    private sealed class MapsuiBindBehavior : BehaviorBase<MapControl>
    {
        private IMapsuiController? controller;

        protected override void OnAttachedTo(MapControl bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            controller?.Attach(bindable);
        }

        protected override void OnDetachingFrom(MapControl bindable)
        {
            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
