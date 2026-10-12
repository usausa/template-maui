namespace Template.MobileApp.Extender;

public sealed class PopupActivationPlugin : IPopupPlugin
{
    private readonly INavigator navigator;

    private INavigationLifecycleSupport? target;

    private int depth;

    public PopupActivationPlugin(INavigator navigator)
    {
        this.navigator = navigator;
    }

    public void Extend(ContentView view)
    {
    }

    public void OnOpening(ContentView view)
    {
        depth++;
        if (depth == 1)
        {
            // Deactivate current view
            target = navigator.CurrentTarget as INavigationLifecycleSupport;
            target?.OnDeactivated();
        }
    }

    public void OnClosed(ContentView view)
    {
        depth--;
        if (depth == 0)
        {
            // Activate current view
            if ((target is not null) && ReferenceEquals(navigator.CurrentTarget, target))
            {
                target.OnActivated();
            }

            target = null;
        }
    }
}
