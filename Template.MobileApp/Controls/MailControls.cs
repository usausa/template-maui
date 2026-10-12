namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui.Controls;

//--------------------------------------------------------------------------------
// Avatar
//--------------------------------------------------------------------------------
public sealed class MailAvatarImage : Image
{
    private const string FontFamily = "sans-serif";

    private const int InitialSize = 144;

    private const float InitialFontRate = 0.42f;

    private static readonly SKColor[] InitialColors =
    [
        SKColor.Parse("#FB8C00"),
        SKColor.Parse("#00897B"),
        SKColor.Parse("#D81B60"),
        SKColor.Parse("#43A047"),
        SKColor.Parse("#3949AB"),
        SKColor.Parse("#6D4C41"),
        SKColor.Parse("#8E24AA")
    ];

    private static readonly Dictionary<string, ImageSource> Files = [];

    private static readonly Dictionary<string, ImageSource> Initials = [];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty AvatarProperty = BindableProperty.Create(
        nameof(Avatar),
        typeof(string),
        typeof(MailAvatarImage),
        propertyChanged: OnSourceChanged);

    public static readonly BindableProperty SenderProperty = BindableProperty.Create(
        nameof(Sender),
        typeof(string),
        typeof(MailAvatarImage),
        propertyChanged: OnSourceChanged);

    public string? Avatar
    {
        get => (string?)GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    public string? Sender
    {
        get => (string?)GetValue(SenderProperty);
        set => SetValue(SenderProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Source
    //--------------------------------------------------------------------------------

    private static void OnSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var image = (MailAvatarImage)bindable;
        image.Source = image.Avatar is { Length: > 0 } avatar
            ? LoadFile(avatar)
            : image.Sender is { Length: > 0 } sender ? CreateInitial(sender) : null;
    }

    private static ImageSource LoadFile(string fileName)
    {
        if (!Files.TryGetValue(fileName, out var source))
        {
            var path = Path.Combine("Avatar", fileName);
            source = ImageSource.FromStream(_ => FileSystem.OpenAppPackageFileAsync(path));
            Files[fileName] = source;
        }
        return source;
    }

    private static ImageSource CreateInitial(string name)
    {
        if (!Initials.TryGetValue(name, out var source))
        {
            var bitmap = new SKBitmap(InitialSize, InitialSize);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(InitialColors[name.Sum(static x => x) % InitialColors.Length]);
                using var fonts = new FontResolver(FontFamily);
                using var paint = new SKPaint();
                paint.IsAntialias = true;
                paint.Color = SKColors.White;
                var size = InitialSize * InitialFontRate;
                canvas.DrawText(fonts.CreateText(name[..1], size, true), InitialSize / 2f, fonts.GetMetrics(size, true).GetCenterBaseline(InitialSize / 2f), SKTextAlign.Center, paint);
            }
            source = new SKBitmapImageSource { Bitmap = bitmap };
            Initials[name] = source;
        }
        return source;
    }
}
