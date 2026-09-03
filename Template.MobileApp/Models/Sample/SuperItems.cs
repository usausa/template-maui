namespace Template.MobileApp.Models.Sample;

public sealed class SuperBanner
{
    public required string Image { get; init; }

    public required string Title { get; init; }

    public required string Sub { get; init; }
}

public sealed class SuperApp
{
    public required string Glyph { get; init; }

    public required string Name { get; init; }

    public required Color Color { get; init; }
}

public sealed class SuperCoupon
{
    public required string Title { get; init; }

    public required string Sub { get; init; }

    public required Brush Background { get; init; }
}
