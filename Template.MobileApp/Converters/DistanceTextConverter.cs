namespace Template.MobileApp.Converters;

public sealed class DistanceTextConverter : IValueConverter
{
    public string KilometerFormat { get; set; } = "{0:F1} km";

    public string MeterFormat { get; set; } = "{0} m";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int meters)
        {
            return value;
        }

        return meters >= 1000
            ? String.Format(culture, KilometerFormat, meters / 1000d)
            : String.Format(culture, MeterFormat, meters);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
