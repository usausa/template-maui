namespace Template.MobileApp.Converters;

public sealed class CompassDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double degree)
        {
            return string.Empty;
        }

        return ((int)Math.Round(degree) % 360) switch
        {
            0 => "N",
            90 => "E",
            180 => "S",
            270 => "W",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
