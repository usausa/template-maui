namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

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

public class TextToggle : SKCanvasView
{
    private static readonly SKTypeface Typeface = SKFontManager.Default.MatchCharacter('あ');

    public event EventHandler<ToggleChangedEventArgs>? ToggleChanged;

    private float animationProgress;

    // State

    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled),
        typeof(bool),
        typeof(TextToggle),
        false,
        propertyChanged: (b, _, n) => ((TextToggle)b).AnimateToggle((bool)n));

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    // Text

    public static readonly BindableProperty OnTextProperty = BindableProperty.Create(
        nameof(OnText),
        typeof(string),
        typeof(TextToggle),
        propertyChanged: Invalidate);

    public string OnText
    {
        get => (string)GetValue(OnTextProperty);
        set => SetValue(OnTextProperty, value);
    }

    public static readonly BindableProperty OffTextProperty = BindableProperty.Create(
        nameof(OffText),
        typeof(string),
        typeof(TextToggle),
        propertyChanged: Invalidate);

    public string OffText
    {
        get => (string)GetValue(OffTextProperty);
        set => SetValue(OffTextProperty, value);
    }

    // Font

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(TextToggle),
        14d,
        propertyChanged: Invalidate);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    // Color

    public static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(
        nameof(SelectedTextColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.Blue,
        propertyChanged: Invalidate);

    public Color SelectedTextColor
    {
        get => (Color)GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
        nameof(SelectedBackgroundColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.White,
        propertyChanged: Invalidate);

    public Color SelectedBackgroundColor
    {
        get => (Color)GetValue(SelectedBackgroundColorProperty);
        set => SetValue(SelectedBackgroundColorProperty, value);
    }

    public static readonly BindableProperty UnselectedTextColorProperty = BindableProperty.Create(
        nameof(UnselectedTextColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.White,
        propertyChanged: Invalidate);

    public Color UnselectedTextColor
    {
        get => (Color)GetValue(UnselectedTextColorProperty);
        set => SetValue(UnselectedTextColorProperty, value);
    }

    public static readonly BindableProperty UnselectedBackgroundColorProperty = BindableProperty.Create(
        nameof(UnselectedBackgroundColor),
        typeof(Color),
        typeof(TextToggle),
        Colors.LightGray,
        propertyChanged: Invalidate);

    public Color UnselectedBackgroundColor
    {
        get => (Color)GetValue(UnselectedBackgroundColorProperty);
        set => SetValue(UnselectedBackgroundColorProperty, value);
    }

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

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((TextToggle)bindable).InvalidateSurface();
    }

    private void AnimateToggle(bool newState)
    {
#pragma warning disable CA2000
        var animation = new Animation(v =>
        {
            animationProgress = (float)v;
            InvalidateSurface();
        }, animationProgress, newState ? 1f : 0f);
#pragma warning restore CA2000

        animation.Commit(this, "ToggleAnimation", 16, 200, Easing.SinInOut);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        using var font = new SKFont(Typeface, size: (float)(FontSize * DeviceDisplay.MainDisplayInfo.Density));

        var radius = e.Info.Height / 2f;
        var onTextWidth = font.MeasureText(OnText);
        var offTextWidth = font.MeasureText(OffText);
        var onWidth = e.Info.Width * (onTextWidth + (radius * 2)) / (onTextWidth + offTextWidth + (radius * 4));
        var offWidth = e.Info.Width - onWidth;

        // Background
        paint.Color = UnselectedBackgroundColor.ToSKColor();
        using var backgroundRect = new SKRoundRect(e.Info.Rect, radius);
        canvas.DrawRoundRect(backgroundRect, paint);

        // Active background
        var animStart = onWidth * animationProgress;
        var animWidth = onWidth + ((offWidth - onWidth) * animationProgress);
        using var activeRect = new SKRoundRect(new SKRect(animStart, e.Info.Rect.Top, animStart + animWidth, e.Info.Rect.Bottom), radius);
        paint.Color = SelectedBackgroundColor.ToSKColor();
        canvas.DrawRoundRect(activeRect, paint);

        // Unselected text
        paint.Color = UnselectedTextColor.ToSKColor();
        var onTextX = (onWidth - onTextWidth) / 2f;
        var offTextX = onWidth + ((offWidth - offTextWidth) / 2f);
        var textY = (e.Info.Height - font.Metrics.Ascent - font.Metrics.Descent) / 2f;
        canvas.DrawText(OnText, onTextX, textY, font, paint);
        canvas.DrawText(OffText, offTextX, textY, font, paint);

        // Active mask
        canvas.SaveLayer();

        // Background
        paint.Color = SelectedBackgroundColor.ToSKColor();
        canvas.DrawRoundRect(activeRect, paint);

        // Selected text
        paint.Color = SelectedTextColor.ToSKColor();
        paint.BlendMode = SKBlendMode.SrcIn;
        canvas.DrawText(OnText, onTextX, textY, font, paint);
        canvas.DrawText(OffText, offTextX, textY, font, paint);

        // Reset mode
        canvas.Restore();
    }
}
