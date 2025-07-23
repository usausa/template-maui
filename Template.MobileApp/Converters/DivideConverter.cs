namespace Template.MobileApp.Converters;

public sealed class DivideConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((value is double doubleValue) && (parameter is double doubleParameter))
        {
            return doubleValue / doubleParameter;
        }
        if ((value is int intValue) && (parameter is int intParameter))
        {
            return intValue / intParameter;
        }

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
