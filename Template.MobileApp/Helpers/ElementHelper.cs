namespace Template.MobileApp.Helpers;

using CommunityToolkit.Maui.Views;

using Microsoft.Maui;

public static partial class ElementHelper
{
    public static VisualElement? FindFocused(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if ((child is VisualElement visual) && visual.IsFocused)
            {
                return visual;
            }

            var focused = FindFocused(child);
            if (focused is not null)
            {
                return focused;
            }
        }

        return null;
    }

    public static IEnumerable<VisualElement> EnumerateFocusable(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is not VisualElement { IsEnabled: true, IsVisible: true } visual)
            {
                continue;
            }

            if (IsPlatformFocusable(visual))
            {
                yield return visual;
            }

            foreach (var descendant in EnumerateFocusable(child))
            {
                yield return descendant;
            }
        }
    }

    private static partial bool IsPlatformFocusable(VisualElement visual);

    public static IEnumerable<T> EnumerateActive<T>(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is not VisualElement { IsEnabled: true, IsVisible: true })
            {
                continue;
            }

            if (child is T element)
            {
                yield return element;
            }

            foreach (var descendant in EnumerateActive<T>(child))
            {
                yield return descendant;
            }
        }
    }

    public static bool MoveFocus(VisualElement parent, bool forward) =>
        PlatformMoveFocus(parent, null, forward);

    public static bool MoveFocusInRoot(VisualElement current, bool forward)
    {
        var parent = FindRoot(current);
        if (parent is null)
        {
            return false;
        }

        return PlatformMoveFocus(parent, current, forward);
    }

    public static VisualElement? FindRoot(this Element element)
    {
        while (true)
        {
            var parent = element.Parent;
            if (parent is null)
            {
                return null;
            }

            if (element is Page page)
            {
                return page;
            }

            if (element is Popup popup)
            {
                return popup.Content;
            }

            element = parent;
        }
    }

    private static partial bool PlatformMoveFocus(VisualElement parent, VisualElement? current, bool forward);
}
