namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

//--------------------------------------------------------------------------------
// Resources
//--------------------------------------------------------------------------------
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

//--------------------------------------------------------------------------------
// Icon
//--------------------------------------------------------------------------------
public sealed class SocialIcon : SocialControl
{
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

    public SocialIcon()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var text = Text;

        using var font = new SKFont(SocialFonts.MaterialIcons, size: (int)(48 * DeviceDisplay.MainDisplayInfo.Density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        canvas.Clear(SocialResources.BackgroundColor);

        paint.Color = new SKColor(224, 224, 224);
        var x = (info.Width - font.MeasureText(text)) / 2;
        var y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, font, paint);
    }
}

//--------------------------------------------------------------------------------
// Episode
//--------------------------------------------------------------------------------
public sealed class SocialEpisode : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialEpisode),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SocialEpisode),
        propertyChanged: Invalidate);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SocialEpisode()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var leftBorder = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var margin = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);

        var text = Text;

        using var font = new SKFont(SocialFonts.NotoSerifJP, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        // Border
        canvas.Clear(SocialResources.BackgroundColor);

        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        paint.Color = SocialResources.TextColor;
        var x = margin + leftBorder;
        var y = fontHeight;
        canvas.DrawText(text, x, y, font, paint);
    }
}

//--------------------------------------------------------------------------------
// Player
//--------------------------------------------------------------------------------
public sealed class SocialPlayer : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialPlayer),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty PercentProperty = BindableProperty.Create(
        nameof(Percent),
        typeof(double),
        typeof(SocialPlayer),
        propertyChanged: Invalidate);

    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(SocialPlayer),
        propertyChanged: Invalidate);

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public SocialPlayer()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var leftBorder = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var imageBorder = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var expHeight = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var imageSize = info.Height - (imageBorder * 2);
        var imageRight = leftBorder + imageSize + (imageBorder * 2);
        var space = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var expWidth = (info.Rect.Right - imageBorder) - (imageRight + space);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(14 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(18 * DeviceDisplay.MainDisplayInfo.Density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // Border
        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);
        paint.Color = SKColors.Black.WithAlpha(224);
        canvas.DrawRect(new SKRect(leftBorder, 0, imageRight, info.Height), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(imageRight, 0, info.Rect.Right, info.Height), paint);

        var bitmap = SocialResources.PlayerBitmap;
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(leftBorder + imageBorder, imageBorder, leftBorder + imageBorder + imageSize, imageBorder + imageSize));

        paint.Color = SocialResources.TextColor;
        canvas.DrawText("PLAYER", imageRight + space, imageBorder + font1Height, font1, paint);
        canvas.DrawText("山奥通信", imageRight + space, imageBorder + font1Height + font2Height, font2, paint);

        var level = "LEVEL 13";
        canvas.DrawText(level, info.Rect.Right - font3.MeasureText(level) - imageBorder, info.Height - imageBorder - expHeight - space, font3, paint);

        paint.Color = new SKColor(33, 33, 33);
        canvas.DrawRect(new SKRect(imageRight + space, info.Height - imageBorder, imageRight + space + expWidth, info.Height - imageBorder - expHeight), paint);
        paint.Color = ProgressColor.ToSKColor();
        canvas.DrawRect(new SKRect(imageRight + space, info.Height - imageBorder, imageRight + space + (int)(expWidth * Percent / 100), info.Height - imageBorder - expHeight), paint);
    }
}

//--------------------------------------------------------------------------------
// Counter
//--------------------------------------------------------------------------------
public sealed class SocialCounter : SocialControl
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(SocialCounter),
        propertyChanged: Invalidate);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty CounterProperty = BindableProperty.Create(
        nameof(Counter),
        typeof(int),
        typeof(SocialCounter),
        propertyChanged: Invalidate);

    public int Counter
    {
        get => (int)GetValue(CounterProperty);
        set => SetValue(CounterProperty, value);
    }

    public SocialCounter()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        using var font = new SKFont(SocialFonts.Oxanium, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        var space = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);

        var fontHeight = (int)Math.Ceiling(-font.Metrics.Ascent);

        canvas.Clear(SocialResources.BackgroundColor);

        // Icon
        var bitmap = Icon switch
        {
            "wrench" => SocialResources.WrenchBitmap,
            "gem" => SocialResources.GemBitmap,
            "money" => SocialResources.MoneyBitmap,
            _ => default!
        };
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), new SKRect(space, space, info.Height - space, info.Height - space));

        // Button
        paint.Color = new SKColor(96, 125, 139);
        canvas.DrawRect(new SKRect(info.Rect.Right - info.Height + space, space, info.Rect.Right - space, info.Height - space), paint);

        paint.Color = new SKColor(224, 224, 224);
        var text = "+";
        var x = info.Rect.Right - info.Height + ((info.Height - font.MeasureText(text)) / 2);
        var y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, font, paint);

        // Counter
        paint.Color = new SKColor(224, 224, 224);
        text = $"{Counter:N0}";
        x = info.Rect.Right - info.Height - font.MeasureText(text);
        y = ((info.Height - fontHeight) / 2) + fontHeight;
        canvas.DrawText(text, x, y, font, paint);
    }
}

//--------------------------------------------------------------------------------
// Alert
//--------------------------------------------------------------------------------
public sealed class SocialAlert : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialAlert),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SocialAlert),
        propertyChanged: Invalidate);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SocialAlert()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var space = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var border = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var sideBorder = (int)(8 * DeviceDisplay.MainDisplayInfo.Density);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(14 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(14 * DeviceDisplay.MainDisplayInfo.Density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var titleHeight = font1Height + (border * 2) + (space * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // Background
        paint.Color = SKColors.Black.WithAlpha(192);
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, info.Height), paint);

        // Border
        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, border), paint);
        canvas.DrawRect(new SKRect(0, titleHeight - border, info.Rect.Right, titleHeight), paint);
        canvas.DrawRect(new SKRect(0, 0, sideBorder, titleHeight), paint);
        canvas.DrawRect(new SKRect(info.Rect.Right - sideBorder, 0, info.Rect.Right, titleHeight), paint);

        canvas.DrawText("BEAST ALERT", (info.Width - font1.MeasureText("BEAST ALERT")) / 2, border + space + font1Height, font1, paint);

        canvas.DrawText("牛鬼級旅団出現", (info.Width - font2.MeasureText("牛鬼級旅団出現")) / 2, titleHeight + font2Height, font2, paint);
    }
}

//--------------------------------------------------------------------------------
// Notification
//--------------------------------------------------------------------------------
public sealed class SocialNotification : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public static readonly BindableProperty Text3Property = BindableProperty.Create(
        nameof(Text3),
        typeof(string),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public string Text3
    {
        get => (string)GetValue(Text3Property);
        set => SetValue(Text3Property, value);
    }

    public static readonly BindableProperty PercentProperty = BindableProperty.Create(
        nameof(Percent),
        typeof(double),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(SocialNotification),
        propertyChanged: Invalidate);

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public SocialNotification()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var space = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var margin = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var leftBorder = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var bottomBorder = (int)(3 * DeviceDisplay.MainDisplayInfo.Density);
        var progressWidth = info.Width - leftBorder - (space * 2);

        using var font1 = new SKFont(SocialFonts.Oxanium, size: (int)(12 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(14 * DeviceDisplay.MainDisplayInfo.Density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(10 * DeviceDisplay.MainDisplayInfo.Density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.Descent - font3.Metrics.Ascent);
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // Background
        paint.Color = SKColors.Black.WithAlpha(160);
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        // Border
        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        paint.Color = ProgressColor.ToSKColor();
        canvas.DrawRect(new SKRect(leftBorder + space, info.Height - bottomBorder, leftBorder + space + (int)(progressWidth * Percent / 100), info.Height), paint);

        var x = margin + leftBorder;
        var y = margin;

        paint.Color = SocialResources.TextColor;

        y += font1Height;
        canvas.DrawText(Text1, x, y, font1, paint);

        y += space;

        y += font2Height;
        canvas.DrawText(Text2, x, y, font2, paint);
        y += font3Height;
        canvas.DrawText(Text3, x, y, font3, paint);
    }
}

//--------------------------------------------------------------------------------
// Status
//--------------------------------------------------------------------------------
public sealed class SocialStatus : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty Color1Property = BindableProperty.Create(
        nameof(Color1),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public Color Color1
    {
        get => (Color)GetValue(Color1Property);
        set => SetValue(Color1Property, value);
    }

    public static readonly BindableProperty Color2Property = BindableProperty.Create(
        nameof(Color2),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public Color Color2
    {
        get => (Color)GetValue(Color2Property);
        set => SetValue(Color2Property, value);
    }

    public static readonly BindableProperty Color3Property = BindableProperty.Create(
        nameof(Color3),
        typeof(Color),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public Color Color3
    {
        get => (Color)GetValue(Color3Property);
        set => SetValue(Color3Property, value);
    }

    public static readonly BindableProperty Value1Property = BindableProperty.Create(
        nameof(Value1),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public int Value1
    {
        get => (int)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }

    public static readonly BindableProperty Value2Property = BindableProperty.Create(
        nameof(Value2),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public int Value2
    {
        get => (int)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }

    public static readonly BindableProperty Value3Property = BindableProperty.Create(
        nameof(Value3),
        typeof(int),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public int Value3
    {
        get => (int)GetValue(Value3Property);
        set => SetValue(Value3Property, value);
    }

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public static readonly BindableProperty Text3Property = BindableProperty.Create(
        nameof(Text3),
        typeof(string),
        typeof(SocialStatus),
        propertyChanged: Invalidate);

    public string Text3
    {
        get => (string)GetValue(Text3Property);
        set => SetValue(Text3Property, value);
    }

    public SocialStatus()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var margin = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var leftBorder = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var circleRadius = (int)(9 * DeviceDisplay.MainDisplayInfo.Density);
        var circleMargin = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);
        var statusHeight = (int)(32 * DeviceDisplay.MainDisplayInfo.Density);
        var levelHeight = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);

        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.Oxanium, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));
        using var font3 = new SKFont(SocialFonts.MaterialIcons, size: (int)(12 * DeviceDisplay.MainDisplayInfo.Density));

        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.Descent - font3.Metrics.Ascent);
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // Background
        paint.Color = SKColors.Black.WithAlpha(160);
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        // Border
        paint.Color = Color.ToSKColor();
        var leftRect = new SKRect(0, 0, leftBorder, info.Height);
        canvas.DrawRect(leftRect, paint);

        var x = leftBorder + margin;
        var y = 0;

        paint.Color = SocialResources.TextColor;

        y += font1Height;
        canvas.DrawText("甲種聖装 瑠璃", x, y, font1, paint);
        canvas.DrawText("A FORM", info.Rect.Right - margin - font2.MeasureText("A FORM"), y, font2, paint);

        y += margin * 2;

        var centerX = x + circleMargin + circleRadius;
        var centerY = y + (statusHeight / 2);
        var levelStart = centerX + circleMargin + circleRadius;
        var levelWidth = info.Rect.Right - margin - leftBorder - levelStart;

        // Status1
        paint.Color = Color1.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text1 = Text1;
        x = (int)(centerX - (font3.MeasureText(text1) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text1, x, y, font3, paint);

        // Value1
        var value1 = Value1;
        var value1Text = $"{value1:N0}";
        x = (int)((levelWidth - font2.MeasureText(value1Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value1Text, x, y, font2, paint);

        // Line1
        y = centerY + (font2Height / 2);
        paint.Color = new SKColor(33, 33, 33);
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color1.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value1 / 65536), levelHeight, paint);

        centerY += statusHeight;

        // Status2
        paint.Color = Color2.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text2 = Text2;
        x = (int)(centerX - (font3.MeasureText(text2) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text2, x, y, font3, paint);

        // Value2
        var value2 = Value2;
        var value2Text = $"{value2:N0}";
        x = (int)((levelWidth - font2.MeasureText(value1Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value2Text, x, y, font2, paint);

        // Line2
        y = centerY + (font2Height / 2);
        paint.Color = new SKColor(33, 33, 33);
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color2.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value2 / 65536), levelHeight, paint);

        centerY += statusHeight;

        // Status3
        paint.Color = Color3.ToSKColor();
        canvas.DrawCircle(centerX, centerY, circleRadius, paint);

        paint.Color = SocialResources.TextColor;
        var text3 = Text3;
        x = (int)(centerX - (font3.MeasureText(text3) / 2));
        y = centerY + (font3Height / 2);
        canvas.DrawText(text3, x, y, font3, paint);

        // Value3
        var value3 = Value3;
        var value3Text = $"{value3:N0}";
        x = (int)((levelWidth - font2.MeasureText(value1Text)) / 2) + levelStart;
        y = centerY + (font2Height / 2) - levelHeight;
        canvas.DrawText(value3Text, x, y, font2, paint);

        // Line3
        y = centerY + (font2Height / 2);
        paint.Color = new SKColor(33, 33, 33);
        canvas.DrawRect(levelStart, y, levelWidth, levelHeight, paint);
        paint.Color = Color3.ToSKColor();
        canvas.DrawRect(levelStart, y, (int)((double)levelWidth * value3 / 65536), levelHeight, paint);
    }
}

//--------------------------------------------------------------------------------
// Information
//--------------------------------------------------------------------------------
public sealed class SocialInformation : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialInformation),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public SocialInformation()
    {
        PaintSurface += OnPaintSurface;
    }

    // TODO
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        var margin = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var leftBorder = (int)(4 * DeviceDisplay.MainDisplayInfo.Density);

        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(16 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(14 * DeviceDisplay.MainDisplayInfo.Density));
        using var font3 = new SKFont(SocialFonts.Oxanium, size: (int)(10 * DeviceDisplay.MainDisplayInfo.Density));
        var font1Height = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var font2Height = (int)Math.Ceiling(-font2.Metrics.Ascent);
        var font3Height = (int)Math.Ceiling(font3.Metrics.Descent - font3.Metrics.Ascent);
        var titleHeight = font1Height + (margin * 2);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // Background
        paint.Color = SKColors.Black.WithAlpha(160);
        canvas.DrawRect(new SKRect(0, 0, info.Rect.Right, titleHeight), paint);
        paint.Color = SocialResources.BackgroundColor;
        canvas.DrawRect(new SKRect(0, titleHeight, info.Rect.Right, info.Height), paint);

        // Border
        paint.Color = Color.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, leftBorder, info.Height), paint);

        var x = leftBorder + margin;
        var y = 0;

        paint.Color = SocialResources.TextColor;

        y += font1Height;
        canvas.DrawText("投入戦力 支援部隊", x, y, font1, paint);

        y += margin * 2;

        y += font2Height;
        canvas.DrawText("辺境伯直属戦術機甲大隊", x, y, font2, paint);
        y += font3Height;
        canvas.DrawText("WOLF GRP", x, y, font3, paint);
        canvas.DrawText("MF-4000 x36", info.Rect.Right - margin - font3.MeasureText("MF-4000 x36"), y, font3, paint);

        y += margin;

        y += font2Height;
        canvas.DrawText("第二騎士団聖女計画特務中隊", x, y, font2, paint);
        y += font3Height;
        canvas.DrawText("HOUND SQD", x, y, font3, paint);
        canvas.DrawText("TYPE-19E x10 + JXD-20", info.Rect.Right - margin - font3.MeasureText("TYPE-19E x10 + JXD-20"), y, font3, paint);

        y += margin;

        y += font2Height;
        canvas.DrawText("第三騎士団突撃前衛部隊", x, y, font2, paint);
        y += font3Height;
        canvas.DrawText("VIPPERS", x, y, font3, paint);
        canvas.DrawText("TYPE-19 BLOOD x8", info.Rect.Right - margin - font3.MeasureText("TYPE-19 BLOOD x8"), y, font3, paint);
    }
}

//--------------------------------------------------------------------------------
// Menu
//--------------------------------------------------------------------------------
public sealed class SocialMenu : SocialControl
{
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        nameof(Text1),
        typeof(string),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        nameof(Text2),
        typeof(string),
        typeof(SocialMenu),
        propertyChanged: Invalidate);

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    public static readonly BindableProperty FontSize1Property = BindableProperty.Create(
        nameof(FontSize1),
        typeof(double),
        typeof(SocialMenu),
        24d,
        propertyChanged: Invalidate);

    public double FontSize1
    {
        get => (double)GetValue(FontSize1Property);
        set => SetValue(FontSize1Property, value);
    }

    public static readonly BindableProperty FontSize2Property = BindableProperty.Create(
        nameof(FontSize2),
        typeof(double),
        typeof(SocialMenu),
        10d,
        propertyChanged: Invalidate);

    public double FontSize2
    {
        get => (double)GetValue(FontSize2Property);
        set => SetValue(FontSize2Property, value);
    }

    public SocialMenu()
    {
        PaintSurface += OnPaintSurface;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        // TODO 整理
        using var paint = new SKPaint();
        paint.IsAntialias = true;

        // TODO 定数
        var border = (int)(2 * DeviceDisplay.MainDisplayInfo.Density);
        var leftBorder = (int)(8 * DeviceDisplay.MainDisplayInfo.Density);
        var clientRect = new SKRect(leftBorder, border, info.Width - border, info.Height - border);

        var text1 = Text1;
        var text2 = Text2;
        using var font1 = new SKFont(SocialFonts.NotoSerifJP, size: (int)(FontSize1 * DeviceDisplay.MainDisplayInfo.Density));
        using var font2 = new SKFont(SocialFonts.Oxanium, size: (int)(10 * DeviceDisplay.MainDisplayInfo.Density));
        var text2Height = (int)Math.Ceiling(font2.Metrics.Descent - font2.Metrics.Ascent);

        // Background
        canvas.Save();
        canvas.ClipRect(clientRect, SKClipOperation.Intersect, true);
        canvas.Clear(SocialResources.MenuBackgroundColor);
        canvas.Restore();

        // Border
        canvas.Save();
        canvas.ClipRect(clientRect, SKClipOperation.Difference, true);
        canvas.Clear(SocialResources.MenuBorderColor);
        canvas.Restore();

        paint.Color = Color.ToSKColor();
        paint.Style = SKPaintStyle.Fill;
        var leftRect = new SKRect(0, 0, leftBorder, info.Height);
        canvas.DrawRect(leftRect, paint);

        // Text1
        paint.Color = SocialResources.MenuTextColor;
        var text1Width = font1.MeasureText(text1, paint);
        var text1X = (int)((clientRect.Width - text1Width) / 2) + leftBorder;
        var text1H = (int)Math.Ceiling(-font1.Metrics.Ascent);
        var text1Y = ((info.Height - text2Height - (border * 5)) / 2) + (text1H / 2);
        canvas.DrawText(text1, text1X, text1Y, font1, paint);

        // Text2
        paint.Color = SocialResources.MenuTextColor;
        var text2Width = font2.MeasureText(text2, paint);
        var text2X = (int)((clientRect.Width - text2Width) / 2) + leftBorder;
        canvas.DrawText(text2, text2X, info.Height - (border * 3), font2, paint);
    }
}
