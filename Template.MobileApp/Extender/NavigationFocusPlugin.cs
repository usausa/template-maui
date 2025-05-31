namespace Template.MobileApp.Extender;

using Template.MobileApp.Helpers;

using Smart.Maui;
using Smart.Navigation.Plugins;

public sealed class NavigationFocusPlugin : PluginBase
{
    private readonly Dictionary<object, VisualElement> focusBackup = [];

    public override void OnClose(IPluginContext pluginContext, object view, object? target)
    {
        focusBackup.Remove(view);
    }

    public override void OnNavigatingFrom(IPluginContext pluginContext, INavigationContext navigationContext, object? view, object? target)
    {
        if (navigationContext.Attribute.IsStacked() && (view is Element element))
        {
            var page = element.FindParent<Page>();
            if (page is not null)
            {
                var focused = ElementHelper.FindFocused(page);
                if (focused is not null)
                {
                    focusBackup[view] = focused;
                }
            }
        }
    }

    public override void OnNavigatedTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        if (navigationContext.Attribute.IsRestore())
        {
            if (focusBackup.TryGetValue(view, out var focused))
            {
                Application.Current?.Dispatcher.Dispatch(() => focused.Focus());
            }
        }
        else
        {
            var element = (Element)view;
            var page = element.FindParent<Page>();
            if (page is not null)
            {
                Application.Current?.Dispatcher.Dispatch(page.SetDefaultFocus);
            }
        }
    }
}
