namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Handlers;

public static partial class Scroll
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.DisableOverScroll)
        {
            ViewHandler.ViewMapper.AppendToMapping(DisableOverScrollProperty.PropertyName, UpdateDisableOverScroll);
        }
    }

    private static void UpdateDisableOverScroll(IViewHandler handler, IView view)
    {
        if ((view is BindableObject bindable) && GetDisableOverScroll(bindable) && (handler.PlatformView is View platformView))
        {
            platformView.OverScrollMode = OverScrollMode.Never;
        }
    }

    private static void UpdateOverScroll(View view, BindableObject element)
    {
        var value = GetDisableOverScroll(element);
        view.OverScrollMode = value ? OverScrollMode.Never : OverScrollMode.IfContentScrolls;
    }
}
