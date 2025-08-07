namespace Template.MobileApp.Controls;

//--------------------------------------------------------------------------------
// Resources
//--------------------------------------------------------------------------------
public static class SocialFont
{
    private static SKTypeface? notoSerifJP;
    private static SKTypeface? oxanium;
    private static SKTypeface? materialIcons;

    public static SKTypeface NotoSerifJP => notoSerifJP ??= LoadFont("NotoSerifJP-Medium.ttf");

    public static SKTypeface Oxanium => oxanium ??= LoadFont("Oxanium-Regular.ttf");

    public static SKTypeface MaterialIcons => materialIcons ??= LoadFont("MaterialIcons-Regular.ttf");

    private static SKTypeface LoadFont(string fontName)
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(fontName).GetAwaiter().GetResult();
        return SKFontManager.Default.CreateTypeface(stream);
    }
}

public static class SocialResources
{
    private static SKBitmap? playerBitmap;
    private static SKBitmap? wrenchBitmap;
    private static SKBitmap? gemBitmap;
    private static SKBitmap? moneyBitmap;

    public static SKBitmap PlayerBitmap => playerBitmap ??= LoadBitmap("player.jpg");

    public static SKBitmap WrenchBitmap => wrenchBitmap ??= LoadBitmap("wrench.png");
    public static SKBitmap GemBitmap => gemBitmap ??= LoadBitmap("gem.png");
    public static SKBitmap MoneyBitmap => moneyBitmap ??= LoadBitmap("moneybag.png");

    private static SKBitmap LoadBitmap(string filename)
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(Path.Combine("Social", filename)).GetAwaiter().GetResult();
        return SKBitmap.Decode(stream);
    }
}

//--------------------------------------------------------------------------------
// Base
//--------------------------------------------------------------------------------
public abstract class SocialControl : SKCanvasView
{
    protected SocialControl()
    {
        BackgroundColor = Colors.Transparent;
    }

    protected static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((SocialControl)bindable).InvalidateSurface();
    }
}

// TODO
