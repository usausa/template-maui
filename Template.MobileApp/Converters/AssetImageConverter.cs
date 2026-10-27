namespace Template.MobileApp.Converters;

public sealed class AssetImageConverter : IValueConverter
{
    private static readonly Dictionary<string, ImageSource> Sources = [];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string fileName || String.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var path = parameter is string folder && !String.IsNullOrEmpty(folder) ? Path.Combine(folder, fileName) : fileName;
        if (!Sources.TryGetValue(path, out var source))
        {
            source = ImageSource.FromStream(_ => FileSystem.OpenAppPackageFileAsync(path));
            Sources[path] = source;
        }

        return source;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
