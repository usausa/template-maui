namespace Template.MobileApp.Controls;

#pragma warning disable CA1001
public sealed class SpeedGauge : GraphicsView, IDrawable
{
    public SpeedGauge()
    {
        Drawable = this;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
    }
}
#pragma warning restore CA1001
