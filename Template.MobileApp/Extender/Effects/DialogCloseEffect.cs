namespace Template.MobileApp.Extender.Effects;

public sealed class DialogCloseEffect : DialogEffect
{
    public DialogCloseEffect(double minScale = 0.7d, uint durationMilliseconds = 220)
        : base(minScale, durationMilliseconds)
    {
    }

    protected override Task EnterAsync(MauiNavigationEffectContext context) => Task.CompletedTask;

    protected override Task LeaveAsync(MauiNavigationEffectContext context) => HideAsync(context.View);
}
