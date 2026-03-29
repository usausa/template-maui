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
}
