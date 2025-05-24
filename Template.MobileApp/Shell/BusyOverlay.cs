namespace Template.MobileApp.Shell;

public sealed class BusyOverlay : WindowOverlay
{
    public BusyOverlay(IWindow window, BusyConfig config)
        : base(window)
    {
        AddWindowElement(new OverlayElement(config));
        EnableDrawableTouchHandling = true;
    }

    private sealed class OverlayElement : IWindowOverlayElement
    {
        private readonly BusyConfig config;

        public OverlayElement(BusyConfig config)
        {
            this.config = config;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = config.BackgroundColor;
            canvas.FillRectangle(dirtyRect);
        }

        public bool Contains(Point point) => true;
    }
}
