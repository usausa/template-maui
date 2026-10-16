namespace Template.MobileApp.Converters;

public sealed class BadgeCountConverter : IValueConverter
{
    public int MaxCount { get; set; } = 99;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((value is not int count) || (count <= 0))
        {
            return string.Empty;
        }

        return count > MaxCount ? $"{MaxCount}+" : count.ToString(CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
