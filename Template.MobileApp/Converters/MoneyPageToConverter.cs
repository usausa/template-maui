namespace Template.MobileApp.Converters;

public abstract class MoneyPageToConverter<T> : IValueConverter
{
    public MoneyPage Page { get; set; }

    public T Default { get; set; } = default!;

    public T Selected { get; set; } = default!;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Equals(value, Page) ? Selected : Default;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class MoneyPageToColorConverter : MoneyPageToConverter<Color>
{
}

public sealed class MoneyPageToImageSourceConverter : MoneyPageToConverter<ImageSource>
{
}
