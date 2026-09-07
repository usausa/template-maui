namespace Template.MobileApp.Extender.Effects;

public sealed class ZoomEffect : IMauiNavigationEffect
{
    private readonly double overshoot;

    private readonly uint duration;

    public ZoomEffect(double overshoot = 1.3d, uint durationMilliseconds = 240)
    {
        this.overshoot = overshoot;
        duration = durationMilliseconds;
    }

    public async Task EffectAsync(MauiNavigationEffectContext context)
    {
        var view = context.View;

        if (context.Phase is MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate)
        {
            view.Scale = overshoot;
            view.Opacity = 0;
            await Task.WhenAll(
                view.ScaleToAsync(1, duration, Easing.CubicOut),
                view.FadeToAsync(1, duration)).ConfigureAwait(true);
        }
        else
        {
            await Task.WhenAll(
                view.ScaleToAsync(overshoot, duration, Easing.CubicOut),
                view.FadeToAsync(0, duration)).ConfigureAwait(true);
        }

        view.Scale = 1;
        view.Opacity = 1;
    }
}
