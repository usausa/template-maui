namespace Template.MobileApp.Converters;

public class MailDateTimeStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var oneWeekAgo = today.AddDays(-7);

            if (dateTime.Date == today)
            {
                return dateTime.ToString("HH:mm", culture);
            }
            if (dateTime.Date > oneWeekAgo)
            {
                return dateTime.ToString("dddd", culture);
            }
            return dateTime.ToString("MM/dd", culture);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
