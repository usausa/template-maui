namespace Template.MobileApp.Extender.Effects;

public sealed class RotateEffect : IMauiNavigationEffect
{
    private readonly double angle;

    private readonly uint duration;

    public RotateEffect(double angle = 180d, uint durationMilliseconds = 380)
    {
        this.angle = angle;
        duration = durationMilliseconds;
    }

    public async Task EffectAsync(MauiNavigationEffectContext context)
    {
        var view = context.View;

        if (context.Phase is MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate)
        {
            view.Rotation = -angle;
            view.Opacity = 0;
            await Task.WhenAll(
                view.RotateToAsync(0, duration, Easing.CubicOut),
                view.FadeToAsync(1, duration)).ConfigureAwait(true);
        }
        else
        {
            await Task.WhenAll(
                view.RotateToAsync(angle, duration, Easing.CubicOut),
                view.FadeToAsync(0, duration)).ConfigureAwait(true);
        }

        view.Rotation = 0;
        view.Opacity = 1;
    }
}
