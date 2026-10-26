namespace Template.MobileApp.Converters;

public sealed class MinuteTextConverter : IValueConverter
{
    public string HourFormat { get; set; } = "{0}時間{1}分";

    public string MinuteFormat { get; set; } = "{0}分";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int minutes)
        {
            return value;
        }

        return minutes >= 60
            ? String.Format(culture, HourFormat, minutes / 60, minutes % 60)
            : String.Format(culture, MinuteFormat, minutes);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
