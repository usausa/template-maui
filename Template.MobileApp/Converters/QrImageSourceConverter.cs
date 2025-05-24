namespace Template.MobileApp.Converters;

using QRCoder;

public sealed class QrImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
        {
            return null;
        }

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L, true);
        using var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(20);
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
