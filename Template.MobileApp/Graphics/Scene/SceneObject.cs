namespace Template.MobileApp.Graphics.Scene;

using System.Diagnostics;

//--------------------------------------------------------------------------------
// Interface
//--------------------------------------------------------------------------------

public interface ISceneObject
{
    void Attach(SceneControl view);

    void Detach();

    void Render(SKCanvas canvas, int width, int height);

    bool Touch(SKPoint location, int width, int height);
}

//--------------------------------------------------------------------------------
// Base
//--------------------------------------------------------------------------------

#pragma warning disable CA1033
public abstract class SceneObject : ISceneObject, IDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000d / 60);

    private static readonly SKTypeface MonoTypeface = SKTypeface.FromFamilyName("monospace", SKFontStyle.Normal);
    private static readonly SKTypeface MonoBoldTypeface = SKTypeface.FromFamilyName("monospace", SKFontStyle.Bold);

    private static readonly Dictionary<int, SKMaskFilter> BlurCache = [];

    private readonly SKFont textFont = new(MonoTypeface);
    private readonly SKFont textFontBold = new(MonoBoldTypeface);
    private readonly SKPaint textPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private readonly Stopwatch clock = new();

    private SceneControl? control;

    private CancellationTokenSource? cts;

    private Task? loopTask;

    private float lastTime;

    protected SKPaint Stroke { get; } = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    protected SKPaint Fill { get; } = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    protected float Time { get; private set; }

    void ISceneObject.Attach(SceneControl view) => control = view;

    void ISceneObject.Detach() => control = null;

    public void Invalidate() => control?.InvalidateSurface();

    private void Post(Action action) => control?.Dispatcher.Dispatch(action);

    //--------------------------------------------------------------------------------
    // Render / Touch
    //--------------------------------------------------------------------------------

    protected SKSize? VirtualSize { get; set; }

    public bool UseDoubleBuffer { get; set; } = true;

    private readonly Lock bufferSync = new();

    private SKSurface? bufferSurface;

    private SKImage? frontImage;

    private int bufferWidth;

    private int bufferHeight;

    private volatile int lastWidth;

    private volatile int lastHeight;

    void ISceneObject.Render(SKCanvas canvas, int width, int height)
    {
        lastWidth = width;
        lastHeight = height;

        if (UseDoubleBuffer)
        {
            lock (bufferSync)
            {
                if (frontImage is not null)
                {
                    canvas.DrawImage(frontImage, 0f, 0f, new SKSamplingOptions(SKFilterMode.Nearest));
                    return;
                }
            }

            if (cts is not null)
            {
                canvas.Clear();
                return;
            }
        }

        var start = Stopwatch.GetTimestamp();
        RenderCore(canvas, width, height);
        RecordFrame(Stopwatch.GetElapsedTime(start).TotalMilliseconds, "direct");
    }

    bool ISceneObject.Touch(SKPoint location, int width, int height)
    {
        if (VirtualSize is { } virtualSize)
        {
            var scale = Math.Min(width / virtualSize.Width, height / virtualSize.Height);
            var x = (location.X - ((width - (virtualSize.Width * scale)) / 2f)) / scale;
            var y = (location.Y - ((height - (virtualSize.Height * scale)) / 2f)) / scale;
            return OnTouch(new SKPoint(x, y), (int)virtualSize.Width, (int)virtualSize.Height);
        }

        return OnTouch(location, width, height);
    }

    private void RenderCore(SKCanvas canvas, int width, int height)
    {
        if (VirtualSize is { } virtualSize)
        {
            var scale = Math.Min(width / virtualSize.Width, height / virtualSize.Height);
            canvas.Save();
            canvas.Translate((width - (virtualSize.Width * scale)) / 2f, (height - (virtualSize.Height * scale)) / 2f);
            canvas.Scale(scale);
            OnRender(canvas, (int)virtualSize.Width, (int)virtualSize.Height);
            canvas.Restore();
        }
        else
        {
            OnRender(canvas, width, height);
        }
    }

    protected virtual bool OnTouch(SKPoint location, int width, int height) => false;

    //--------------------------------------------------------------------------------
    // Run loop
    //--------------------------------------------------------------------------------

    public void Start()
    {
        if ((cts is not null) && !cts.IsCancellationRequested)
        {
            return;
        }

        clock.Start();
        cts = new CancellationTokenSource();
        loopTask = Loop(cts.Token);
    }

    public void Stop()
    {
        if (cts is null)
        {
            return;
        }

        clock.Stop();
        cts.Cancel();

        try
        {
            loopTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Ignore
        }

        loopTask = null;
        cts.Dispose();
        cts = null;
    }

    private async Task Loop(CancellationToken token)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval);
            var invalidate = InvalidateFrame;
            var update = UpdateFrame;

            // Run on the thread pool without returning to the UI thread's SynchronizationContext (do not render on the UI thread)
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                if (UseDoubleBuffer && (lastWidth > 0))
                {
                    // Double buffer: update and off-screen rendering on the loop thread
                    var t = (float)clock.Elapsed.TotalSeconds;
                    var dt = Math.Clamp(t - lastTime, 0f, 0.1f);
                    lastTime = t;

                    Time = t;
                    Update(t, dt);
                    RenderToBuffer(lastWidth, lastHeight);
                    Post(invalidate);
                }
                else
                {
                    Post(update);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }

        void InvalidateFrame()
        {
            if (!token.IsCancellationRequested)
            {
                Invalidate();
            }
        }

        void UpdateFrame()
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var t = (float)clock.Elapsed.TotalSeconds;
            var dt = Math.Clamp(t - lastTime, 0f, 0.1f);
            lastTime = t;

            Time = t;
            Update(t, dt);
            Invalidate();
        }
    }

    private void RenderToBuffer(int width, int height)
    {
        var start = Stopwatch.GetTimestamp();

        if ((bufferSurface is null) || (bufferWidth != width) || (bufferHeight != height))
        {
            bufferSurface?.Dispose();
            bufferSurface = SKSurface.Create(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            bufferWidth = width;
            bufferHeight = height;
        }

        var canvas = bufferSurface.Canvas;
        canvas.Clear();
        RenderCore(canvas, width, height);
        canvas.Flush();

        var image = bufferSurface.Snapshot();
        lock (bufferSync)
        {
            frontImage?.Dispose();
            frontImage = image;
        }

        RecordFrame(Stopwatch.GetElapsedTime(start).TotalMilliseconds, "buffer");
    }

    //--------------------------------------------------------------------------------
    // Frame stats
    //--------------------------------------------------------------------------------

    public static bool FrameStatsEnabled { get; set; }

    private double statTotal;

    private double statMax;

    private int statCount;

    private long statLastReport;

    private void RecordFrame(double milliseconds, string mode)
    {
        if (!FrameStatsEnabled)
        {
            return;
        }

        statTotal += milliseconds;
        statMax = Math.Max(statMax, milliseconds);
        statCount++;

        var now = Environment.TickCount64;
        if (statLastReport == 0)
        {
            statLastReport = now;
        }
        else if (now - statLastReport >= 3000)
        {
            var message = $"{GetType().Name} {mode} avg={statTotal / statCount:F2}ms max={statMax:F2}ms frames={statCount}";
#if ANDROID
            Android.Util.Log.Debug("SceneStats", message);
#else
            Console.WriteLine($"[SceneStats] {message}");
#endif
            statTotal = 0d;
            statMax = 0d;
            statCount = 0;
            statLastReport = now;
        }
    }

    protected abstract void Update(float t, float dt);

    protected abstract void OnRender(SKCanvas canvas, int width, int height);

    //--------------------------------------------------------------------------------
    // Dispose
    //--------------------------------------------------------------------------------

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
            Stroke.Dispose();
            Fill.Dispose();
            textFont.Dispose();
            textFontBold.Dispose();
            textPaint.Dispose();

            foreach (var layer in layerCache.Values)
            {
                layer.Picture?.Dispose();
            }

            layerCache.Clear();

            staticImage?.Dispose();
            staticImage = null;

            lock (bufferSync)
            {
                frontImage?.Dispose();
                frontImage = null;
            }

            bufferSurface?.Dispose();
            bufferSurface = null;
        }
    }

    //--------------------------------------------------------------------------------
    // Layer cache
    //--------------------------------------------------------------------------------

    private sealed class CachedLayer
    {
        public SKPicture? Picture { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }
    }

    private readonly Dictionary<string, CachedLayer> layerCache = [];

    protected void DrawCachedLayer<TState>(SKCanvas canvas, string key, float width, float height, TState state, Action<SKCanvas, TState> draw)
    {
        if (!layerCache.TryGetValue(key, out var layer))
        {
            layer = new CachedLayer();
            layerCache[key] = layer;
        }

        if ((layer.Picture is null) || (MathF.Abs(layer.Width - width) > 0.5f) || (MathF.Abs(layer.Height - height) > 0.5f))
        {
            layer.Picture?.Dispose();
            layer.Picture = null;
            using var recorder = new SKPictureRecorder();
            draw(recorder.BeginRecording(new SKRect(0f, 0f, width, height)), state);
            layer.Picture = recorder.EndRecording();
            layer.Width = width;
            layer.Height = height;
        }

        canvas.DrawPicture(layer.Picture);
    }

    private SKImage? staticImage;

    private int staticWidth;

    private int staticHeight;

    protected void DrawStaticImage<TState>(SKCanvas canvas, int width, int height, float scale, TState state, Action<SKCanvas, TState> draw)
    {
        if ((staticImage is null) || (staticWidth != width) || (staticHeight != height))
        {
            staticImage?.Dispose();
            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            surface.Canvas.Scale(scale);
            draw(surface.Canvas, state);
            staticImage = surface.Snapshot();
            staticWidth = width;
            staticHeight = height;
        }

        canvas.DrawImage(staticImage, 0f, 0f, new SKSamplingOptions(SKFilterMode.Nearest));
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    protected static float DegToRad(float degrees) => degrees * (MathF.PI / 180f);

    protected static bool Blink(float t, float hz) => ((t * hz) % 1f) < 0.55f;

    protected static SKMaskFilter GetBlur(float sigma)
    {
        var key = (int)(sigma * 10f);
        if (!BlurCache.TryGetValue(key, out var filter))
        {
            filter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sigma);
            BlurCache[key] = filter;
        }

        return filter;
    }

    protected void DrawGlowLine(SKCanvas canvas, float x1, float y1, float x2, float y2, SKColor color, float width)
    {
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = color.WithAlpha(55);
        Stroke.StrokeWidth = width + 4.5f;
        canvas.DrawLine(x1, y1, x2, y2, Stroke);
        Stroke.Color = color;
        Stroke.StrokeWidth = width;
        canvas.DrawLine(x1, y1, x2, y2, Stroke);
    }

    protected void DrawGlowPath(SKCanvas canvas, SKPath path, SKColor color, float width)
    {
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = color.WithAlpha(55);
        Stroke.StrokeWidth = width + 4.5f;
        canvas.DrawPath(path, Stroke);
        Stroke.Color = color;
        Stroke.StrokeWidth = width;
        canvas.DrawPath(path, Stroke);
    }

    protected void DrawText(SKCanvas canvas, string text, float x, float y, float size, SKColor color, bool bold = false, SKTextAlign align = SKTextAlign.Left)
    {
        var font = bold ? textFontBold : textFont;
        font.Size = size;
        textPaint.Color = color;

        var tx = align switch
        {
            SKTextAlign.Center => x - (font.MeasureText(text) / 2f),
            SKTextAlign.Right => x - font.MeasureText(text),
            _ => x
        };
        canvas.DrawText(text, tx, y, SKTextAlign.Left, font, textPaint);
    }

    protected void DrawGlowText(SKCanvas canvas, string text, float x, float y, float size, SKColor color, float sigma, bool bold = false, SKTextAlign align = SKTextAlign.Left)
    {
        var font = bold ? textFontBold : textFont;
        font.Size = size;

        var tx = align switch
        {
            SKTextAlign.Center => x - (font.MeasureText(text) / 2f),
            SKTextAlign.Right => x - font.MeasureText(text),
            _ => x
        };

        textPaint.Color = color.WithAlpha(110);
        textPaint.MaskFilter = GetBlur(sigma);
        canvas.DrawText(text, tx, y, SKTextAlign.Left, font, textPaint);
        textPaint.MaskFilter = null;
        textPaint.Color = color;
        canvas.DrawText(text, tx, y, SKTextAlign.Left, font, textPaint);
    }

    protected float MeasureText(string text, float size, bool bold = false)
    {
        var font = bold ? textFontBold : textFont;
        font.Size = size;
        return font.MeasureText(text);
    }

    protected static SKPath CreateCutPanel(float x, float y, float w, float h, float cut)
    {
        using var builder = new SKPathBuilder();
        builder.MoveTo(x + cut, y);
        builder.LineTo(x + w - cut, y);
        builder.LineTo(x + w, y + cut);
        builder.LineTo(x + w, y + h - cut);
        builder.LineTo(x + w - cut, y + h);
        builder.LineTo(x + cut, y + h);
        builder.LineTo(x, y + h - cut);
        builder.LineTo(x, y + cut);
        builder.Close();
        return builder.Detach();
    }

    protected void DrawCutPanel(SKCanvas canvas, float x, float y, float w, float h, float cut, SKColor fill, SKColor border, float borderWidth)
    {
        using var path = CreateCutPanel(x, y, w, h, cut);
        Fill.Color = fill;
        canvas.DrawPath(path, Fill);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = border;
        Stroke.StrokeWidth = borderWidth;
        canvas.DrawPath(path, Stroke);
    }
}
#pragma warning restore CA1033
