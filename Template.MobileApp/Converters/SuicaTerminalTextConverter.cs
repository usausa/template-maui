namespace Template.MobileApp.Converters;

using Template.MobileApp.Domain.FeliCa;

public class SuicaTerminalTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte byteValue)
        {
            return Suica.ConvertTerminalString(byteValue);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
