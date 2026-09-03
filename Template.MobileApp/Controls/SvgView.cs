namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

using Svg.Skia;

public sealed class SvgView : SKCanvasView
{
    // Source 指定でロードした SKSvg の共有キャッシュ (アプリパッケージ内アセットが対象のため無効化は不要)
    private static readonly Dictionary<string, SKSvg> SourceCache = [];

    public static readonly BindableProperty SvgProperty = BindableProperty.Create(
        nameof(Svg),
        typeof(SKSvg),
        typeof(SvgView),
        propertyChanged: Invalidate);

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(string),
        typeof(SvgView),
        propertyChanged: static (bindable, _, _) => ((SvgView)bindable).HandleSourceChanged());

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(SvgView),
        propertyChanged: static (bindable, _, newValue) => ((SvgView)bindable).HandleFallbackChanged((string?)newValue));

    public static readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create(
        nameof(ErrorPlaceholder),
        typeof(string),
        typeof(SvgView),
        propertyChanged: static (bindable, _, newValue) => ((SvgView)bindable).HandleFallbackChanged((string?)newValue));

    public event EventHandler? Loading;

    public event EventHandler? Ready;

    public event EventHandler? Error;

    public SKSvg? Svg
    {
        get => (SKSvg?)GetValue(SvgProperty);
        set => SetValue(SvgProperty, value);
    }

    // アプリパッケージ内のファイルパス (例: Svg/dotnet_bot.svg)。Svg より優先される
    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    // ロード中に表示する SVG のパス
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // ロード失敗時に表示する SVG のパス
    public string? ErrorPlaceholder
    {
        get => (string?)GetValue(ErrorPlaceholderProperty);
        set => SetValue(ErrorPlaceholderProperty, value);
    }

    private SKSvg? sourceSvg;

    private bool sourceFailed;

    private int loadVersion;

    public SvgView()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((SvgView)bindable).InvalidateSurface();
    }

    //--------------------------------------------------------------------------------
    // Source loading
    //--------------------------------------------------------------------------------

    private void HandleSourceChanged()
    {
        var version = ++loadVersion;
        sourceSvg = null;
        sourceFailed = false;

        var source = Source;
        if (String.IsNullOrEmpty(source))
        {
            InvalidateSurface();
            return;
        }

        if (SourceCache.TryGetValue(source, out var cached))
        {
            sourceSvg = cached;
            Ready?.Invoke(this, EventArgs.Empty);
            InvalidateSurface();
            return;
        }

        Loading?.Invoke(this, EventArgs.Empty);
        InvalidateSurface();

        _ = LoadSourceAsync(source, version);
    }

    // Placeholder / ErrorPlaceholder は表示タイミングでロードが間に合わないため先読みしておく
    private void HandleFallbackChanged(string? source)
    {
        if (!String.IsNullOrEmpty(source) && !SourceCache.ContainsKey(source))
        {
            _ = PreloadAsync(source);
        }
    }

    private async Task LoadSourceAsync(string source, int version)
    {
        var svg = await LoadSvgAsync(source).ConfigureAwait(false);
        await Dispatcher.DispatchAsync(() =>
        {
            if (svg is not null)
            {
                SourceCache[source] = svg;
            }

            if (version != loadVersion)
            {
                return;
            }

            if (svg is not null)
            {
                sourceSvg = svg;
                Ready?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                sourceFailed = true;
                Error?.Invoke(this, EventArgs.Empty);
            }

            InvalidateSurface();
        }).ConfigureAwait(false);
    }

    private async Task PreloadAsync(string source)
    {
        var svg = await LoadSvgAsync(source).ConfigureAwait(false);
        if (svg is null)
        {
            return;
        }

        await Dispatcher.DispatchAsync(() =>
        {
            SourceCache[source] = svg;
            InvalidateSurface();
        }).ConfigureAwait(false);
    }

    private static async Task<SKSvg?> LoadSvgAsync(string source)
    {
        try
        {
            var svg = new SKSvg();
            await using (var stream = await FileSystem.OpenAppPackageFileAsync(source).ConfigureAwait(false))
            {
                svg.Load(stream);
            }

            return svg.Picture is not null ? svg : null;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.Xml.XmlException)
        {
            return null;
        }
    }

    private SKSvg? ResolveDisplaySvg()
    {
        if (String.IsNullOrEmpty(Source))
        {
            return Svg;
        }

        if (sourceSvg is not null)
        {
            return sourceSvg;
        }

        var fallback = sourceFailed ? ErrorPlaceholder ?? Placeholder : Placeholder;
        if (!String.IsNullOrEmpty(fallback) && SourceCache.TryGetValue(fallback, out var cached))
        {
            return cached;
        }

        return null;
    }

    //--------------------------------------------------------------------------------
    // Paint
    //--------------------------------------------------------------------------------

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;

        if (BackgroundColor is not null)
        {
            canvas.Clear(BackgroundColor.ToSKColor());
        }
        else
        {
            canvas.Clear();
        }

        var svg = ResolveDisplaySvg();
        if (svg?.Picture is null)
        {
            return;
        }

        var canvasMin = Math.Min(e.Info.Width, e.Info.Height);
        var svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
        var scale = canvasMin / svgMax;
        var x = (e.Info.Width - (svg.Picture.CullRect.Width * scale)) / 2;
        var y = (e.Info.Height - (svg.Picture.CullRect.Height * scale)) / 2;
        var matrix = SKMatrix.CreateScale(scale, scale);

        canvas.Save();
        canvas.Translate(x, y);
        canvas.DrawPicture(svg.Picture, matrix);
        canvas.Restore();
    }
}
