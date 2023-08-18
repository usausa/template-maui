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
        var start = current is not null ? ResolveCurrentView(current) : viewGroup.FindFocus();
        var focused = start;
        while (true)
        {
            var next = ff.FindNextFocus(viewGroup, focused, forward ? FocusSearchDirection.Forward : FocusSearchDirection.Backward);

            // No more control
            if ((next is null) || (next == start))
            {
                return false;
            }

            // Focus if enabled
            if (next.Enabled)
            {
                return next.RequestFocus();
            }

            // Find next
            focused = next;
        }
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
