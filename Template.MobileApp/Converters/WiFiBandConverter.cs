namespace Template.MobileApp.Converters;

// 周波数 (MHz) を帯域の文言へ
public sealed class WiFiBandConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        >= 5925 => "6 GHz",
        >= 4900 => "5 GHz",
        >= 2400 => "2.4 GHz",
        _ => string.Empty
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
