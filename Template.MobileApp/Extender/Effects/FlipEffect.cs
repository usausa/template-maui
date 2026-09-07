namespace Template.MobileApp.Extender.Effects;

public sealed class FlipEffect : IMauiNavigationEffect
{
    private readonly uint duration;

    public FlipEffect(uint durationMilliseconds = 300)
    {
        duration = durationMilliseconds;
    }

    public async Task EffectAsync(MauiNavigationEffectContext context)
    {
        var view = context.View;

        if (context.Phase is MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate)
        {
            view.RotationY = -90;
            await view.RotateYToAsync(0, duration, Easing.CubicOut).ConfigureAwait(true);
        }
        else
        {
            await view.RotateYToAsync(90, duration, Easing.CubicIn).ConfigureAwait(true);
        }

        view.RotationY = 0;
    }
}
