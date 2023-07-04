namespace Template.MobileApp;

using System.Reflection;

public static class Extensions
{
    //--------------------------------------------------------------------------------
    // Resource
    //--------------------------------------------------------------------------------

    public static T FindResource<T>(this ResourceDictionary resource, string key) =>
        resource.TryGetValue(key, out var value) ? (T)value : default!;

    public static IEnumerable<Type> UnderNamespaceTypes(this Assembly assembly, Type baseNamespaceType)
    {
        var ns = baseNamespaceType.Namespace!;
        return assembly.ExportedTypes.Where(x => x.Namespace?.StartsWith(ns, StringComparison.Ordinal) ?? false);
    }

    //--------------------------------------------------------------------------------
    // Element
    //--------------------------------------------------------------------------------

    // TODO Focus

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public static async ValueTask PostForwardAsync(this INavigator navigator, object viewId, NavigationParameter? parameter = null)
    {
        if (navigator.Executing)
        {
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    await navigator.ForwardAsync(viewId, parameter);
                }
            }

            navigator.ExecutingChanged += ExecutingChanged;
        }
        else
        {
            await navigator.ForwardAsync(viewId, parameter);
        }
    }

    public static async ValueTask PostActionAsync(this INavigator navigator, Func<Task> task)
    {
        if (navigator.Executing)
        {
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    await task();
                }
            }

            navigator.ExecutingChanged += ExecutingChanged;
        }
        else
        {
            await task();
        }
    }
}
