namespace Template.MobileApp.Converters;

public class SuicaLogDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SuicaLogData log)
        {
            return null;
        }

        return SuicaLogic.IsProcessOfSales(log.Process) ? log.DateTime : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
