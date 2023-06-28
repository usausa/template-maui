namespace Template.MobileApp.Behaviors;

internal static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder)
    {
        EntryOption.UseCustomMapper();
        return builder;
    }
}
