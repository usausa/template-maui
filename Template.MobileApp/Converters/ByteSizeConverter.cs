namespace Template.MobileApp.Converters;

public sealed class ByteSizeConverter : IValueConverter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long size)
        {
            return value;
        }

        double result = size;
        var unit = 0;
        while ((result >= 1024) && (unit < Units.Length - 1))
        {
            result /= 1024;
            unit++;
        }

        return unit == 0 ? $"{size:N0} {Units[unit]}" : $"{result:F1} {Units[unit]}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
