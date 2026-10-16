namespace Template.MobileApp.Converters;

public sealed class RuntimeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int minutes)
        {
            return minutes >= 60 ? $"{minutes / 60}h {minutes % 60:D2}m" : $"{minutes}m";
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
