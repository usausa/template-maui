namespace Template.MobileApp.Converters;

public sealed class ElapsedTimeConverter : IValueConverter
{
    public string JustNowText { get; set; } = "たった今";

    public string MinutesFormat { get; set; } = "{0}分前";

    public string HoursFormat { get; set; } = "{0}時間前";

    public string DaysFormat { get; set; } = "{0}日前";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime time)
        {
            return string.Empty;
        }

        var elapsed = DateTime.Now - time;
        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return JustNowText;
        }

        if (elapsed < TimeSpan.FromHours(1))
        {
            return String.Format(culture, MinutesFormat, (int)elapsed.TotalMinutes);
        }

        return elapsed < TimeSpan.FromDays(1)
            ? String.Format(culture, HoursFormat, (int)elapsed.TotalHours)
            : String.Format(culture, DaysFormat, (int)elapsed.TotalDays);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
