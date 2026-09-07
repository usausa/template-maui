namespace Template.MobileApp.Extender.Effects;

public sealed class DialogEffect : IMauiNavigationEffect
{
    private readonly bool open;

    private readonly double minScale;

    private readonly uint duration;

    public DialogEffect(bool open, double minScale = 0.7d, uint durationMilliseconds = 220)
    {
        this.open = open;
        this.minScale = minScale;
        duration = durationMilliseconds;
    }

    public async Task EffectAsync(MauiNavigationEffectContext context)
    {
        var view = context.View;

        if (open)
        {
            if (context.Phase is not (MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate))
            {
                return;
            }

            view.Scale = minScale;
            view.Opacity = 0;
            await Task.WhenAll(
                view.ScaleToAsync(1, duration, Easing.CubicOut),
                view.FadeToAsync(1, duration)).ConfigureAwait(true);
        }
        else
        {
            if (context.Phase is not (MauiNavigationEffectPhase.Close or MauiNavigationEffectPhase.Deactivate))
            {
                return;
            }

            view.ZIndex = 1;
            await Task.WhenAll(
                view.ScaleToAsync(minScale, duration, Easing.CubicOut),
                view.FadeToAsync(0, duration)).ConfigureAwait(true);
            view.ZIndex = 0;
        }

        view.Scale = 1;
        view.Opacity = 1;
    }
}
