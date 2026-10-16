namespace Template.MobileApp.Converters;

public sealed class RelativeDayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var today = DateTime.Today;
            if (date.Date == today)
            {
                return "今日";
            }
            if (date.Date == today.AddDays(-1))
            {
                return "昨日";
            }
            return date.ToString("MM/dd (ddd)", culture);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
