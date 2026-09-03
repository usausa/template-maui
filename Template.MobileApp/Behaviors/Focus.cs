namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

// 同一名前空間の static class Border(attached property)と区別する
using MauiBorder = Microsoft.Maui.Controls.Border;

public static class Focus
{
    // ------------------------------------------------------------------ SuppressDefaultFocus

    public static readonly BindableProperty SuppressDefaultFocusProperty = BindableProperty.CreateAttached(
        "SuppressDefaultFocus",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetSuppressDefaultFocus(BindableObject bindable) => (bool)bindable.GetValue(SuppressDefaultFocusProperty);

    public static void SetSuppressDefaultFocus(BindableObject bindable, bool value) => bindable.SetValue(SuppressDefaultFocusProperty, value);

    // ------------------------------------------------------------------ Default

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DefaultProperty = BindableProperty.CreateAttached(
        "Default",
        typeof(bool),
        typeof(Focus),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDefault(BindableObject bindable) => (bool)bindable.GetValue(DefaultProperty);

    public static void SetDefault(BindableObject bindable, bool value) => bindable.SetValue(DefaultProperty, value);

    // ------------------------------------------------------------------ FocusedStroke / FocusedThickness (フォーカス枠)

    // FocusedStroke を設定すると、親 Border の枠線をフォーカス連動でアニメーションする FocusBorderBehavior を自動付与する
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
        2d);

    public static double GetFocusedThickness(BindableObject bindable) => (double)bindable.GetValue(FocusedThicknessProperty);

    public static void SetFocusedThickness(BindableObject bindable, double value) => bindable.SetValue(FocusedThicknessProperty, value);

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

    // FocusedStroke / FocusedThickness に応じて、親 Border の枠線を色・太さアニメーション付きで強調する。
    // XAML から直接は使わず、上記の添付プロパティ経由で自動付与される。
    private sealed class FocusBorderBehavior : BehaviorBase<VisualElement>
    {
        private const string AnimationName = "FocusBorderAnimation";

        // 通常時の枠は初回フォーカス時に親 Border から取り込む
        private Color normalStroke = Colors.Transparent;

        private double normalThickness;

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
            }

            var focusedStroke = GetFocusedStroke(element) ?? Colors.Blue;
            var focusedThickness = GetFocusedThickness(element);

            var fromColor = (border.Stroke as SolidColorBrush)?.Color ?? normalStroke;
            var toColor = focused ? focusedStroke : normalStroke;
            var fromThickness = border.StrokeThickness;
            var toThickness = focused ? focusedThickness : normalThickness;

            border.AbortAnimation(AnimationName);

            // フレームごとのBrush生成を避け、単一インスタンスのColorのみ更新する
            var brush = new SolidColorBrush(fromColor);
            border.Stroke = brush;
            border.Animate(
                AnimationName,
                v =>
                {
                    brush.Color = LerpColor(fromColor, toColor, v);
                    border.StrokeThickness = fromThickness + ((toThickness - fromThickness) * v);
                },
                16,
                150,
                Easing.CubicOut,
                (_, _) =>
                {
                    brush.Color = toColor;
                    border.StrokeThickness = toThickness;
                });
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
            new(
                (float)(from.Red + ((to.Red - from.Red) * t)),
                (float)(from.Green + ((to.Green - from.Green) * t)),
                (float)(from.Blue + ((to.Blue - from.Blue) * t)),
                (float)(from.Alpha + ((to.Alpha - from.Alpha) * t)));
    }
}
