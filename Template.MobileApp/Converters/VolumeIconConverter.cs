namespace Template.MobileApp.Converters;

public sealed class VolumeIconConverter : IValueConverter
{
    public string? Off { get; set; }

    public string? Low { get; set; }

    public string? High { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        double volume => Math.Round(volume * 100) switch
        {
            <= 0 => Off,
            < 50 => Low,
            _ => High
        },
        _ => High
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
