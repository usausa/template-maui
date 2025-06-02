namespace Template.MobileApp.Behaviors;

internal static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder)
    {
        return builder.ConfigureCustomBehaviors(static _ => { });
    }

    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder, Action<BehaviorOptions> configure)
    {
        var options = new BehaviorOptions();
        configure(options);

        Border.UseCustomMapper(options);
        ButtonOption.UseCustomMapper();
        LabelOption.UseCustomMapper();
        EntryOption.UseCustomMapper(options);
        ListViewOption.UseCustomMapper(options);

        return builder;
    }
}
