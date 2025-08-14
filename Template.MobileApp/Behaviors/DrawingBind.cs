namespace Template.MobileApp.Behaviors;

using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

public static class DrawingBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IDrawingController),
        typeof(DrawingBind),
        null,
        propertyChanged: BindChanged);

    public static IDrawingController? GetController(BindableObject bindable) =>
        (IDrawingController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IDrawingController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not DrawingView view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is DrawingBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new DrawingBindBehavior());
        }
    }

    private sealed class DrawingBindBehavior : BehaviorBase<DrawingView>
    {
        private IDrawingController? controller;

        protected override void OnAttachedTo(DrawingView bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if ((controller is not null) && (AssociatedObject is not null))
            {
                controller.Attach(bindable);

                bindable.SetBinding(
                    DrawingView.LineColorProperty,
                    static (IDrawingController controller) => controller.LineColor,
                    source: controller);
                bindable.SetBinding(
                    DrawingView.LineWidthProperty,
                    static (IDrawingController controller) => controller.LineWidth,
                    source: controller);
                bindable.SetBinding(
                    DrawingView.LinesProperty,
                    static (IDrawingController controller) => controller.Lines,
                    source: controller);
            }
        }

        protected override void OnDetachingFrom(DrawingView bindable)
        {
            bindable.RemoveBinding(DrawingView.LineColorProperty);
            bindable.RemoveBinding(DrawingView.LineWidthProperty);
            bindable.RemoveBinding(DrawingView.LinesProperty);

            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
