namespace Template.MobileApp.Converters;

using Template.MobileApp.Modules;

public sealed class TileAreaConverter : IMultiValueConverter
{
    public double Padding { get; set; }

    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((values is not [int columns, int count]) || (columns <= 0))
        {
            return 0d;
        }

        var pitch = (ScreenSize.Width - (Padding * 2)) / columns;
        return Math.Ceiling((double)count / columns) * pitch;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
