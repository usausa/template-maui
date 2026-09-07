namespace Template.MobileApp.Extender.Effects;

public static class NavigationProviderOptionsExtensions
{
    public static MauiNavigationProviderOptions RegisterAppEffects(this MauiNavigationProviderOptions options)
    {
        options.RegisterEffect(AppEffect.Zoom, new ZoomEffect());
        options.RegisterEffect(AppEffect.Drop, new DropEffect());
        options.RegisterEffect(AppEffect.Flip, new FlipEffect());
        options.RegisterEffect(AppEffect.Rotate, new RotateEffect());
        options.RegisterEffect(AppEffect.DialogOpen, new DialogEffect(open: true));
        options.RegisterEffect(AppEffect.DialogClose, new DialogEffect(open: false));
        return options;
    }
}
