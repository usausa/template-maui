namespace Template.MobileApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

#if ANDROID && DEVICE_HAS_KEYPAD
using Android.Views;
#endif

using CommunityToolkit.Maui;

using MauiComponents.Resolver;

using Microsoft.Maui.LifecycleEvents;

using Rester;

using Smart.Data.Mapper;
using Smart.Resolver;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components.Storage;
using Template.MobileApp.Controls;
using Template.MobileApp.Extender;
using Template.MobileApp.Helpers;
using Template.MobileApp.Helpers.Data;
using Template.MobileApp.Modules;
using Template.MobileApp.Services;
using Template.MobileApp.Usecase;

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
                events.AddAndroid(android => android.OnCreate(static (activity, _) => AndroidHelper.FullScreen(activity)));
#endif
            })
            // ReSharper restore UnusedParameter.Local
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesome");
            })
            //.ConfigureEssentials(static c => { })
            .UseMauiCommunityToolkit()
            .UseMauiInterfaces()
            .UseCommunityToolkitInterfaces()
            .ConfigureCustomControls()
            .ConfigureCustomBehaviors()
            .ConfigureContainer(new SmartServiceProviderFactory(), ConfigureContainer);

        // Logging
        builder.Logging
#if DEBUG
            .AddDebug()
#endif
#if ANDROID && !DEBUG
            .AddAndroidLogger(static options => options.ShortCategory = true)
#endif
            .AddFileLogger(static options =>
            {
#if ANDROID
                options.Directory = Path.Combine(AndroidHelper.GetExternalFilesDir(), "log");
#endif
                options.RetainDays = 7;
            })
            .AddFilter(typeof(MauiProgram).Namespace, LogLevel.Debug);

        // Config DataMapper
        SqlMapperConfig.Default.ConfigureTypeHandlers(config =>
        {
            config[typeof(DateTime)] = new DateTimeTypeHandler();
            config[typeof(Guid)] = new GuidTypeHandler();
        });

        // Config Rest
        RestConfig.Default.UseJsonSerializer(config =>
        {
            config.Converters.Add(new Template.MobileApp.Helpers.Json.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        // Config App Center
        if (!String.IsNullOrEmpty(Variants.AppCenterSecret()))
        {
            Microsoft.AppCenter.AppCenter.Start(
                Variants.AppCenterSecret(),
                typeof(Microsoft.AppCenter.Analytics.Analytics),
                typeof(Microsoft.AppCenter.Crashes.Crashes));
        }

        // Crash dump
        CrashReport.Start();

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

        // MauiComponents
#if ANDROID
        config.AddComponentsDialog(c =>
        {
            var resources = Application.Current!.Resources;
            c.IndicatorColor = resources.FindResource<Color>("BlueAccent2");
            c.LoadingMessageBackgroundColor = Colors.White;
            c.LoadingMessageColor = Colors.Black;
            c.LoadingMessageFontSize = 28;
            c.ProgressValueColor = Colors.Black;
            c.ProgressAreaBackgroundColor = Colors.White;
            c.ProgressCircleColor1 = resources.FindResource<Color>("BlueAccent2");
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
        config.AddComponentsPopup(static c => c.AutoRegister(ViewRegistry.DialogSource()));
        config.AddComponentsPopupPlugin<PopupFocusPlugin>();
        config.AddComponentsSerializer();
        config.AddComponentsScreen();
        config.AddComponentsSpeech();
        config.AddComponentsLocation();

        // Navigator
        config.AddNavigator(c =>
        {
            c.UseMauiNavigationProvider();
            c.AddResolverPlugin();
            c.AddPlugin<NavigationFocusPlugin>();
            c.UseIdViewMapper(static m => m.AutoRegister(ViewRegistry.ViewSource()));
        });

        // Components
        config.BindSingleton<IStorageManager, StorageManager>();

        // State
        config.BindSingleton<ApplicationState>();
        config.BindSingleton<Session>();
        config.BindSingleton<Settings>();

        // Service
        config.BindSingleton(p =>
        {
            var storage = p.GetRequiredService<IStorageManager>();
            return new DataServiceOptions
            {
#if DEBUG
                Path = Path.Combine(storage.PublicFolder, "data.db")
#else
                Path = Path.Combine(storage.PrivateFolder, "data.db")
#endif
            };
        });
        config.BindSingleton<DataService>();

        config.BindSingleton<NetworkService>();

        // Usecase
        config.BindSingleton<NetworkOperator>();

        config.BindSingleton<SampleUsecase>();
        config.BindSingleton<CognitiveUsecase>();

        // Startup
        config.BindSingleton<IMauiInitializeService, ApplicationInitializer>();
    }
}
