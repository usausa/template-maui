namespace Template.MobileApp;

using System.Reflection;

using CommunityToolkit.Maui;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Smart.Resolver;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components.Device;
using Template.MobileApp.Controls;
using Template.MobileApp.Modules;
using Template.MobileApp.Services;
using Template.MobileApp.State;

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
            .ConfigureCustomControls()
            .ConfigureCustomBehaviors()
            .ConfigureService(services =>
            {
#if ANDROID
                services.AddComponentsDialog();
#endif
                // TODO SourceGenerator?
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

        // MAUI
        config.BindSingleton(Preferences.Default);

        // Components
        config.BindSingleton<IMauiInitializeService, ApplicationInitializer>();

        config.BindSingleton<IDeviceManager, DeviceManager>();

        // State
        config.BindSingleton<ApplicationState>();

        config.BindSingleton<Settings>();
        config.BindSingleton<Session>();

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
            // TODO SourceGenerator?
            c.UseIdViewMapper(m =>
                m.AutoRegister(Assembly.GetExecutingAssembly().UnderNamespaceTypes(typeof(ViewId))));
        });
    }
}
