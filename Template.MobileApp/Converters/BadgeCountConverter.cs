namespace Template.MobileApp.Converters;

// バッジ件数の表示文字列に変換する(0 以下は空文字、上限超えは "99+")
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
