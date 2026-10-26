namespace Template.MobileApp.Extender.Effects;

public abstract class DialogEffect : IMauiNavigationEffect
{
    private readonly double minScale;

    private readonly uint duration;

    protected DialogEffect(double minScale, uint durationMilliseconds)
    {
        this.minScale = minScale;
        duration = durationMilliseconds;
    }

    public Task EffectAsync(MauiNavigationEffectContext context) =>
        context.Phase switch
        {
            MauiNavigationEffectPhase.Open or MauiNavigationEffectPhase.Activate => EnterAsync(context),
            MauiNavigationEffectPhase.Close or MauiNavigationEffectPhase.Deactivate => LeaveAsync(context),
            _ => Task.CompletedTask
        };

    protected abstract Task EnterAsync(MauiNavigationEffectContext context);

    protected abstract Task LeaveAsync(MauiNavigationEffectContext context);

    protected async Task ShowAsync(View view)
    {
        view.Scale = minScale;
        view.Opacity = 0;
        await Task.WhenAll(
            view.ScaleToAsync(1, duration, Easing.CubicOut),
            view.FadeToAsync(1, duration)).ConfigureAwait(true);
        Reset(view);
    }

    protected async Task HideAsync(View view)
    {
        view.ZIndex = 1;
        await Task.WhenAll(
            view.ScaleToAsync(minScale, duration, Easing.CubicOut),
            view.FadeToAsync(0, duration)).ConfigureAwait(true);
        view.ZIndex = 0;
        Reset(view);
    }

    private static void Reset(View view)
    {
        view.Scale = 1;
        view.Opacity = 1;
    }
}
