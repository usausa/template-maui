namespace Template.MobileApp.Behaviors;

internal static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder)
    {
        return builder.ConfigureCustomBehaviors(_ => { });
    }

    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder, Action<BehaviorOptions> configure)
    {
        var options = new BehaviorOptions();
        configure(options);

        EntryOption.UseCustomMapper(options);
        Border.UseCustomMapper(options);

        return builder;
    }
}
