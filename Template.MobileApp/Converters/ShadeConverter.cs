namespace Template.MobileApp.Converters;

// Darkens or lightens a color by a fixed rate, for a tone built from artwork
public sealed class ShadeConverter : IValueConverter
{
    public float Rate { get; set; } = 1f;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Color color
            ? new Color(color.Red * Rate, color.Green * Rate, color.Blue * Rate)
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
