namespace Template.MobileApp.Controls;

using System.Collections.Specialized;

using SkiaSharp.Views.Maui;

//--------------------------------------------------------------------------------
// Resources
//--------------------------------------------------------------------------------
#pragma warning disable IDE0032
public static class SocialFonts
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

    public static SKColor BackgroundColor { get; } = SKColors.Black.WithAlpha(128);
    public static SKColor TextColor { get; } = new(238, 238, 238);

    public static SKColor MenuBackgroundColor { get; } = SKColors.Black.WithAlpha(160);
    public static SKColor MenuBorderColor { get; } = new SKColor(158, 158, 158).WithAlpha(192);
    public static SKColor MenuTextColor { get; } = new(238, 238, 238);

    public static SKColor TitleBackgroundColor { get; } = SKColors.Black.WithAlpha(160);
    public static SKColor TrackColor { get; } = new(33, 33, 33);

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
#pragma warning restore IDE0032

//--------------------------------------------------------------------------------
// Base
//--------------------------------------------------------------------------------
public abstract class SocialControl : SKCanvasView
{
    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    protected SocialControl()
    {
        BackgroundColor = Colors.Transparent;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

#pragma warning disable IDE0060
    protected static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((SocialControl)bindable).InvalidateSurface();
    }
#pragma warning restore IDE0060
}

//--------------------------------------------------------------------------------
// Icon
//--------------------------------------------------------------------------------
public sealed class SocialIcon : SocialControl
{
    private const int FontSize = 48;

    private static readonly SKColor TextColor = new(224, 224, 224);

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SocialIcon),
        propertyChanged: Invalidate);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialIcon()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var text = Text;

        using var font = new SKFont(SocialFonts.MaterialIcons, size: (int)(FontSize * density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        canvas.Clear(SocialResources.BackgroundColor);

        paint.Color = TextColor;
        var x = (info.Width - font.MeasureText(text)) / 2;
        var y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
    }
}

//--------------------------------------------------------------------------------
// Episode
//--------------------------------------------------------------------------------
public sealed class SocialEpisode : SocialControl
{
    private const int LeftBorderWidth = 4;

    private const int InnerMargin = 4;

    private const int FontSize = 16;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialEpisode),
        propertyChanged: Invalidate);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SocialEpisode),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialEpisode()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var leftBorder = (int)(LeftBorderWidth * density);
        var margin = (int)(InnerMargin * density);

        var text = Text;

        using var font = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize * density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        canvas.Clear(SocialResources.BackgroundColor);

        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        paint.Color = SocialResources.TextColor;
        var x = margin + leftBorder;
        var y = fontHeight;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
    }
}

//--------------------------------------------------------------------------------
// Player
//--------------------------------------------------------------------------------
public sealed class SocialPlayer : SocialControl
{
    private const string ExpAnimationName = "SocialPlayerExp";

    private const uint AnimationRate = 16;

    private const uint AnimationLength = 800;

    private const int LeftBorderWidth = 4;

    private const int ImageBorderWidth = 2;

    private const int ExpHeight = 4;

    private const int Space = 2;

    private const int FontSize1 = 14;

    private const int FontSize2 = 18;

    private const int FontSize3 = 16;

    private const byte ImageBackgroundAlpha = 224;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private double displayPercent;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialPlayer),
        propertyChanged: Invalidate);

    public static readonly BindableProperty PercentProperty = BindableProperty.Create(
        nameof(Percent),
        typeof(double),
        typeof(SocialPlayer),
        propertyChanged: OnPercentChanged);

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(SocialPlayer),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialPlayer()
    {
        PaintSurface += OnPaintSurface;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var leftBorder = (int)(LeftBorderWidth * density);
        var imageBorder = (int)(ImageBorderWidth * density);
        var expHeight = (int)(ExpHeight * density);
        var imageSize = info.Height - (imageBorder * 2);
        var imageRight = leftBorder + imageSize + (imageBorder * 2);
        var space = (int)(Space * density);
        var expWidth = (info.Rect.Right - imageBorder) - (imageRight + space);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize2 * density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize3 * density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        canvas.Clear();
        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);
        paint.Color = SKColors.Black.WithAlpha(ImageBackgroundAlpha);
        canvas.DrawRect(new SKRect(leftBorder, 0, imageRight, info.Height), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(imageRight, 0, info.Rect.Right, info.Height), paint);

        var bitmap = SocialResources.PlayerBitmap;
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(leftBorder + imageBorder, imageBorder, leftBorder + imageBorder + imageSize, imageBorder + imageSize), SKSamplingOptions.Default);

        paint.Color = SocialResources.TextColor;
        canvas.DrawText("PLAYER", imageRight + space, imageBorder + font1Height, SKTextAlign.Left, font1, paint);
        canvas.DrawText("山奥通信", imageRight + space, imageBorder + font1Height + font2Height, SKTextAlign.Left, font2, paint);

        var level = "LEVEL 13";
        canvas.DrawText(level, info.Rect.Right - font3.MeasureText(level) - imageBorder, info.Height - imageBorder - expHeight - space, SKTextAlign.Left, font3, paint);

        paint.Color = SocialResources.TrackColor;
        canvas.DrawRect(new SKRect(imageRight + space, info.Height - imageBorder, imageRight + space + expWidth, info.Height - imageBorder - expHeight), paint);
        paint.Color = ProgressColor.ToSKColor();
        canvas.DrawRect(new SKRect(imageRight + space, info.Height - imageBorder, imageRight + space + (int)(expWidth * displayPercent / 100), info.Height - imageBorder - expHeight), paint);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void OnLoaded(object? sender, EventArgs e)
    {
        displayPercent = 0;
        AnimateExp(Percent);
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        this.AbortAnimation(ExpAnimationName);
    }

    private static void OnPercentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SocialPlayer)bindable;
        if (control.IsLoaded)
        {
            control.AnimateExp((double)newValue);
        }
    }

    private void AnimateExp(double percent)
    {
        var from = displayPercent;

        this.AbortAnimation(ExpAnimationName);
        this.Animate(
            ExpAnimationName,
            v =>
            {
                displayPercent = from + ((percent - from) * v);
                InvalidateSurface();
            },
            AnimationRate,
            AnimationLength,
            Easing.CubicOut,
            (_, _) =>
            {
                displayPercent = percent;
                InvalidateSurface();
            });
    }
}

//--------------------------------------------------------------------------------
// Counter
//--------------------------------------------------------------------------------
public sealed class SocialCounter : SocialControl
{
    private const string ValueAnimationName = "SocialCounterValue";

    private const uint AnimationRate = 16;

    private const uint AnimationLength = 800;

    private const int Space = 2;

    private const int FontSize = 16;

    private static readonly SKColor ButtonColor = new(96, 125, 139);

    private static readonly SKColor TextColor = new(224, 224, 224);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float displayProgress = 1f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(SocialCounter),
        propertyChanged: Invalidate);

    public static readonly BindableProperty CounterProperty = BindableProperty.Create(
        nameof(Counter),
        typeof(int),
        typeof(SocialCounter),
        propertyChanged: OnCounterChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public int Counter
    {
        get => (int)GetValue(CounterProperty);
        set => SetValue(CounterProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialCounter()
    {
        PaintSurface += OnPaintSurface;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        using var font = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize * density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var space = (int)(Space * density);

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        canvas.Clear(SocialResources.BackgroundColor);

        var bitmap = Icon switch
        {
            "wrench" => SocialResources.WrenchBitmap,
            "gem" => SocialResources.GemBitmap,
            "money" => SocialResources.MoneyBitmap,
            _ => default!
        };
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(space, space, info.Height - space, info.Height - space), SKSamplingOptions.Default);

        paint.Color = ButtonColor;
        canvas.DrawRect(new SKRect(info.Rect.Right - info.Height + space, space, info.Rect.Right - space, info.Height - space), paint);

        paint.Color = TextColor;
        var text = "+";
        var x = info.Rect.Right - info.Height + ((info.Height - font.MeasureText(text)) / 2);
        var y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);

        paint.Color = TextColor;
        text = $"{(int)(Counter * displayProgress):N0}";
        x = info.Rect.Right - info.Height - font.MeasureText(text);
        y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void OnLoaded(object? sender, EventArgs e)
    {
        displayProgress = 0f;
        AnimateValues();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        this.AbortAnimation(ValueAnimationName);
    }

    private static void OnCounterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SocialCounter)bindable;
        if (control.IsLoaded)
        {
            control.displayProgress = 0f;
            control.AnimateValues();
        }
        else
        {
            control.InvalidateSurface();
        }
    }

    private void AnimateValues()
    {
        this.AbortAnimation(ValueAnimationName);
        this.Animate(
            ValueAnimationName,
            v =>
            {
                displayProgress = (float)v;
                InvalidateSurface();
            },
            AnimationRate,
            AnimationLength,
            Easing.CubicOut,
            (_, _) =>
            {
                displayProgress = 1f;
                InvalidateSurface();
            });
    }
}

//--------------------------------------------------------------------------------
// Alert
//--------------------------------------------------------------------------------
public sealed class SocialAlert : SocialControl
{
    private const int Space = 2;

    private const int BorderWidth = 2;

    private const int SideBorderWidth = 8;

    private const int FontSize1 = 14;

    private const int FontSize2 = 14;

    private const byte BackgroundAlpha = 192;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialAlert),
        propertyChanged: Invalidate);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SocialAlert),
        string.Empty,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SocialAlert),
        string.Empty,
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialAlert()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var space = (int)(Space * density);
        var border = (int)(BorderWidth * density);
        var sideBorder = (int)(SideBorderWidth * density);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize2 * density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var titleHeight = font1Height + (border * 2) + (space * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        canvas.Clear();
        paint.Color = SKColors.Black.WithAlpha(BackgroundAlpha);
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, info.Height), paint);

        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, border), paint);
        canvas.DrawRect(new SKRect(0, titleHeight - border, info.Rect.Right, titleHeight), paint);
        canvas.DrawRect(new SKRect(0, 0, sideBorder, titleHeight), paint);
        canvas.DrawRect(new SKRect(info.Rect.Right - sideBorder, 0, info.Rect.Right, titleHeight), paint);

        var title = Title;
        var text = Text;
        canvas.DrawText(title, (info.Width - font1.MeasureText(title)) / 2, border + space + font1Height, SKTextAlign.Left, font1, paint);

        canvas.DrawText(text, (info.Width - font2.MeasureText(text)) / 2, titleHeight + font2Height, SKTextAlign.Left, font2, paint);
    }
}

//--------------------------------------------------------------------------------
// Notification
//--------------------------------------------------------------------------------
public sealed class SocialNotification : SocialControl
{
    private const int Space = 2;

    private const int InnerMargin = 4;

    private const int LeftBorderWidth = 4;

    private const int BottomBorderWidth = 3;

    private const int FontSize1 = 12;

    private const int FontSize2 = 14;

    private const int FontSize3 = 10;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text3Property = BindableProperty.Create(
        nameof(Text3),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public static readonly BindableProperty PercentProperty = BindableProperty.Create(
        nameof(Percent),
        typeof(double),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public string Text3
    {
        get => (string)GetValue(Text3Property);
        set => SetValue(Text3Property, value);
    }

    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialNotification()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var space = (int)(Space * density);
        var margin = (int)(InnerMargin * density);
        var leftBorder = (int)(LeftBorderWidth * density);
        var bottomBorder = (int)(BottomBorderWidth * density);
        var progressWidth = info.Width - leftBorder - (space * 2);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize2 * density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize3 * density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.GetLineHeight());
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        canvas.Clear();
        paint.Color = SocialResources.TitleBackgroundColor;
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        paint.Color = ProgressColor.ToSKColor();
        canvas.DrawRect(new SKRect(leftBorder + space, info.Height - bottomBorder, leftBorder + space + (int)(progressWidth * Percent / 100), info.Height), paint);

        var x = margin + leftBorder;
        var y = margin;

        paint.Color = SocialResources.TextColor;

        y += font1Height;
        canvas.DrawText(Text1, x, y, SKTextAlign.Left, font1, paint);

        y += space;

        y += font2Height;
        canvas.DrawText(Text2, x, y, SKTextAlign.Left, font2, paint);
        y += font3Height;
        canvas.DrawText(Text3, x, y, SKTextAlign.Left, font3, paint);
    }
}

//--------------------------------------------------------------------------------
// Status
//--------------------------------------------------------------------------------
public sealed class SocialStatus : SocialControl
{
    private const string ValueAnimationName = "SocialStatusValue";

    private const uint AnimationRate = 16;

    private const uint AnimationLength = 800;

    private const int InnerMargin = 2;

    private const int LeftBorderWidth = 4;

    private const int CircleRadius = 9;

    private const int CircleMargin = 4;

    private const int StatusHeight = 32;

    private const int LevelHeight = 4;

    private const int LevelMaximum = 65536;

    private const int FontSize1 = 16;

    private const int FontSize2 = 16;

    private const int FontSize3 = 12;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float displayProgress = 1f;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Color1Property = BindableProperty.Create(
        nameof(Color1),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Color2Property = BindableProperty.Create(
        nameof(Color2),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Color3Property = BindableProperty.Create(
        nameof(Color3),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Value1Property = BindableProperty.Create(
        nameof(Value1),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: OnValueChanged);

    public static readonly BindableProperty Value2Property = BindableProperty.Create(
        nameof(Value2),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: OnValueChanged);

    public static readonly BindableProperty Value3Property = BindableProperty.Create(
        nameof(Value3),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: OnValueChanged);

    public static readonly BindableProperty Title1Property = BindableProperty.Create(
        nameof(Title1),
        typeof(string),
        typeof(SocialStatus),
        string.Empty,
        propertyChanged: Invalidate);

    public static readonly BindableProperty Title2Property = BindableProperty.Create(
        nameof(Title2),
        typeof(string),
        typeof(SocialStatus),
        string.Empty,
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text3Property = BindableProperty.Create(
        nameof(Text3),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public Color Color1
    {
        get => (Color)GetValue(Color1Property);
        set => SetValue(Color1Property, value);
    }

    public Color Color2
    {
        get => (Color)GetValue(Color2Property);
        set => SetValue(Color2Property, value);
    }

    public Color Color3
    {
        get => (Color)GetValue(Color3Property);
        set => SetValue(Color3Property, value);
    }

    public int Value1
    {
        get => (int)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }

    public int Value2
    {
        get => (int)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }

    public int Value3
    {
        get => (int)GetValue(Value3Property);
        set => SetValue(Value3Property, value);
    }

    public string Title1
    {
        get => (string)GetValue(Title1Property);
        set => SetValue(Title1Property, value);
    }

    public string Title2
    {
        get => (string)GetValue(Title2Property);
        set => SetValue(Title2Property, value);
    }

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public string Text3
    {
        get => (string)GetValue(Text3Property);
        set => SetValue(Text3Property, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialStatus()
    {
        PaintSurface += OnPaintSurface;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var margin = (int)(InnerMargin * density);
        var leftBorder = (int)(LeftBorderWidth * density);
        var circleRadius = (int)(CircleRadius * density);
        var circleMargin = (int)(CircleMargin * density);
        var statusHeight = (int)(StatusHeight * density);
        var levelHeight = (int)(LevelHeight * density);

        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize2 * density));
        using var font3 = new SKFont(SocialFonts.MaterialIcons, size: (int)(FontSize3 * density));

        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.GetLineHeight());
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        canvas.Clear();
        paint.Color = SocialResources.TitleBackgroundColor;
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        paint.Color = Color.ToSKColor();
        var leftRect = new SKRect(0, 0, leftBorder, info.Height);
        canvas.DrawRect(leftRect, paint);

        var x = leftBorder + margin;
        var y = 0;

        paint.Color = SocialResources.TextColor;

        var title1 = Title1;
        var title2 = Title2;
        y += font1Height;
        canvas.DrawText(title1, x, y, SKTextAlign.Left, font1, paint);
        canvas.DrawText(title2, info.Rect.Right - margin - font2.MeasureText(title2), y, SKTextAlign.Left, font2, paint);

        y += margin * 2;

        var centerX = x + circleMargin + circleRadius;
        var centerY = y + (statusHeight / 2);
        var levelStart = centerX + circleMargin + circleRadius;
        var levelWidth = info.Rect.Right - margin - leftBorder - levelStart;

        paint.Color = Color1.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text1 = Text1;
        x = (int)(centerX - (font3.MeasureText(text1) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text1, x, y, SKTextAlign.Left, font3, paint);

        var value1 = (int)(Value1 * displayProgress);
        var value1Text = $"{value1:N0}";
        x = (int)((levelWidth - font2.MeasureText(value1Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value1Text, x, y, SKTextAlign.Left, font2, paint);

        y = centerY + (font2Height / 2);
        paint.Color = SocialResources.TrackColor;
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color1.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value1 / LevelMaximum), levelHeight, paint);

        centerY += statusHeight;

        paint.Color = Color2.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text2 = Text2;
        x = (int)(centerX - (font3.MeasureText(text2) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text2, x, y, SKTextAlign.Left, font3, paint);

        var value2 = (int)(Value2 * displayProgress);
        var value2Text = $"{value2:N0}";
        x = (int)((levelWidth - font2.MeasureText(value2Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value2Text, x, y, SKTextAlign.Left, font2, paint);

        y = centerY + (font2Height / 2);
        paint.Color = SocialResources.TrackColor;
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color2.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value2 / LevelMaximum), levelHeight, paint);

        centerY += statusHeight;

        paint.Color = Color3.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text3 = Text3;
        x = (int)(centerX - (font3.MeasureText(text3) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text3, x, y, SKTextAlign.Left, font3, paint);

        var value3 = (int)(Value3 * displayProgress);
        var value3Text = $"{value3:N0}";
        x = (int)((levelWidth - font2.MeasureText(value3Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value3Text, x, y, SKTextAlign.Left, font2, paint);

        y = centerY + (font2Height / 2);
        paint.Color = SocialResources.TrackColor;
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color3.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value3 / LevelMaximum), levelHeight, paint);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void OnLoaded(object? sender, EventArgs e)
    {
        displayProgress = 0f;
        AnimateValues();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        this.AbortAnimation(ValueAnimationName);
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SocialStatus)bindable;
        if (control.IsLoaded)
        {
            control.displayProgress = 0f;
            control.AnimateValues();
        }
        else
        {
            control.InvalidateSurface();
        }
    }

    private void AnimateValues()
    {
        this.AbortAnimation(ValueAnimationName);
        this.Animate(
            ValueAnimationName,
            v =>
            {
                displayProgress = (float)v;
                InvalidateSurface();
            },
            AnimationRate,
            AnimationLength,
            Easing.CubicOut,
            (_, _) =>
            {
                displayProgress = 1f;
                InvalidateSurface();
            });
    }
}

//--------------------------------------------------------------------------------
// Information
//--------------------------------------------------------------------------------
public sealed class SocialInformation : SocialControl
{
    private const int InnerMargin = 2;

    private const int LeftBorderWidth = 4;

    private const int FontSize1 = 16;

    private const int FontSize2 = 14;

    private const int FontSize3 = 10;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialInformation),
        propertyChanged: Invalidate);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SocialInformation),
        string.Empty,
        propertyChanged: Invalidate);

    public static readonly BindableProperty UnitsProperty = BindableProperty.Create(
        nameof(Units),
        typeof(IReadOnlyList<SocialUnit>),
        typeof(SocialInformation),
        propertyChanged: OnUnitsChanged);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IReadOnlyList<SocialUnit>? Units
    {
        get => (IReadOnlyList<SocialUnit>?)GetValue(UnitsProperty);
        set => SetValue(UnitsProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialInformation()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnUnitsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var information = (SocialInformation)bindable;
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= information.OnUnitsCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += information.OnUnitsCollectionChanged;
        }

        information.InvalidateSurface();
    }

    private void OnUnitsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateSurface();

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var margin = (int)(InnerMargin * density);
        var leftBorder = (int)(LeftBorderWidth * density);

        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize2 * density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize3 * density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.GetLineHeight());
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        canvas.Clear();
        paint.Color = SocialResources.TitleBackgroundColor;
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        var x = leftBorder + margin;
        var y = 0;

        paint.Color = SocialResources.TextColor;

        y += font1Height;
        canvas.DrawText(Title, x, y, SKTextAlign.Left, font1, paint);

        y += margin;

        var units = Units;
        if (units is not null)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                y += margin;

                y += font2Height;
                canvas.DrawText(unit.Name, x, y, SKTextAlign.Left, font2, paint);
                y += font3Height;
                canvas.DrawText(unit.Code, x, y, SKTextAlign.Left, font3, paint);
                canvas.DrawText(unit.Force, info.Rect.Right - margin - font3.MeasureText(unit.Force), y, SKTextAlign.Left, font3, paint);
            }
        }
    }
}

//--------------------------------------------------------------------------------
// Menu
//--------------------------------------------------------------------------------
public sealed class SocialMenu : SocialControl
{
    private const int BorderWidth = 2;

    private const int LeftBorderWidth = 8;

    private const int TextSpaceBorders = 5;

    private const int Text2BottomBorders = 3;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public static readonly BindableProperty FontSize1Property = BindableProperty.Create(
        nameof(FontSize1),
        typeof(double),
        typeof(SocialMenu),
        24d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty FontSize2Property = BindableProperty.Create(
        nameof(FontSize2),
        typeof(double),
        typeof(SocialMenu),
        10d,
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public double FontSize1
    {
        get => (double)GetValue(FontSize1Property);
        set => SetValue(FontSize1Property, value);
    }

    public double FontSize2
    {
        get => (double)GetValue(FontSize2Property);
        set => SetValue(FontSize2Property, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SocialMenu()
    {
        PaintSurface += OnPaintSurface;
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        var density = DeviceDisplay.MainDisplayInfo.Density;

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        var border = (int)(BorderWidth * density);
        var leftBorder = (int)(LeftBorderWidth * density);
        var clientRect = new SKRect(leftBorder, border, info.Width - border, info.Height - border);

        var text1 = Text1;
        var text2 = Text2;
        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize1 * density));
        using var font2 = new SKFont(SocialFonts.Oxanium, size: (int)(FontSize2 * density));
        var text2Height = (int)Math.Ceiling(font2.Metrics.GetLineHeight());

        canvas.Save();
        canvas.ClipRect(clientRect, SKClipOperation.Intersect, true);
        canvas.Clear(SocialResources.MenuBackgroundColor);
        canvas.Restore();

        canvas.Save();
        canvas.ClipRect(clientRect, SKClipOperation.Difference, true);
        canvas.Clear(SocialResources.MenuBorderColor);
        canvas.Restore();

        paint.Color = Color.ToSKColor();
        paint.Style = SKPaintStyle.Fill;
        var leftRect = new SKRect(0, 0, leftBorder, info.Height);
        canvas.DrawRect(leftRect, paint);

        paint.Color = SocialResources.MenuTextColor;
        var text1Width = font1.MeasureText(text1, paint);
        var text1X = (int)((clientRect.Width - text1Width) / 2) + leftBorder;
        var text1H = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var text1Y = ((info.Height - text2Height - (border * TextSpaceBorders)) / 2) + (text1H / 2);
        canvas.DrawText(text1, text1X, text1Y, SKTextAlign.Left, font1, paint);

        paint.Color = SocialResources.MenuTextColor;
        var text2Width = font2.MeasureText(text2, paint);
        var text2X = (int)((clientRect.Width - text2Width) / 2) + leftBorder;
        canvas.DrawText(text2, text2X, info.Height - (border * Text2BottomBorders), SKTextAlign.Left, font2, paint);
    }
}
