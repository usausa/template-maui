namespace Template.MobileApp.Helpers;

using CommunityToolkit.Maui.Views;

using Microsoft.Maui;

public static partial class ElementHelper
{
    public static VisualElement? FindFocused(IVisualTreeElement parent)
    {
        var children = parent.GetVisualChildren();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
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

    public static VisualElement? FindFocusable(IVisualTreeElement parent, Func<VisualElement, bool> predicate) =>
        FindActive(parent, predicate, static (x, p) => IsPlatformFocusable(x) && p(x));

    private static partial bool IsPlatformFocusable(VisualElement visual);

    public static VisualElement? FindActive<TState>(IVisualTreeElement parent, TState state, Func<VisualElement, TState, bool> predicate)
    {
        var children = parent.GetVisualChildren();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is not VisualElement { IsEnabled: true, IsVisible: true } visual)
            {
                continue;
            }

            if (predicate(visual, state))
            {
                return visual;
            }

            var found = FindActive(visual, state, predicate);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    public static bool MoveFocus(VisualElement parent, bool forward) =>
        PlatformMoveFocus(parent, null, forward);

    public static bool MoveFocusInRoot(VisualElement current, bool forward)
    {
        var parent = current.FindRoot();
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
