namespace Template.MobileApp.Converters;

using Fonts;

public enum HomeDeviceTextPart
{
    Icon,
    State,
    Detail
}

// 値は Kind・IsOn・Level の順
public sealed class HomeDeviceTextConverter : IMultiValueConverter
{
    public HomeDeviceTextPart Part { get; set; }

    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture) =>
        values is [HomeDeviceKind kind, bool on, double level, ..]
            ? Part switch
            {
                HomeDeviceTextPart.Icon => Icon(kind, on),
                HomeDeviceTextPart.State => State(kind, on),
                _ => Detail(kind, on, level)
            }
            : string.Empty;

    private static string Icon(HomeDeviceKind kind, bool on) => kind switch
    {
        HomeDeviceKind.Light => MaterialIcons.Lightbulb,
        HomeDeviceKind.Blinds => on ? MaterialIcons.Blinds : MaterialIcons.Blinds_closed,
        HomeDeviceKind.Lock => on ? MaterialIcons.Verified_user : MaterialIcons.Lock_open,
        HomeDeviceKind.Lamp => MaterialIcons.Light,
        HomeDeviceKind.Fan => MaterialIcons.Air,
        _ => MaterialIcons.Ac_unit
    };

    private static string State(HomeDeviceKind kind, bool on) => kind switch
    {
        HomeDeviceKind.Blinds => on ? "開" : "閉",
        HomeDeviceKind.Lock => on ? "施錠" : "解錠",
        _ => on ? "オン" : "オフ"
    };

    private static string Detail(HomeDeviceKind kind, bool on, double level) => (kind, on) switch
    {
        (HomeDeviceKind.Blinds, true) => String.Format(CultureInfo.InvariantCulture, "開度{0:0}%", level),
        (HomeDeviceKind.Blinds, false) => "全閉",
        (HomeDeviceKind.Lock, true) => "安全",
        (HomeDeviceKind.Lock, false) => "要確認",
        (HomeDeviceKind.Fan, true) => String.Format(CultureInfo.InvariantCulture, "風量{0:0}", level),
        (HomeDeviceKind.AirConditioner, true) => String.Format(CultureInfo.InvariantCulture, "冷房{0:0.0}°C", level),
        _ => "停止"
    };

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
