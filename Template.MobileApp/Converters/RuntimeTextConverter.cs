namespace Template.MobileApp.Converters;

// 上映時間 (分) を「2h 18m」の形に (1 時間未満は「58m」)
public sealed class RuntimeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int minutes)
        {
            return minutes >= 60 ? $"{minutes / 60}h {minutes % 60:D2}m" : $"{minutes}m";
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
