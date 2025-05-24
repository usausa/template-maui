namespace Template.MobileApp.Shell;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder UseCustomBusyOverlay(this MauiAppBuilder builder)
    {
        return builder.UseCustomBusyOverlay(_ => { });
    }

    public static MauiAppBuilder UseCustomBusyOverlay(this MauiAppBuilder builder, Action<BusyConfig> action)
    {
        builder.Services.AddSingleton(_ =>
        {
            var config = new BusyConfig();
            action(config);
            return config;
        });

        builder.Services.AddSingleton<IBusyView, BusyView>();

        return builder;
    }
}
