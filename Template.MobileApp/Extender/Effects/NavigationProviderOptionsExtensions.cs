namespace Template.MobileApp.Extender.Effects;

public static class NavigationProviderOptionsExtensions
{
    public static MauiNavigationProviderOptions RegisterAppEffects(this MauiNavigationProviderOptions options)
    {
        options.RegisterEffect<ZoomEffect>(AppEffect.Zoom);
        options.RegisterEffect<DropEffect>(AppEffect.Drop);
        options.RegisterEffect<FlipEffect>(AppEffect.Flip);
        options.RegisterEffect<RotateEffect>(AppEffect.Rotate);
        options.RegisterEffect<DialogOpenEffect>(AppEffect.DialogOpen);
        options.RegisterEffect<DialogCloseEffect>(AppEffect.DialogClose);
        return options;
    }
}
