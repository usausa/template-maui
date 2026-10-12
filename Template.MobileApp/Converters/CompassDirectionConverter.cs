namespace Template.MobileApp.Converters;

public sealed class CompassDirectionConverter : IValueConverter
{
    private static readonly string[] Directions = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double degree)
        {
            return string.Empty;
        }

        var rounded = (int)Math.Round(degree) % 360;
        return (rounded >= 0) && (rounded % 45 == 0) ? Directions[rounded / 45] : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
