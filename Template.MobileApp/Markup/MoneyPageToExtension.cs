namespace Template.MobileApp.Markup;

using Template.MobileApp.Converters;

[AcceptEmptyServiceProvider]
public sealed class MoneyPageToColorExtension : IMarkupExtension<MoneyPageToColorConverter>
{
    public MoneyPage Page { get; set; }

    public Color Default { get; set; } = default!;

    public Color Selected { get; set; } = default!;

    public MoneyPageToColorConverter ProvideValue(IServiceProvider serviceProvider) =>
        new() { Page = Page, Default = Default, Selected = Selected };

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

[AcceptEmptyServiceProvider]
public sealed class MoneyPageToImageSourceExtension : IMarkupExtension<MoneyPageToImageSourceConverter>
{
    public MoneyPage Page { get; set; }

    public ImageSource Default { get; set; } = default!;

    public ImageSource Selected { get; set; } = default!;

    public MoneyPageToImageSourceConverter ProvideValue(IServiceProvider serviceProvider) =>
        new() { Page = Page, Default = Default, Selected = Selected };

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
