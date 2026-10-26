namespace Template.MobileApp.Extender.Effects;

public sealed class DialogOpenEffect : DialogEffect
{
    public DialogOpenEffect(double minScale = 0.7d, uint durationMilliseconds = 220)
        : base(minScale, durationMilliseconds)
    {
    }

    protected override Task EnterAsync(MauiNavigationEffectContext context) => ShowAsync(context.View);

    protected override Task LeaveAsync(MauiNavigationEffectContext context) => Task.CompletedTask;
}
