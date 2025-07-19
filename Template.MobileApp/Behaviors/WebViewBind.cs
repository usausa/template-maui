namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using IWebViewController = Template.MobileApp.Messaging.IWebViewController;

public static class WebViewBind
{
    public static readonly BindableProperty ControllerProperty = BindableProperty.CreateAttached(
        "Controller",
        typeof(IWebViewController),
        typeof(WebViewBind),
        null,
        propertyChanged: BindChanged);

    public static IWebViewController? GetController(BindableObject bindable) =>
        (IWebViewController)bindable.GetValue(ControllerProperty);

    public static void SetController(BindableObject bindable, IWebViewController? value) =>
        bindable.SetValue(ControllerProperty, value);

    private static void BindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not HybridWebView view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is WebViewBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new WebViewBindBehavior());
        }
    }

    private sealed class WebViewBindBehavior : BehaviorBase<HybridWebView>
    {
        private IWebViewController? controller;

        protected override void OnAttachedTo(HybridWebView bindable)
        {
            base.OnAttachedTo(bindable);

            controller = GetController(bindable);
            controller?.Attach(bindable);
        }

        protected override void OnDetachingFrom(HybridWebView bindable)
        {
            controller?.Detach();
            controller = null;

            base.OnDetachingFrom(bindable);
        }
    }
}
