namespace Template.MobileApp;

using System.Reflection;

using CommunityToolkit.Maui;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Smart.Resolver;

using Template.MobileApp.Modules;
using Template.MobileApp.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCommunityToolkit()
            .ConfigureService(services =>
            {
#if ANDROID
                services.AddComponentsDialog();
#endif
                services.AddComponentsPopup(c =>
                    c.AutoRegister(Assembly.GetExecutingAssembly().UnderNamespaceTypes(typeof(DialogId))));
                services.AddComponentsSerializer();
            })
            .ConfigureContainer(new SmartServiceProviderFactory(), ConfigureContainer);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        if (!String.IsNullOrEmpty(Variants.AppCenterSecret()))
        {
            AppCenter.Start(Variants.AppCenterSecret(), typeof(Analytics), typeof(Crashes));
        }

        return builder.Build();
    }

    private static void ConfigureContainer(ResolverConfig config)
    {
        config
            .UseAutoBinding()
            .UseArrayBinding()
            .UseAssignableBinding()
            .UsePropertyInjector()
            .UsePageContextScope();

        // Components
        config.BindSingleton<IMauiInitializeService, ApplicationInitializer>();

        config.BindSingleton<ApplicationState>();

        // Service
        config.BindSingleton(new DataServiceOptions
        {
            Path = Path.Combine(FileSystem.AppDataDirectory, "Data.db")
        });

        config.BindSingleton<DataService>();
        config.AddNavigator(c =>
        {
            c.UseMauiNavigationProvider();
            // TODO
            //c.AddPlugin<NavigationFocusPlugin>();
            c.UseIdViewMapper(m =>
                m.AutoRegister(Assembly.GetExecutingAssembly().UnderNamespaceTypes(typeof(ViewId))));
        });
    }
}
