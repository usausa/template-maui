namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using Template.MobileApp.Messaging;

public static class ScrollBind
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(ScrollController),
        typeof(ScrollBind),
        null,
        propertyChanged: BindChanged);

    public static ScrollController? GetController(BindableObject bindable) =>
        (ScrollController?)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, ScrollController? value) =>
        bindable.SetValue(ControllerProperty, value);

    public static readonly BindableProperty InsetXProperty = BindableProperty.CreateAttached(
        "InsetX",
        typeof(double),
        typeof(ScrollBind),
        0d);

    public static double GetInsetX(BindableObject bindable) => (double)bindable.GetValue(InsetXProperty);

    public static void SetInsetX(BindableObject bindable, double value) => bindable.SetValue(InsetXProperty, value);

    public static readonly BindableProperty InsetYProperty = BindableProperty.CreateAttached(
        "InsetY",
        typeof(double),
        typeof(ScrollBind),
        0d);

    public static double GetInsetY(BindableObject bindable) => (double)bindable.GetValue(InsetYProperty);

    public static void SetInsetY(BindableObject bindable, double value) => bindable.SetValue(InsetYProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is ScrollBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new ScrollBindBehavior());
        }
    }

    //--------------------------------------------------------------------------------
    // Behavior
    //--------------------------------------------------------------------------------

    private sealed class ScrollBindBehavior : BehaviorBase<View>
    {
        private ScrollController? controller;

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            if (controller is not null)
            {
                controller.ScrollRequest += HandleScrollRequest;
            }
        }

        protected override void OnDetachingFrom(View bindable)
        {
            if (controller is not null)
            {
                controller.ScrollRequest -= HandleScrollRequest;
                controller = null;
            }

            base.OnDetachingFrom(bindable);
        }

        private void HandleScrollRequest(object? sender, ScrollRequestEventArgs e)
        {
            if (e.Kind == ScrollRequestKind.Index)
            {
                ScrollToIndex((int)e.X);
            }
            else
            {
                ScrollToOffset(e.X, e.Y);
            }
        }

        private void ScrollToIndex(int index)
        {
            switch (AssociatedObject)
            {
                case ItemsView itemsView:
                    itemsView.ScrollTo(index, position: ScrollToPosition.Start);
                    break;
                case ScrollView { Content: Layout layout } scrollView when (index >= 0) && (index < layout.Count):
                    _ = scrollView.ScrollToAsync((Element)layout[index], ScrollToPosition.Start, true);
                    break;
            }
        }

        private void ScrollToOffset(double x, double y)
        {
            if (AssociatedObject is ScrollView scrollView)
            {
                _ = scrollView.ScrollToAsync(
                    Math.Max(0, x - GetInsetX(scrollView)),
                    Math.Max(0, y - GetInsetY(scrollView)),
                    true);
            }
        }
    }
}
