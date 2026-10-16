namespace Template.MobileApp.Converters;

public sealed class DurationToSecondsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TimeSpan duration ? Math.Max(1, duration.TotalSeconds) : 1d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
