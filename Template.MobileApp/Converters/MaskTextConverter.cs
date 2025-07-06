namespace Template.MobileApp.Converters;

public sealed class MaskTextConverter : IValueConverter
{
    public char MaskChar { get; set; } = '*';

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
        {
            return null;
        }

        return new string(MaskChar, text.Length);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
