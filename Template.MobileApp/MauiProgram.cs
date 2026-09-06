namespace Template.MobileApp;

using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using BarcodeScanning;

using BunnyTail.DependencyInjection;

using CommunityToolkit.Maui;

using Fonts;

using Indiko.Maui.Controls.Markdown;

using Maui.PDFView;

using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;

using Plugin.Maui.Audio;
#if false
using Plugin.Maui.DebugRainbows;
#endif

using Rester;

using Shiny;

using SkiaSharp.Views.Maui.Controls.Hosting;

using Smart.Data.Mapper;
using Smart.Mvvm.Resolver;

using Syncfusion.Maui.Toolkit.Hosting;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components;
using Template.MobileApp.Extender;
using Template.MobileApp.Helpers;
using Template.MobileApp.Helpers.Data;
using Template.MobileApp.Modules;
using Template.MobileApp.Providers;
using Template.MobileApp.Services;
using Template.MobileApp.Usecase;

public static partial class MauiProgram
{
    private const string ModulesNamespace = "Template.MobileApp.Modules";

    public static MauiApp CreateMauiApp() =>
        MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseGeneratedServiceProvider()
            .ConfigureDebug()
            .ConfigureFonts(ConfigureFonts)
            .ConfigureLifecycleEvents(ConfigureLifecycleEvents)
            .ConfigureEssentials(ConfigureEssentials)
            .ConfigureLogging()
            .ConfigureGlobalSettings()
            .ConfigureSyncfusionToolkit()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit(ConfigureMauiCommunityToolkit)
            .UseMauiCommunityToolkitCamera()
            .UseMauiCommunityToolkitMediaElement(true)
            .UseMauiMaps()
            .UseBarcodeScanning()
            .UseShiny()
            .UseMarkdownView()
            .UseMauiPdfView()
            .UseMauiServices()
            .UseMauiComponents()
            .UseCommunityToolkitServices()
            .UseCustomView()
            .UseCustomLayouts()
            .BuildApplication();

    // ------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureDebug(this MauiAppBuilder builder)
    {
#if DEBUG
        AppContext.SetSwitch("HybridWebView.InvokeJavaScriptThrowsExceptions", true);
        builder.Services.AddHybridWebViewDeveloperTools();

#if false
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

    private static void ConfigureLifecycleEvents(ILifecycleBuilder effects)
    {
        // プラットフォーム固有ライフサイクルのフック例。挙動は変えずログ出力のみ行う
        // (確認は adb logcat -s AppLifecycle)
#if ANDROID
        effects.AddAndroid(static android => android
            .OnCreate(static (activity, _) => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnCreate)))
            .OnStart(static activity => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnStart)))
            .OnResume(static activity => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnResume)))
            .OnPause(static activity => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnPause)))
            .OnStop(static activity => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnStop)))
            .OnDestroy(static activity => LogLifecycleEvent(activity, nameof(AndroidLifecycle.OnDestroy))));
#endif
    }

#if ANDROID
    private static void LogLifecycleEvent(Android.App.Activity activity, string eventName) =>
        Android.Util.Log.Debug("AppLifecycle", $"{activity.LocalClassName} {eventName}");
#endif

    // ReSharper disable UnusedParameter.Local
    private static void ConfigureEssentials(IEssentialsBuilder config)
    {
    }
    // ReSharper restore UnusedParameter.Local

    private static void ConfigureMauiCommunityToolkit(Options options)
    {
        options.SetPopupDefaults(new DefaultPopupSettings
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            Padding = 0
        });
        options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            Shadow = null,
            Shape = null
        });
    }

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

        return builder;
    }

    private static MauiAppBuilder UseCustomLayouts(this MauiAppBuilder builder)
    {
        // ILayoutManagerFactory: レイアウト型ごとにマネージャを DI で差し替えるフック
        // (Layouts/AppLayoutManagerFactory 参照。CascadeStackLayout のみ対象で他は既定のまま)
        builder.Services.AddSingleton<ILayoutManagerFactory, Layouts.AppLayoutManagerFactory>();
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
        fonts.AddFont("Oxanium-Regular.ttf", "OxaniumRegular");
        fonts.AddFont("851Gkktt_005.ttf", "Gkktt");
        fonts.AddFont("DSEG7Classic-Regular.ttf", "DSEG7");
        fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
        fonts.AddFont("NotoSerifJP-Medium.ttf", "NotoSerifJP");
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

    private static MauiAppBuilder UseGeneratedServiceProvider(this MauiAppBuilder builder)
    {
        builder.ConfigureContainer(
            new GeneratedServiceProviderFactory(static options => options.TrackTransientDisposables = false),
            ConfigureComponents);
        return builder;
    }

    private static void ConfigureComponents(IServiceCollection services)
    {
        // View & ViewModel
        services.AddTransient<MainPage>();
        services.AddTransient<MainPageViewModel>();
        services.AddViews();
        services.AddViewModels();
        services.AddContexts();

        // MauiComponents
        services.AddComponentsDialog(static c =>
        {
            ConfigureDialogDesign(c);
            c.EnablePromptEnterAction = true;
            c.EnablePromptSelectAll = true;
        });
        services.AddComponentsPopup(static c => c.AutoRegister(DialogSource()));
        services.AddComponentsPopupPlugin<PopupFocusPlugin>();
        services.AddComponentsScreen();
        services.AddComponentsLocation();
        services.AddComponentsSpeech();

        // Messenger
        services.AddSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigator
        services.AddNavigator(static (_, config) =>
        {
            config.UseMauiNavigationProvider();
            config.AddPlugin<NavigationFocusPlugin>();
            config.UseIdViewMapper(static m => m.AutoRegister(ViewSource()));
        });

        // Components
        services.AddSingleton<IStorageManager, StorageManager>();
        services.AddSingleton<IBluetoothSerialFactory, BluetoothSerialFactory>();
        services.AddSingleton<INfcReader, NfcReader>();
        services.AddSingleton<INoiseMonitor, NoiseMonitor>();
        services.AddSingleton<IOcrReader, OcrReader>();
        services.AddSingleton<IActivityRecognizer, ActivityRecognizer>();

        services.AddSingleton(AudioManager.Current);

        // Bluetooth
        services.AddBluetoothLE();
        services.AddBluetoothLeHosting();
        services.AddSingleton<UserCharacteristic>();

        // Resource
        services.AddSingleton<ResourceDictionary>(static _ => Application.Current!.Resources);

        // State
        services.AddSingleton(BusyState.Default);
        services.AddSingleton<StartupState>();
        services.AddSingleton<DeviceState>();
        services.AddSingleton<Session>();
        services.AddSingleton<Settings>();

        // HttpClient
        services
            .AddHttpClient(ApiNames.Default, SetupHttpClient)
            .ConfigurePrimaryHttpMessageHandler(CreateHttpMessageHandler)
            .AddHttpMessageHandler<ApiDelegatingHandler>();
        services
            .AddHttpClient(ApiNames.Transfer, SetupTransferHttpClient)
            .ConfigurePrimaryHttpMessageHandler(CreateHttpMessageHandler)
            .AddHttpMessageHandler<ApiDelegatingHandler>();
        services.AddTransient<ApiDelegatingHandler>();
        services.AddSingleton<ApiContext>();

        // Service
        services.AddSingleton(static p =>
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
        services.AddSingleton<DataService>();

        services.AddSingleton<HttpService>();

        // サンプルデータ生成器 (VMからのnew直生成を避けDI注入の見本とする)
        services.AddSingleton<IScheduleEventProvider, ScheduleService>();
        services.AddSingleton<HolidayService>();

        // Usecase
        services.AddSingleton<INetworkInteraction, DialogNetworkInteraction>();
        services.AddSingleton<NetworkOperator>();
        services.AddSingleton<NetworkUsecase>();
        services.AddSingleton<CognitiveUsecase>();

        // Models
        services.AddSingleton(new ActivityCalculator(0.0005, 65, 0.6));
        services.AddSingleton<ScpService>();
    }

    // ------------------------------------------------------------
    // Network
    // ------------------------------------------------------------

    private static void SetupHttpClient(IServiceProvider provider, HttpClient client)
    {
        client.BaseAddress = provider.GetRequiredService<ApiContext>().BaseAddress;
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    }

    private static void SetupTransferHttpClient(IServiceProvider provider, HttpClient client)
    {
        client.BaseAddress = provider.GetRequiredService<ApiContext>().BaseAddress;
        client.Timeout = TimeSpan.FromMinutes(10);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    }

    private static HttpMessageHandler CreateHttpMessageHandler() =>
        new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(1)
        };

    // ------------------------------------------------------------
    // Build
    // ------------------------------------------------------------

    private static MauiApp BuildApplication(this MauiAppBuilder builder)
    {
        var app = builder.Build();

        var services = app.Services;

        // Setup provider
        ResolveProvider.Default.Provider = services;

#if DEBUG
        // Diagnostics for GeneratedServiceProvider
        if (services is GeneratedServiceProvider generatedProvider)
        {
            foreach (var line in BunnyTail.DependencyInjection.Diagnostics.ServiceFactoryReportExtensions.DescribeRuntimeFallbacks(generatedProvider).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }

        // Setup navigator
        var navigator = services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine($"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };
#endif

        // Initial setting
        var settings = services.GetRequiredService<Settings>();

        if (String.IsNullOrEmpty(settings.ApiEndPoint) && !String.IsNullOrEmpty(EmbeddedProperty.ApiEndPoint))
        {
            settings.ApiEndPoint = EmbeddedProperty.ApiEndPoint;
        }

        if (String.IsNullOrEmpty(settings.UniqueId))
        {
            var uniqueId = Guid.NewGuid();
            settings.UniqueId = uniqueId.ToString();
        }

        var apiContext = services.GetRequiredService<ApiContext>();
        if (!String.IsNullOrEmpty(settings.ApiEndPoint))
        {
            apiContext.BaseAddress = new Uri(settings.ApiEndPoint);
        }

        return app;
    }

    // ------------------------------------------------------------
    // View & ViewModel
    // ------------------------------------------------------------

    // ReSharper disable UnusedMethodReturnValue.Local
    [ComponentRegistration(Lifetime.Transient, "View$", Namespace = ModulesNamespace)]
    private static partial IServiceCollection AddViews(this IServiceCollection services);

    [ComponentRegistration(Lifetime.Transient, "ViewModel$", Namespace = ModulesNamespace)]
    private static partial IServiceCollection AddViewModels(this IServiceCollection services);

    [ComponentRegistration(Lifetime.Transient, "Context$", Namespace = ModulesNamespace)]
    private static partial IServiceCollection AddContexts(this IServiceCollection services);
    // ReSharper restore UnusedMethodReturnValue.Local

    // ------------------------------------------------------------
    // Navigation
    // ------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    [PopupSource]
    public static partial IEnumerable<KeyValuePair<DialogId, Type>> DialogSource();
}
