namespace Template.MobileApp.Converters;

// A span as a playback reading like "3:21", the minutes are not padded
public sealed class MinuteSecondConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan span)
        {
            return value;
        }

        return $"{(int)span.TotalMinutes}:{Math.Abs(span.Seconds):D2}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
