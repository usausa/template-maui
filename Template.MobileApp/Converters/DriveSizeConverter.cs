namespace Template.MobileApp.Converters;

public sealed class DriveSizeConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [DriveItemKind kind, int count, long size])
        {
            return string.Empty;
        }

        return kind == DriveItemKind.Folder
            ? String.Format(culture, "{0} 項目", count)
            : size >= 1024 * 1024
                ? String.Format(culture, "{0:F1} MB", size / 1024d / 1024d)
                : String.Format(culture, "{0:F0} KB", size / 1024d);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
