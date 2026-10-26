namespace Template.MobileApp.Converters;

public sealed class SingleLineConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text
            ? String.Join(' ', text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
