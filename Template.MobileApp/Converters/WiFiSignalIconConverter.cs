namespace Template.MobileApp.Converters;

public sealed class WiFiSignalIconConverter : IValueConverter
{
    public string? Off { get; set; }

    public string? Level0 { get; set; }

    public string? Level1 { get; set; }

    public string? Level2 { get; set; }

    public string? Level3 { get; set; }

    public string? Level4 { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        < 0 => Off,
        0 => Level0,
        1 => Level1,
        2 => Level2,
        3 => Level3,
        int => Level4,
        _ => Off
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
