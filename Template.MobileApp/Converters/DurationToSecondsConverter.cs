namespace Template.MobileApp.Converters;

// Slider は Maximum <= Minimum を許容しないため、未取得時は 1 秒として扱う
public sealed class DurationToSecondsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TimeSpan duration ? Math.Max(1, duration.TotalSeconds) : 1d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
