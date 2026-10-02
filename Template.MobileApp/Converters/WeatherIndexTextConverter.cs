namespace Template.MobileApp.Converters;

public sealed class WeatherIndexTextConverter : IValueConverter
{
    private static readonly Dictionary<WeatherIndexKind, string[]> Texts = new()
    {
        [WeatherIndexKind.Laundry] = ["乾かない", "乾きにくい", "乾く", "よく乾く", "すぐ乾く"],
        [WeatherIndexKind.Umbrella] = ["不要", "念のため", "あると安心", "必要", "必須"],
        [WeatherIndexKind.Clothing] = ["半袖", "長袖シャツ", "羽織もの", "セーター", "コート"],
        [WeatherIndexKind.Heatstroke] = ["ほぼ安全", "注意", "警戒", "厳重警戒", "危険"],
        [WeatherIndexKind.Ultraviolet] = ["弱い", "中程度", "強い", "非常に強い", "極端に強い"],
        [WeatherIndexKind.Pollen] = ["少ない", "やや多い", "多い", "非常に多い", "極めて多い"]
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value is WeatherIndex index) && Texts.TryGetValue(index.Kind, out var texts)
            ? texts[Math.Clamp(index.Level, 1, texts.Length) - 1]
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
