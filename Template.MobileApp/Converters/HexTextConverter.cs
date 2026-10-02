namespace Template.MobileApp.Converters;

public sealed class HexTextConverter : IValueConverter
{
    public int Length { get; set; } = 8;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ReadOnlyMemory<byte> { IsEmpty: false } bytes
            ? bytes.Length > Length
                ? $"{System.Convert.ToHexString(bytes.Span[..Length])}… ({bytes.Length} B)"
                : System.Convert.ToHexString(bytes.Span)
            : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
