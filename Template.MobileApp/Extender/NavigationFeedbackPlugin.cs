namespace Template.MobileApp.Extender;

using Smart.Maui;
using Smart.Navigation.Plugins;

public sealed class NavigationFeedbackPlugin : PluginBase
{
    public override void OnNavigatedTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        // Disable footer button feedback on Android when navigating to a new page to prevent feedback from continuing on the new page
#if ANDROID
        if (view is not Element element)
        {
            return;
        }

        var page = element.FindParent<Page>();
        if (page is null)
        {
            return;
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            // ViewGroup jumpDrawablesToCurrentState call propagates to descendants
            if (page.Handler?.PlatformView is Android.Views.View platformView)
            {
                platformView.JumpDrawablesToCurrentState();
            }
        });
#endif
    }
}
