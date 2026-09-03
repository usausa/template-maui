namespace Template.MobileApp.Markup;

using System;

using Fonts;

public abstract class FontIconExtensionBase : IMarkupExtension
{
    public string? Glyph { get; set; }

    public Color? Color { get; set; }

    public double Size { get; set; }

    protected abstract string FontFamily { get; }

    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (Glyph is null)
        {
            return null;
        }

        var source = new FontImageSource
        {
            FontFamily = FontFamily,
            Glyph = Glyph,
            Size = Size
        };
        if (Color is not null)
        {
            source.Color = Color;
        }

        return source;
    }
}

[AcceptEmptyServiceProvider]
[ContentProperty("Glyph")]
public class MaterialExtension : FontIconExtensionBase
{
    protected override string FontFamily => MaterialIcons.FontFamily;
}

[AcceptEmptyServiceProvider]
[ContentProperty("Glyph")]
public sealed class FluentExtension : FontIconExtensionBase
{
    protected override string FontFamily => FluentUI.FontFamily;
}

[AcceptEmptyServiceProvider]
[ContentProperty("Glyph")]
public sealed class MenuIconExtension : MaterialExtension
{
    public MenuIconExtension()
    {
        Color = Colors.White;
        Size = 24d;
    }
}

[AcceptEmptyServiceProvider]
[ContentProperty("Glyph")]
public sealed class MoneyIconExtension : MaterialExtension
{
    public MoneyIconExtension()
    {
        Color = Colors.White;
        Size = 28d;
    }
}
