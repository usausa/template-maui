namespace Template.MobileApp.Converters;

public sealed class DurationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan duration)
        {
            return value;
        }

        if (duration < TimeSpan.FromMinutes(1))
        {
            return $"{duration.TotalSeconds:F1} s";
        }

        return duration.Days > 0
            ? $"{duration.Days}d {duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"
            : $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
