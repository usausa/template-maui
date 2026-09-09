namespace Template.MobileApp.Markup;

using Fonts;

using Microsoft.Extensions.DependencyInjection;

// XAML から x:Static で参照するアイコン。FontImageSource をここで生成して共有し、
// 起動時の温めも同じ定義を対象にする。全て MaterialIcons のグリフ。
// グリフのビットマップはサイズと色を含むキーでキャッシュされるため、
// 同じ指定の組み合わせは 1 つにまとめる
public static class AppIcons
{
    private const double SmallSize = 18d;

    private const double MenuSize = 24d;

    private const double MoneySize = 28d;

    private const double LargeSize = 36d;

    // 一度に投げる要求数。初期表示の後ろで動かす分が UI スレッドを長く占有しないように区切る
    private const int BatchSize = 16;

    private static readonly List<FontImageSource> Sources = [];

    //--------------------------------------------------------------------------------
    // Menu (Material / 24 / White)
    //--------------------------------------------------------------------------------

    public static readonly FontImageSource AccountBalanceWallet = Create(MaterialIcons.Account_balance_wallet, MenuSize, Colors.White);
    public static readonly FontImageSource AccountCircle = Create(MaterialIcons.Account_circle, MenuSize, Colors.White);
    public static readonly FontImageSource AccountTree = Create(MaterialIcons.Account_tree, MenuSize, Colors.White);
    public static readonly FontImageSource Animation = Create(MaterialIcons.Animation, MenuSize, Colors.White);
    public static readonly FontImageSource Apps = Create(MaterialIcons.Apps, MenuSize, Colors.White);
    public static readonly FontImageSource Archive = Create(MaterialIcons.Archive, MenuSize, Colors.White);
    public static readonly FontImageSource ArrowBack = Create(MaterialIcons.Arrow_back, MenuSize, Colors.White);
    public static readonly FontImageSource ArrowDownward = Create(MaterialIcons.Arrow_downward, MenuSize, Colors.White);
    public static readonly FontImageSource ArrowForward = Create(MaterialIcons.Arrow_forward, MenuSize, Colors.White);
    public static readonly FontImageSource ArrowUpward = Create(MaterialIcons.Arrow_upward, MenuSize, Colors.White);
    public static readonly FontImageSource Article = Create(MaterialIcons.Article, MenuSize, Colors.White);
    public static readonly FontImageSource Assessment = Create(MaterialIcons.Assessment, MenuSize, Colors.White);
    public static readonly FontImageSource Attractions = Create(MaterialIcons.Attractions, MenuSize, Colors.White);
    public static readonly FontImageSource AutoAwesome = Create(MaterialIcons.Auto_awesome, MenuSize, Colors.White);
    public static readonly FontImageSource AvTimer = Create(MaterialIcons.Av_timer, MenuSize, Colors.White);
    public static readonly FontImageSource BarChart = Create(MaterialIcons.Bar_chart, MenuSize, Colors.White);
    public static readonly FontImageSource Block = Create(MaterialIcons.Block, MenuSize, Colors.White);
    public static readonly FontImageSource Bluetooth = Create(MaterialIcons.Bluetooth, MenuSize, Colors.White);
    public static readonly FontImageSource BluetoothConnected = Create(MaterialIcons.Bluetooth_connected, MenuSize, Colors.White);
    public static readonly FontImageSource BluetoothSearching = Create(MaterialIcons.Bluetooth_searching, MenuSize, Colors.White);
    public static readonly FontImageSource BlurOn = Create(MaterialIcons.Blur_on, MenuSize, Colors.White);
    public static readonly FontImageSource Bolt = Create(MaterialIcons.Bolt, MenuSize, Colors.White);
    public static readonly FontImageSource BorderStyle = Create(MaterialIcons.Border_style, MenuSize, Colors.White);
    public static readonly FontImageSource Brush = Create(MaterialIcons.Brush, MenuSize, Colors.White);
    public static readonly FontImageSource Calculate = Create(MaterialIcons.Calculate, MenuSize, Colors.White);
    public static readonly FontImageSource CalendarMonth = Create(MaterialIcons.Calendar_month, MenuSize, Colors.White);
    public static readonly FontImageSource Category = Create(MaterialIcons.Category, MenuSize, Colors.White);
    public static readonly FontImageSource CellTower = Create(MaterialIcons.Cell_tower, MenuSize, Colors.White);
    public static readonly FontImageSource Chat = Create(MaterialIcons.Chat, MenuSize, Colors.White);
    public static readonly FontImageSource Cloud = Create(MaterialIcons.Cloud, MenuSize, Colors.White);
    public static readonly FontImageSource CloudUpload = Create(MaterialIcons.Cloud_upload, MenuSize, Colors.White);
    public static readonly FontImageSource Crop = Create(MaterialIcons.Crop, MenuSize, Colors.White);
    public static readonly FontImageSource Dashboard = Create(MaterialIcons.Dashboard, MenuSize, Colors.White);
    public static readonly FontImageSource Delete = Create(MaterialIcons.Delete, MenuSize, Colors.White);
    public static readonly FontImageSource Devices = Create(MaterialIcons.Devices, MenuSize, Colors.White);
    public static readonly FontImageSource DirectionsRun = Create(MaterialIcons.Directions_run, MenuSize, Colors.White);
    public static readonly FontImageSource DocumentScanner = Create(MaterialIcons.Document_scanner, MenuSize, Colors.White);
    public static readonly FontImageSource Download = Create(MaterialIcons.Download, MenuSize, Colors.White);
    public static readonly FontImageSource DragIndicator = Create(MaterialIcons.Drag_indicator, MenuSize, Colors.White);
    public static readonly FontImageSource Draw = Create(MaterialIcons.Draw, MenuSize, Colors.White);
    public static readonly FontImageSource ErrorOutline = Create(MaterialIcons.Error_outline, MenuSize, Colors.White);
    public static readonly FontImageSource Explore = Create(MaterialIcons.Explore, MenuSize, Colors.White);
    public static readonly FontImageSource Extension = Create(MaterialIcons.Extension, MenuSize, Colors.White);
    public static readonly FontImageSource Face = Create(MaterialIcons.Face, MenuSize, Colors.White);
    public static readonly FontImageSource FactCheck = Create(MaterialIcons.Fact_check, MenuSize, Colors.White);
    public static readonly FontImageSource FilterList = Create(MaterialIcons.Filter_list, MenuSize, Colors.White);
    public static readonly FontImageSource Fingerprint = Create(MaterialIcons.Fingerprint, MenuSize, Colors.White);
    public static readonly FontImageSource FlightTakeoff = Create(MaterialIcons.Flight_takeoff, MenuSize, Colors.White);
    public static readonly FontImageSource Flip = Create(MaterialIcons.Flip, MenuSize, Colors.White);
    public static readonly FontImageSource FontDownload = Create(MaterialIcons.Font_download, MenuSize, Colors.White);
    public static readonly FontImageSource FormatPaint = Create(MaterialIcons.Format_paint, MenuSize, Colors.White);
    public static readonly FontImageSource Forum = Create(MaterialIcons.Forum, MenuSize, Colors.White);
    public static readonly FontImageSource GridOn = Create(MaterialIcons.Grid_on, MenuSize, Colors.White);
    public static readonly FontImageSource GridView = Create(MaterialIcons.Grid_view, MenuSize, Colors.White);
    public static readonly FontImageSource Handyman = Create(MaterialIcons.Handyman, MenuSize, Colors.White);
    public static readonly FontImageSource Hearing = Create(MaterialIcons.Hearing, MenuSize, Colors.White);
    public static readonly FontImageSource HourglassEmpty = Create(MaterialIcons.Hourglass_empty, MenuSize, Colors.White);
    public static readonly FontImageSource Image = Create(MaterialIcons.Image, MenuSize, Colors.White);
    public static readonly FontImageSource Info = Create(MaterialIcons.Info, MenuSize, Colors.White);
    public static readonly FontImageSource InsertChart = Create(MaterialIcons.Insert_chart, MenuSize, Colors.White);
    public static readonly FontImageSource Insights = Create(MaterialIcons.Insights, MenuSize, Colors.White);
    public static readonly FontImageSource Language = Create(MaterialIcons.Language, MenuSize, Colors.White);
    public static readonly FontImageSource Layers = Create(MaterialIcons.Layers, MenuSize, Colors.White);
    public static readonly FontImageSource ListAlt = Create(MaterialIcons.List_alt, MenuSize, Colors.White);
    public static readonly FontImageSource LiveTv = Create(MaterialIcons.Live_tv, MenuSize, Colors.White);
    public static readonly FontImageSource LocationOn = Create(MaterialIcons.Location_on, MenuSize, Colors.White);
    public static readonly FontImageSource Lock = Create(MaterialIcons.Lock, MenuSize, Colors.White);
    public static readonly FontImageSource Login = Create(MaterialIcons.Login, MenuSize, Colors.White);
    public static readonly FontImageSource Logout = Create(MaterialIcons.Logout, MenuSize, Colors.White);
    public static readonly FontImageSource Mail = Create(MaterialIcons.Mail, MenuSize, Colors.White);
    public static readonly FontImageSource Map = Create(MaterialIcons.Map, MenuSize, Colors.White);
    public static readonly FontImageSource Memory = Create(MaterialIcons.Memory, MenuSize, Colors.White);
    public static readonly FontImageSource Mood = Create(MaterialIcons.Mood, MenuSize, Colors.White);
    public static readonly FontImageSource MoreHoriz = Create(MaterialIcons.More_horiz, MenuSize, Colors.White);
    public static readonly FontImageSource Movie = Create(MaterialIcons.Movie, MenuSize, Colors.White);
    public static readonly FontImageSource MovieFilter = Create(MaterialIcons.Movie_filter, MenuSize, Colors.White);
    public static readonly FontImageSource Navigation = Create(MaterialIcons.Navigation, MenuSize, Colors.White);
    public static readonly FontImageSource Nfc = Create(MaterialIcons.Nfc, MenuSize, Colors.White);
    public static readonly FontImageSource Opacity = Create(MaterialIcons.Opacity, MenuSize, Colors.White);
    public static readonly FontImageSource Palette = Create(MaterialIcons.Palette, MenuSize, Colors.White);
    public static readonly FontImageSource People = Create(MaterialIcons.People, MenuSize, Colors.White);
    public static readonly FontImageSource Pets = Create(MaterialIcons.Pets, MenuSize, Colors.White);
    public static readonly FontImageSource PhotoCamera = Create(MaterialIcons.Photo_camera, MenuSize, Colors.White);
    public static readonly FontImageSource PictureAsPdf = Create(MaterialIcons.Picture_as_pdf, MenuSize, Colors.White);
    public static readonly FontImageSource PointOfSale = Create(MaterialIcons.Point_of_sale, MenuSize, Colors.White);
    public static readonly FontImageSource QrCode = Create(MaterialIcons.Qr_code, MenuSize, Colors.White);
    public static readonly FontImageSource QrCodeScanner = Create(MaterialIcons.Qr_code_scanner, MenuSize, Colors.White);
    public static readonly FontImageSource Radar = Create(MaterialIcons.Radar, MenuSize, Colors.White);
    public static readonly FontImageSource Refresh = Create(MaterialIcons.Refresh, MenuSize, Colors.White);
    public static readonly FontImageSource RotateRight = Create(MaterialIcons.Rotate_right, MenuSize, Colors.White);
    public static readonly FontImageSource Schedule = Create(MaterialIcons.Schedule, MenuSize, Colors.White);
    public static readonly FontImageSource Science = Create(MaterialIcons.Science, MenuSize, Colors.White);
    public static readonly FontImageSource Sell = Create(MaterialIcons.Sell, MenuSize, Colors.White);
    public static readonly FontImageSource Sensors = Create(MaterialIcons.Sensors, MenuSize, Colors.White);
    public static readonly FontImageSource Settings = Create(MaterialIcons.Settings, MenuSize, Colors.White);
    public static readonly FontImageSource ShowChart = Create(MaterialIcons.Show_chart, MenuSize, Colors.White);
    public static readonly FontImageSource SmartToy = Create(MaterialIcons.Smart_toy, MenuSize, Colors.White);
    public static readonly FontImageSource Speed = Create(MaterialIcons.Speed, MenuSize, Colors.White);
    public static readonly FontImageSource SportsEsports = Create(MaterialIcons.Sports_esports, MenuSize, Colors.White);
    public static readonly FontImageSource Storage = Create(MaterialIcons.Storage, MenuSize, Colors.White);
    public static readonly FontImageSource Storefront = Create(MaterialIcons.Storefront, MenuSize, Colors.White);
    public static readonly FontImageSource SwapHoriz = Create(MaterialIcons.Swap_horiz, MenuSize, Colors.White);
    public static readonly FontImageSource Sync = Create(MaterialIcons.Sync, MenuSize, Colors.White);
    public static readonly FontImageSource TextFields = Create(MaterialIcons.Text_fields, MenuSize, Colors.White);
    public static readonly FontImageSource Timeline = Create(MaterialIcons.Timeline, MenuSize, Colors.White);
    public static readonly FontImageSource ToggleOn = Create(MaterialIcons.Toggle_on, MenuSize, Colors.White);
    public static readonly FontImageSource TouchApp = Create(MaterialIcons.Touch_app, MenuSize, Colors.White);
    public static readonly FontImageSource Translate = Create(MaterialIcons.Translate, MenuSize, Colors.White);
    public static readonly FontImageSource Tune = Create(MaterialIcons.Tune, MenuSize, Colors.White);
    public static readonly FontImageSource Upload = Create(MaterialIcons.Upload, MenuSize, Colors.White);
    public static readonly FontImageSource VerticalAlignBottom = Create(MaterialIcons.Vertical_align_bottom, MenuSize, Colors.White);
    public static readonly FontImageSource ViewCarousel = Create(MaterialIcons.View_carousel, MenuSize, Colors.White);
    public static readonly FontImageSource ViewList = Create(MaterialIcons.View_list, MenuSize, Colors.White);
    public static readonly FontImageSource ViewQuilt = Create(MaterialIcons.View_quilt, MenuSize, Colors.White);
    public static readonly FontImageSource ViewTimeline = Create(MaterialIcons.View_timeline, MenuSize, Colors.White);
    public static readonly FontImageSource VolumeUp = Create(MaterialIcons.Volume_up, MenuSize, Colors.White);
    public static readonly FontImageSource Web = Create(MaterialIcons.Web, MenuSize, Colors.White);
    public static readonly FontImageSource Widgets = Create(MaterialIcons.Widgets, MenuSize, Colors.White);
    public static readonly FontImageSource Wifi = Create(MaterialIcons.Wifi, MenuSize, Colors.White);
    public static readonly FontImageSource ZoomIn = Create(MaterialIcons.Zoom_in, MenuSize, Colors.White);

    //--------------------------------------------------------------------------------
    // Money (Material / 28 / White)
    //--------------------------------------------------------------------------------

    public static readonly FontImageSource MoneyAccountCircle = Create(MaterialIcons.Account_circle, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyAddCircle = Create(MaterialIcons.Add_circle, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyDocumentScanner = Create(MaterialIcons.Document_scanner, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyKey = Create(MaterialIcons.Key, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyPaid = Create(MaterialIcons.Paid, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyPause = Create(MaterialIcons.Pause, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyPlayArrow = Create(MaterialIcons.Play_arrow, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyReceiptLong = Create(MaterialIcons.Receipt_long, MoneySize, Colors.White);
    public static readonly FontImageSource MoneySend = Create(MaterialIcons.Send, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyShoppingCart = Create(MaterialIcons.Shopping_cart, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyStars = Create(MaterialIcons.Stars, MoneySize, Colors.White);
    public static readonly FontImageSource MoneyViewComfy = Create(MaterialIcons.View_comfy, MoneySize, Colors.White);

    //--------------------------------------------------------------------------------
    // Small (Material / 18 / BlueGrayDarken1)
    //--------------------------------------------------------------------------------

    public static readonly FontImageSource SmallBrightnessHigh = Create(MaterialIcons.Brightness_high, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallBrightnessLow = Create(MaterialIcons.Brightness_low, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallDoNotDisturb = Create(MaterialIcons.Do_not_disturb, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallFlashlightOff = Create(MaterialIcons.Flashlight_off, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallFlashlightOn = Create(MaterialIcons.Flashlight_on, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallInput = Create(MaterialIcons.Input, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallMic = Create(MaterialIcons.Mic, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallRecordVoiceOver = Create(MaterialIcons.Record_voice_over, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallScreenshot = Create(MaterialIcons.Screenshot, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallStayCurrentLandscape = Create(MaterialIcons.Stay_current_landscape, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallStayCurrentPortrait = Create(MaterialIcons.Stay_current_portrait, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallTextFormat = Create(MaterialIcons.Text_format, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallTouchApp = Create(MaterialIcons.Touch_app, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallVibration = Create(MaterialIcons.Vibration, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallVisibility = Create(MaterialIcons.Visibility, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallVisibilityOff = Create(MaterialIcons.Visibility_off, SmallSize, ResourceColor("BlueGrayDarken1"));
    public static readonly FontImageSource SmallVoiceOverOff = Create(MaterialIcons.Voice_over_off, SmallSize, ResourceColor("BlueGrayDarken1"));

    //--------------------------------------------------------------------------------
    // Large (Material / 36 / White)
    //--------------------------------------------------------------------------------

    public static readonly FontImageSource LargeAdd = Create(MaterialIcons.Add, LargeSize, Colors.White);
    public static readonly FontImageSource LargeQrCode = Create(MaterialIcons.Qr_code, LargeSize, Colors.White);

    //--------------------------------------------------------------------------------
    // Screen (画面固有の指定)
    //--------------------------------------------------------------------------------

    public static readonly FontImageSource LoginVisibility = Create(MaterialIcons.Visibility, MenuSize, ResourceColor("GrayDefault"));
    public static readonly FontImageSource LoginVisibilityOff = Create(MaterialIcons.Visibility_off, MenuSize, ResourceColor("LightBlueDarken1"));
    public static readonly FontImageSource MapAdd = Create(MaterialIcons.Add, MenuSize, ResourceColor("GrayDarken3"));
    public static readonly FontImageSource MapCircle = Create(MaterialIcons.Circle, MenuSize, ResourceColor("RedDefault"));
    public static readonly FontImageSource MapHome = Create(MaterialIcons.Home, MenuSize, ResourceColor("GrayDarken3"));
    public static readonly FontImageSource MapLayers = Create(MaterialIcons.Layers, MenuSize, ResourceColor("GrayDarken3"));
    public static readonly FontImageSource MapPentagon = Create(MaterialIcons.Pentagon, MenuSize, ResourceColor("GreenDefault"));
    public static readonly FontImageSource MapRemove = Create(MaterialIcons.Remove, MenuSize, ResourceColor("GrayDarken3"));
    public static readonly FontImageSource MapRoute = Create(MaterialIcons.Route, MenuSize, ResourceColor("BlueDefault"));
    public static readonly FontImageSource ValidationCheck = Create(MaterialIcons.Check_circle, SmallSize, ResourceColor("GreenDefault"));
    public static readonly FontImageSource ValidationError = Create(MaterialIcons.Error, SmallSize, ResourceColor("RedDefault"));
    //--------------------------------------------------------------------------------
    // Warmup
    //--------------------------------------------------------------------------------

    // 最初に表示する画面 (MenuView) の分
    public static readonly FontImageSource[] Startup =
    [
        Widgets,
        Navigation,
        Devices,
        Storage,
        Cloud,
        Layers,
        Palette,
        Insights,
        Science,
        Apps,
        Settings
    ];

    // Typeface はフォント毎に FontManager がキャッシュする。
    // FluentUI は Label.Text で使うため、こちらも生成しておく
    public static void WarmTypefaces(IServiceProvider provider)
    {
        var fontManager = provider.GetRequiredService<IFontManager>();
        fontManager.GetTypeface(Microsoft.Maui.Font.OfSize(MaterialIcons.FontFamily, MenuSize));
        fontManager.GetTypeface(Microsoft.Maui.Font.OfSize(FluentUI.FontFamily, MenuSize));
    }

    // 最初の画面を表示する前に必要な分
    public static ValueTask WarmStartupAsync(IServiceProvider provider) => WarmAsync(provider, Startup);

    // 全て。初期表示を待たせないよう後から呼ぶ
    public static ValueTask WarmAllAsync(IServiceProvider provider) => WarmAsync(provider, Sources);

    private static async ValueTask WarmAsync(IServiceProvider provider, IEnumerable<FontImageSource> sources)
    {
#if ANDROID
        var imageSourceServiceProvider = provider.GetService<IImageSourceServiceProvider>();
        if (imageSourceServiceProvider is null)
        {
            return;
        }

        var context = Android.App.Application.Context;
        var pending = new List<Task>(BatchSize);
        foreach (var source in sources)
        {
            var service = imageSourceServiceProvider.GetRequiredImageSourceService(source);
            pending.Add(service.GetDrawableAsync(source, context));

            if (pending.Count == BatchSize)
            {
                await Task.WhenAll(pending).ConfigureAwait(true);
                pending.Clear();
            }
        }

        if (pending.Count > 0)
        {
            await Task.WhenAll(pending).ConfigureAwait(true);
        }
#endif
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static FontImageSource Create(string glyph, double size, Color color)
    {
        var source = new FontImageSource
        {
            FontFamily = MaterialIcons.FontFamily,
            Glyph = glyph,
            Size = size,
            Color = color
        };
        Sources.Add(source);
        return source;
    }

    private static Color ResourceColor(string key)
    {
        var resources = Application.Current?.Resources;
        if ((resources is not null) && resources.TryGetValue(key, out var value) && (value is Color color))
        {
            return color;
        }

        return Colors.White;
    }
}
