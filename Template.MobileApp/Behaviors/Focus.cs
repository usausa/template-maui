namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

using MauiBorder = Microsoft.Maui.Controls.Border;

public static class Focus
{
    //--------------------------------------------------------------------------------
    // SuppressDefaultFocus
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SuppressDefaultFocusProperty = BindableProperty.CreateAttached(
        "SuppressDefaultFocus",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetSuppressDefaultFocus(BindableObject bindable) => (bool)bindable.GetValue(SuppressDefaultFocusProperty);

    public static void SetSuppressDefaultFocus(BindableObject bindable, bool value) => bindable.SetValue(SuppressDefaultFocusProperty, value);

    //--------------------------------------------------------------------------------
    // Default
    //--------------------------------------------------------------------------------

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DefaultProperty = BindableProperty.CreateAttached(
        "Default",
        typeof(bool),
        typeof(Focus),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDefault(BindableObject bindable) => (bool)bindable.GetValue(DefaultProperty);

    public static void SetDefault(BindableObject bindable, bool value) => bindable.SetValue(DefaultProperty, value);

    //--------------------------------------------------------------------------------
    // FocusedStroke / FocusedThickness
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty FocusedStrokeProperty = BindableProperty.CreateAttached(
        "FocusedStroke",
        typeof(Color),
        typeof(Focus),
        null,
        propertyChanged: OnFocusedStrokeChanged);

    public static Color? GetFocusedStroke(BindableObject bindable) => (Color?)bindable.GetValue(FocusedStrokeProperty);

    public static void SetFocusedStroke(BindableObject bindable, Color? value) => bindable.SetValue(FocusedStrokeProperty, value);

    public static readonly BindableProperty FocusedThicknessProperty = BindableProperty.CreateAttached(
        "FocusedThickness",
        typeof(double),
        typeof(Focus),
        double.NaN);

    public static double GetFocusedThickness(BindableObject bindable) => (double)bindable.GetValue(FocusedThicknessProperty);

    public static void SetFocusedThickness(BindableObject bindable, double value) => bindable.SetValue(FocusedThicknessProperty, value);

    //--------------------------------------------------------------------------------
    // ErrorStroke / HasError
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ErrorStrokeProperty = BindableProperty.CreateAttached(
        "ErrorStroke",
        typeof(Color),
        typeof(Focus),
        null);

    public static Color? GetErrorStroke(BindableObject bindable) => (Color?)bindable.GetValue(ErrorStrokeProperty);

    public static void SetErrorStroke(BindableObject bindable, Color? value) => bindable.SetValue(ErrorStrokeProperty, value);

    public static readonly BindableProperty HasErrorProperty = BindableProperty.CreateAttached(
        "HasError",
        typeof(bool),
        typeof(Focus),
        false,
        propertyChanged: OnHasErrorChanged);

    public static bool GetHasError(BindableObject bindable) => (bool)bindable.GetValue(HasErrorProperty);

    public static void SetHasError(BindableObject bindable, bool value) => bindable.SetValue(HasErrorProperty, value);

    private static void OnFocusedStrokeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        var behavior = element.Behaviors.OfType<FocusBorderBehavior>().FirstOrDefault();
        if (newValue is Color)
        {
            if (behavior is null)
            {
                element.Behaviors.Add(new FocusBorderBehavior());
            }
        }
        else if (behavior is not null)
        {
            element.Behaviors.Remove(behavior);
        }
    }

    private static void OnHasErrorChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is VisualElement element)
        {
            element.Behaviors.OfType<FocusBorderBehavior>().FirstOrDefault()?.Refresh();
        }
    }

    private sealed class FocusBorderBehavior : BehaviorBase<VisualElement>
    {
        private const string AnimationName = "FocusBorderAnimation";

        private Color normalStroke = Colors.Transparent;

        private double normalThickness;

        private Thickness normalMargin;

        private bool captured;

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Focused += OnFocused;
            bindable.Unfocused += OnUnfocused;
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            if (FindParentBorder(bindable) is { } border)
            {
                border.AbortAnimation(AnimationName);
            }

            bindable.Focused -= OnFocused;
            bindable.Unfocused -= OnUnfocused;

            base.OnDetachingFrom(bindable);
        }

        public void Refresh()
        {
            if (AssociatedObject is { } element)
            {
                AnimateBorder(element.IsFocused);
            }
        }

        private void OnFocused(object? sender, FocusEventArgs e) => AnimateBorder(focused: true);

        private void OnUnfocused(object? sender, FocusEventArgs e) => AnimateBorder(focused: false);

        private void AnimateBorder(bool focused)
        {
            if ((AssociatedObject is not { } element) || (FindParentBorder(element) is not { } border))
            {
                return;
            }

            if (!captured)
            {
                captured = true;
                normalStroke = (border.Stroke as SolidColorBrush)?.Color ?? Colors.Transparent;
                normalThickness = border.StrokeThickness;
                normalMargin = border.Margin;
            }

            var focusedStroke = GetFocusedStroke(element) ?? Colors.Blue;
            var errorStroke = GetHasError(element) ? GetErrorStroke(element) : null;
            var focusedThickness = GetFocusedThickness(element);
            var changeThickness = !Double.IsNaN(focusedThickness);

            var fromColor = (border.Stroke as SolidColorBrush)?.Color ?? normalStroke;
            var toColor = errorStroke ?? (focused ? focusedStroke : normalStroke);
            var fromThickness = border.StrokeThickness;
            var toThickness = focused && changeThickness ? focusedThickness : normalThickness;

            border.AbortAnimation(AnimationName);

            var brush = new SolidColorBrush(fromColor);
            border.Stroke = brush;
            border.Animate(
                AnimationName,
                v =>
                {
                    UpdateStroke(border, brush, LerpColor(fromColor, toColor, v));
                    if (changeThickness)
                    {
                        UpdateThickness(border, fromThickness + ((toThickness - fromThickness) * v));
                    }
                },
                16,
                150,
                Easing.CubicOut,
                (_, _) =>
                {
                    UpdateStroke(border, brush, toColor);
                    if (changeThickness)
                    {
                        UpdateThickness(border, toThickness);
                    }
                });
        }

        private void UpdateThickness(MauiBorder border, double thickness)
        {
            var grow = thickness - normalThickness;
            border.StrokeThickness = thickness;
            border.Margin = new Thickness(normalMargin.Left - grow, normalMargin.Top - grow, normalMargin.Right - grow, normalMargin.Bottom - grow);
        }

        private static void UpdateStroke(MauiBorder border, SolidColorBrush brush, Color color)
        {
            brush.Color = color;
            border.Handler?.UpdateValue(nameof(MauiBorder.Stroke));
        }

        private static MauiBorder? FindParentBorder(Element start)
        {
            var parent = start.Parent;
            while (parent is not null)
            {
                if (parent is MauiBorder border)
                {
                    return border;
                }
                parent = parent.Parent;
            }
            return null;
        }

        private static Color LerpColor(Color from, Color to, double t) =>
            new((float)(from.Red + ((to.Red - from.Red) * t)), (float)(from.Green + ((to.Green - from.Green) * t)), (float)(from.Blue + ((to.Blue - from.Blue) * t)), (float)(from.Alpha + ((to.Alpha - from.Alpha) * t)));
    }
}
