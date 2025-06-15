namespace Template.MobileApp.Converters;

using Template.MobileApp.Domain.FeliCa;

public class SuicaLogDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SuicaLogData log)
        {
            return null;
        }

        return Suica.IsProcessOfSales(log.Process) ? log.DateTime : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
