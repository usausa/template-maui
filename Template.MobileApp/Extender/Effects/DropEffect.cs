namespace Template.MobileApp.Extender.Effects;

public sealed class DropEffect : IMauiNavigationEffect
{
    private readonly uint duration;

    public DropEffect(uint durationMilliseconds = 450)
    {
        duration = durationMilliseconds;
    }

    public async Task EffectAsync(MauiNavigationEffectContext context)
    {
        var height = context.Container.Height;
        if (height <= 0)
        {
            return;
        }

        var view = context.View;

        if (context.Phase is MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate)
        {
            view.TranslationY = -height;
            await view.TranslateToAsync(view.TranslationX, 0, duration, Easing.BounceOut).ConfigureAwait(true);
        }
        else
        {
            await view.TranslateToAsync(view.TranslationX, -height, duration, Easing.CubicIn).ConfigureAwait(true);
        }

        view.TranslationY = 0;
    }
}
