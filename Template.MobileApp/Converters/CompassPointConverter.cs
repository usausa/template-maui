namespace Template.MobileApp.Converters;

public sealed class CompassPointConverter : IValueConverter
{
    private static readonly string[] Points = ["北", "北東", "東", "南東", "南", "南西", "西", "北西"];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            int degree => ToPoint(degree),
            double degree => ToPoint(degree),
            _ => string.Empty
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string ToPoint(double degree)
    {
        var index = (int)Math.Round((((degree % 360) + 360) % 360) / 45) % Points.Length;
        return Points[index];
    }
}
