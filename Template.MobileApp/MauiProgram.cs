namespace Template.MobileApp;

using System.Reflection;

#if ANDROID
// ReSharper disable RedundantUsingDirective
using Android.Views;
// ReSharper restore RedundantUsingDirective
#endif

using CommunityToolkit.Maui;

using Microsoft.Maui.LifecycleEvents;

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
        // Builder
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
#if ANDROID
            // ReSharper disable UnusedParameter.Local
            .ConfigureLifecycleEvents(events =>
            {
                // Lifecycle
#if DEVICE_FULL_SCREEN
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                {
                    var window = activity.Window!;
                    window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
                }));
#endif
            })
            // ReSharper restore UnusedParameter.Local
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesome");
            })
            //.ConfigureEssentials(c => { })
            .UseMauiCommunityToolkit()
            .ConfigureCustomControls()
            .ConfigureCustomBehaviors()
            .ConfigureService(services =>
            {
                // TODO inside ConfigureContainer？
                // MauiComponents
#if ANDROID
                services.AddComponentsDialog(c =>
                {
                    var resources = Application.Current!.Resources;
                    c.IndicatorColor = resources.FindResource<Color>("BlueAccent1");
                    c.LoadingMessageBackgroundColor = Colors.White;
                    c.LoadingMessageColor = Colors.Black;
                    c.ProgressValueColor = Colors.Black;
                    c.ProgressAreaBackgroundColor = Colors.White;
                    c.ProgressCircleColor1 = resources.FindResource<Color>("BlueAccent1");
                    c.ProgressCircleColor2 = resources.FindResource<Color>("GrayLighten2");
#if DEVICE_HAS_KEYPAD
                    c.DismissKeys = new[] { Keycode.Escape, Keycode.Del };
                    c.IgnorePromptDismissKeys = new[] { Keycode.Del };
                    c.EnableDialogButtonFocus = true;
#endif
                    c.EnablePromptEnterAction = true;
                    c.EnablePromptSelectAll = true;
                });
#endif
                // TODO SourceGenerator?
                services.AddComponentsPopup(c =>
                    c.AutoRegister(Assembly.GetExecutingAssembly().UnderNamespaceTypes(typeof(DialogId))));
                services.AddComponentsSerializer();
            })
            .ConfigureContainer(new SmartServiceProviderFactory(), ConfigureContainer);

        // Logging
        builder.Logging
#if DEBUG
            .AddDebug()
#endif
#if ANDROID
            .AddAndroidLogger(options =>
            {
                options.ShortCategory = true;
            })
#endif
            .AddFileLogger(options =>
            {
#if ANDROID
                options.Directory = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(string.Empty)!.Path, "log");
#endif
                options.RetainDays = 7;
            })
            .AddFilter(typeof(MauiProgram).Namespace, LogLevel.Debug);

        if (!String.IsNullOrEmpty(Variants.AppCenterSecret()))
        {
            Microsoft.AppCenter.AppCenter.Start(
                Variants.AppCenterSecret(),
                typeof(Microsoft.AppCenter.Analytics.Analytics),
                typeof(Microsoft.AppCenter.Crashes.Crashes));
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
        config.BindSingleton(FileSystem.Current);
        config.BindSingleton(Preferences.Default);
        config.BindSingleton(Vibration.Default);

        // Components
        config.BindSingleton<IMauiInitializeService, ApplicationInitializer>();

        config.BindSingleton<IDeviceManager, DeviceManager>();

        // State
        config.BindSingleton<ApplicationState>();

        config.BindSingleton<Settings>();
        config.BindSingleton<Session>();

        // Service
#if DEBUG && ANDROID
        config.BindSingleton(_ => new DataServiceOptions { Path = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(string.Empty)!.Path, "Data.db") });
#else
        config.BindSingleton(p => new DataServiceOptions { Path = Path.Combine(p.GetRequiredService<IFileSystem>().AppDataDirectory, "Data.db") });
#endif

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
