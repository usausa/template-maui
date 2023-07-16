namespace Template.MobileApp.Helpers;

using Android.Views;

using Microsoft.Maui.Platform;

public static partial class ElementHelper
{
    private static partial bool IsPlatformFocusable(VisualElement visual) =>
        (visual.Handler?.PlatformView is View view) && view.Focusable;

    private static partial bool PlatformMoveFocus(VisualElement parent, VisualElement? current, bool forward)
    {
        if (parent.Handler?.MauiContext is null)
        {
            Android.Util.Log.Debug(nameof(ElementHelper), null!, "Parent context is null.");
            return false;
        }

        var ff = FocusFinder.Instance;
        if (ff is null)
        {
            Android.Util.Log.Debug(nameof(ElementHelper), null!, "FocusFinder is null.");
            return false;
        }

        var viewGroup = (ViewGroup)parent.ToPlatform(parent.Handler.MauiContext);
        var focused = current is not null ? ResolveCurrentView(current) : viewGroup.FindFocus();
        var next = ff.FindNextFocus(viewGroup, focused, forward ? FocusSearchDirection.Down : FocusSearchDirection.Up);

        return next?.RequestFocus() ?? false;
    }

    private static View? ResolveCurrentView(VisualElement element)
    {
        if (element.Handler?.MauiContext is null)
        {
            Android.Util.Log.Debug(nameof(ElementHelper), null!, "Element context is null.");
            return null;
        }

        return element.ToPlatform(element.Handler.MauiContext);
    }
}
