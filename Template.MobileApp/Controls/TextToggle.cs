namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

//--------------------------------------------------------------------------------
// Event
//--------------------------------------------------------------------------------
public sealed class ToggleChangedEventArgs : EventArgs
{
    public static readonly ToggleChangedEventArgs On = new(true);
    public static readonly ToggleChangedEventArgs Off = new(false);

    public bool IsToggled { get; }

    private ToggleChangedEventArgs(bool isToggled)
    {
        IsToggled = isToggled;
    }
}

//--------------------------------------------------------------------------------
// Toggle
//--------------------------------------------------------------------------------
public class TextToggle : SKCanvasView
{
    private const string AnimationName = "ToggleAnimation";

    private const string FontFamily = "sans-serif";

    private const uint AnimationRate = 16;

    private const uint AnimationLength = 200;

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private float animationProgress;

    private FontResolver? fonts;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled),
        typeof(bool),
        typeof(TextToggle),
        false,
        propertyChanged: (b, _, n) => ((TextToggle)b).AnimateToggle((bool)n));

    public static readonly BindableProperty OnTextProperty = BindableProperty.Create(
        nameof(OnText),
        typeof(string),
        typeof(TextToggle),
        propertyChanged: Invalidate);

    public static readonly BindableProperty OffTextProperty = BindableProperty.Create(
        nameof(OffText),
        typeof(string),
        typeof(TextToggle),
        propertyChanged: Invalidate);

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(TextToggle),
        14d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(
        nameof(SelectedTextColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.Blue,
        propertyChanged: Invalidate);

    public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
        nameof(SelectedBackgroundColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.White,
        propertyChanged: Invalidate);

    public static readonly BindableProperty UnselectedTextColorProperty = BindableProperty.Create(
        nameof(UnselectedTextColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.White,
        propertyChanged: Invalidate);

    public static readonly BindableProperty UnselectedBackgroundColorProperty = BindableProperty.Create(
        nameof(UnselectedBackgroundColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.LightGray,
        propertyChanged: Invalidate);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public string OnText
    {
        get => (string)GetValue(OnTextProperty);
        set => SetValue(OnTextProperty, value);
    }

    public string OffText
    {
        get => (string)GetValue(OffTextProperty);
        set => SetValue(OffTextProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color SelectedTextColor
    {
        get => (Color)GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public Color SelectedBackgroundColor
    {
        get => (Color)GetValue(SelectedBackgroundColorProperty);
        set => SetValue(SelectedBackgroundColorProperty, value);
    }

    public Color UnselectedTextColor
    {
        get => (Color)GetValue(UnselectedTextColorProperty);
        set => SetValue(UnselectedTextColorProperty, value);
    }

    public Color UnselectedBackgroundColor
    {
        get => (Color)GetValue(UnselectedBackgroundColorProperty);
        set => SetValue(UnselectedBackgroundColorProperty, value);
    }

    public event EventHandler<ToggleChangedEventArgs>? ToggleChanged;

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public TextToggle()
    {
        EnableTouchEvents = true;
        Touch += (_, _) =>
        {
            IsToggled = !IsToggled;
            ToggleChanged?.Invoke(this, IsToggled ? ToggleChangedEventArgs.On : ToggleChangedEventArgs.Off);
            AnimateToggle(IsToggled);
        };
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((TextToggle)bindable).InvalidateSurface();
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.NewHandler is null)
        {
            fonts?.Dispose();
            fonts = null;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        fonts ??= new FontResolver(FontFamily);
        var size = (float)(FontSize * DeviceDisplay.MainDisplayInfo.Density);
        var onText = fonts.CreateText(OnText, size, false);
        var offText = fonts.CreateText(OffText, size, false);

        var radius = e.Info.Height / 2f;
        var onTextWidth = onText.Width;
        var offTextWidth = offText.Width;
        var onWidth = e.Info.Width * (onTextWidth + (radius * 2)) / (onTextWidth + offTextWidth + (radius * 4));
        var offWidth = e.Info.Width - onWidth;

        paint.Color = UnselectedBackgroundColor.ToSKColor();
        using var backgroundRect = new SKRoundRect(e.Info.Rect, radius);
        canvas.DrawRoundRect(backgroundRect, paint);

        var animStart = onWidth * animationProgress;
        var animWidth = onWidth + ((offWidth - onWidth) * animationProgress);
        using var activeRect = new SKRoundRect(new SKRect(animStart, e.Info.Rect.Top, animStart + animWidth, e.Info.Rect.Bottom), radius);
        paint.Color = SelectedBackgroundColor.ToSKColor();
        canvas.DrawRoundRect(activeRect, paint);

        paint.Color = UnselectedTextColor.ToSKColor();
        var onTextX = (onWidth - onTextWidth) / 2f;
        var offTextX = onWidth + ((offWidth - offTextWidth) / 2f);
        var textY = fonts.GetMetrics(size, false).GetCenterBaseline(e.Info.Height / 2f);
        canvas.DrawText(onText, onTextX, textY, SKTextAlign.Left, paint);
        canvas.DrawText(offText, offTextX, textY, SKTextAlign.Left, paint);

        canvas.SaveLayer();

        paint.Color = SelectedBackgroundColor.ToSKColor();
        canvas.DrawRoundRect(activeRect, paint);

        paint.Color = SelectedTextColor.ToSKColor();
        paint.BlendMode = SKBlendMode.SrcIn;
        canvas.DrawText(onText, onTextX, textY, SKTextAlign.Left, paint);
        canvas.DrawText(offText, offTextX, textY, SKTextAlign.Left, paint);

        canvas.Restore();
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void AnimateToggle(bool newState)
    {
#pragma warning disable CA2000
        var animation = new Animation(v =>
        {
            animationProgress = (float)v;
            InvalidateSurface();
        }, animationProgress, newState ? 1f : 0f);
#pragma warning restore CA2000

        animation.Commit(this, AnimationName, AnimationRate, AnimationLength, Easing.SinInOut);
    }
}
