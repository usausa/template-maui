namespace Template.MobileApp.Extender.Effects;

public static class AppEffect
{
    public const string Zoom = nameof(Zoom);
    public const string Drop = nameof(Drop);
    public const string Flip = nameof(Flip);
    public const string Rotate = nameof(Rotate);
    public const string DialogOpen = nameof(DialogOpen);
    public const string DialogClose = nameof(DialogClose);

    public static string Reverse(string effect) => effect switch
    {
        MauiEffect.Forward => MauiEffect.Back,
        MauiEffect.Back => MauiEffect.Forward,
        MauiEffect.Push => MauiEffect.Pop,
        MauiEffect.Pop => MauiEffect.Push,
        _ => effect
    };
}
