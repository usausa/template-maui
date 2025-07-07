namespace Template.MobileApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using BarcodeScanning;

using CommunityToolkit.Maui;

using Fonts;

using Indiko.Maui.Controls.Markdown;

using MauiComponents.Resolver;

using Microsoft.Maui.LifecycleEvents;

using Plugin.Maui.Audio;
#if DEBUG
using Plugin.Maui.DebugRainbows;
#endif

using Rester;

using Shiny;

using SkiaSharp.Views.Maui.Controls.Hosting;

using Smart.Data.Mapper;
using Smart.Resolver;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components;
using Template.MobileApp.Extender;
using Template.MobileApp.Helpers;
using Template.MobileApp.Helpers.Data;
using Template.MobileApp.Modules;
using Template.MobileApp.Providers;
using Template.MobileApp.Services;
using Template.MobileApp.Shell;
using Template.MobileApp.Usecase;

public static partial class MauiProgram
{
    public static MauiApp CreateMauiApp() =>
        MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            //.ConfigureDebug()
            .ConfigureFonts(ConfigureFonts)
            .ConfigureLifecycleEvents(ConfigureLifecycle)
            .ConfigureEssentials(ConfigureEssentials)
            .ConfigureLogging()
            .ConfigureGlobalSettings()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCamera()
            .UseBarcodeScanning()
            .UseShiny()
            .UseMarkdownView()
            .UseMauiServices()
            .UseMauiComponents()
            .UseCommunityToolkitServices()
            .UseCustomView()
            .ConfigureComponents()
            .ConfigureHttpClient()
            .ConfigureContainer()
            .Build();

    // ------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureDebug(this MauiAppBuilder builder)
    {
#if DEBUG
        builder
            .UseDebugRainbows(new DebugRainbowsOptions
            {
                ShowRainbows = true,
                ShowGrid = true,
                HorizontalItemSize = 20,
                VerticalItemSize = 20,
                MajorGridLineInterval = 4,
                MajorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 0.5, Width = 3 },
                MinorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 0.25, Width = 1 },
                GridOrigin = DebugGridOrigin.TopLeft
            });
#endif

        return builder;
    }

    // ------------------------------------------------------------
    // Logging
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureLogging(this MauiAppBuilder builder)
    {
        // Debug
#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Android
#if ANDROID
        builder.Logging.AddAndroidLogger(static options => options.ShortCategory = true);
#endif
        // File
        builder.Logging.AddFileLogger(static options =>
            {
#if ANDROID
                options.Directory = Path.Combine(AndroidHelper.GetExternalFilesDir(), "log");
#endif
                options.RetainDays = 7;
            })
            .AddFilter(typeof(MauiProgram).Namespace, LogLevel.Debug);

        return builder;
    }

    // ------------------------------------------------------------
    // Application
    // ------------------------------------------------------------

    // ReSharper disable UnusedParameter.Local
    private static void ConfigureLifecycle(ILifecycleBuilder effects)
    {
    }
    // ReSharper restore UnusedParameter.Local

    // ReSharper disable UnusedParameter.Local
    private static void ConfigureEssentials(IEssentialsBuilder config)
    {
    }
    // ReSharper restore UnusedParameter.Local

    private static MauiAppBuilder ConfigureGlobalSettings(this MauiAppBuilder builder)
    {
        // Config DataMapper
        SqlMapperConfig.Default.ConfigureTypeHandlers(static config =>
        {
            config[typeof(DateTime)] = new DateTimeTypeHandler();
            config[typeof(Guid)] = new GuidTypeHandler();
        });

        // Config Rest
        RestConfig.Default.UseJsonSerializer(static config =>
        {
            config.Converters.Add(new Template.MobileApp.Helpers.Json.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        // TODO App center alternative

        // Crash dump
        CrashReport.Start();

        return builder;
    }

    private static MauiAppBuilder UseCustomView(this MauiAppBuilder builder)
    {
        // Behaviors
        builder.ConfigureCustomBehaviors(static options =>
        {
            options.DisableShowSoftInputOnFocus = false;
        });

        // Busy
        builder.UseCustomBusyOverlay();

        return builder;
    }

    // ------------------------------------------------------------
    // Design
    // ------------------------------------------------------------

    private static void ConfigureFonts(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
        fonts.AddFont("MaterialIcons-Regular.ttf", MaterialIcons.FontFamily);
        fonts.AddFont("851Gkktt_005.ttf", "Gkktt");
        fonts.AddFont("DSEG7Classic-Regular.ttf", "DSEG7");
    }

    private static void ConfigureDialogDesign(DialogConfig config)
    {
        var resources = Application.Current!.Resources;
        config.IndicatorColor = resources.FindResource<Color>("BlueAccent2");
        config.LoadingMessageFontSize = 28;
        config.ProgressCircleColor1 = resources.FindResource<Color>("BlueAccent2");
        config.ProgressCircleColor2 = resources.FindResource<Color>("GrayLighten2");

        // Avoiding conflicts with progress
        config.LockBackgroundColor = Colors.Transparent;
        config.LoadingBackgroundColor = Colors.Transparent;
        config.ProgressBackgroundColor = Colors.Transparent;
    }

    // ------------------------------------------------------------
    // Components
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureComponents(this MauiAppBuilder builder)
    {
        // Components
        builder.Services.AddBluetoothLE();
        builder.Services.AddBleHostedCharacteristic<UserCharacteristic>();
        builder.Services.AddBluetoothLeHosting();

        return builder;
    }

    private static MauiAppBuilder ConfigureContainer(this MauiAppBuilder builder)
    {
        builder.ConfigureContainer(new SmartServiceProviderFactory(), ConfigureContainer);

        return builder;
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
        config.AddComponentsDialog(static c =>
        {
            ConfigureDialogDesign(c);
            c.EnablePromptEnterAction = true;
            c.EnablePromptSelectAll = true;
        });
        config.AddComponentsPopup(static c => c.AutoRegister(DialogSource()));
        config.AddComponentsPopupPlugin<PopupFocusPlugin>();
        config.AddComponentsSerializer();
        config.AddComponentsScreen();
        config.AddComponentsLocation();
        config.AddComponentsSpeech();

        // Messenger
        config.BindSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigator
        config.AddNavigator(static c =>
        {
            c.UseMauiNavigationProvider();
            c.AddResolverPlugin();
            c.AddPlugin<NavigationFocusPlugin>();
            c.UseIdViewMapper(static m => m.AutoRegister(ViewSource()));
        });

        // Components
        config.BindSingleton<IStorageManager, StorageManager>();
        config.BindSingleton<INfcReader, NfcReader>();
        config.BindSingleton<INoiseMonitor, NoiseMonitor>();
        config.BindSingleton<IOcrReader, OcrReader>();

        config.BindSingleton(AudioManager.Current);

        // State
        config.BindSingleton<DeviceState>();
        config.BindSingleton<Session>();
        config.BindSingleton<Settings>();

        // Service
        config.BindSingleton(static p =>
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

        config.BindSingleton<NetworkUsecase>();
        config.BindSingleton<CognitiveUsecase>();
        config.BindSingleton<SampleUsecase>();

        // Startup
        config.BindSingleton<IMauiInitializeService, ApplicationInitializer>();
    }

    // ------------------------------------------------------------
    // View & Dialog
    // ------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    [PopupSource]
    public static partial IEnumerable<KeyValuePair<DialogId, Type>> DialogSource();
}
