namespace Template.MobileApp.Shell;

public sealed class BusyView : IBusyView
{
    private readonly BusyConfig config;

    private BusyOverlay? overlay;

    private bool visible;

    public BusyView(BusyConfig config)
    {
        this.config = config;
    }

    private BusyOverlay GetOverlay()
    {
        overlay ??= new BusyOverlay(Application.Current!.Windows[0], config);
        return overlay;
    }

    public void Show()
    {
        if (visible)
        {
            return;
        }

        var o = GetOverlay();
        if (o.Window.AddOverlay(o))
        {
            visible = true;
        }
    }

    public void Hide()
    {
        if (!visible)
        {
            return;
        }

        var o = GetOverlay();
        if (o.Window.RemoveOverlay(o))
        {
            visible = false;
        }
    }
}
