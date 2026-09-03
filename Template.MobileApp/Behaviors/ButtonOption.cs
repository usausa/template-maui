namespace Template.MobileApp.Behaviors;

using Smart.Maui.Interactivity;

public static partial class ButtonOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    public static readonly BindableProperty EnableTextAlignmentProperty = BindableProperty.CreateAttached(
        "EnableTextAlignment",
        typeof(bool),
        typeof(ButtonOption),
        false);

    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.CreateAttached(
        "HorizontalTextAlignment",
        typeof(TextAlignment),
        typeof(ButtonOption),
        TextAlignment.Center);

    public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.CreateAttached(
        "VerticalTextAlignment",
        typeof(TextAlignment),
        typeof(ButtonOption),
        TextAlignment.Center);

    public static readonly BindableProperty DisableRippleEffectProperty = BindableProperty.CreateAttached(
        "DisableRippleEffect",
        typeof(bool),
        typeof(ButtonOption),
        false);

    public static bool GetEnableTextAlignment(BindableObject bindable) => (bool)bindable.GetValue(EnableTextAlignmentProperty);

    public static void SetEnableTextAlignment(BindableObject bindable, bool value) => bindable.SetValue(EnableTextAlignmentProperty, value);

    public static TextAlignment GetHorizontalTextAlignment(BindableObject bindable) => (TextAlignment)bindable.GetValue(HorizontalTextAlignmentProperty);

    public static void SetHorizontalTextAlignment(BindableObject bindable, TextAlignment value) => bindable.SetValue(HorizontalTextAlignmentProperty, value);

    public static TextAlignment GetVerticalTextAlignment(BindableObject bindable) => (TextAlignment)bindable.GetValue(VerticalTextAlignmentProperty);

    public static void SetVerticalTextAlignment(BindableObject bindable, TextAlignment value) => bindable.SetValue(VerticalTextAlignmentProperty, value);

    public static bool GetDisableRippleEffect(BindableObject bindable) => (bool)bindable.GetValue(DisableRippleEffectProperty);

    public static void SetDisableRippleEffect(BindableObject bindable, bool value) => bindable.SetValue(DisableRippleEffectProperty, value);

    // ------------------------------------------------------------
    // Pressed
    // ------------------------------------------------------------

    public static readonly BindableProperty IsPressedProperty = BindableProperty.CreateAttached(
        "IsPressed",
        typeof(bool),
        typeof(ButtonOption),
        false,
        defaultBindingMode: BindingMode.OneWayToSource);

    public static bool GetIsPressed(BindableObject obj) =>
        (bool)obj.GetValue(IsPressedProperty);

    public static void SetIsPressed(BindableObject obj, bool value) =>
        obj.SetValue(IsPressedProperty, value);

    public static readonly BindableProperty PressBindProperty = BindableProperty.CreateAttached(
        "PressBind",
        typeof(bool),
        typeof(ButtonOption),
        defaultValue: false,
        propertyChanged: OnPressBindChanged);

    public static bool GetPressBind(BindableObject obj) =>
        (bool)obj.GetValue(PressBindProperty);

    public static void SetPressBind(BindableObject obj, bool value) =>
        obj.SetValue(PressBindProperty, value);

    private static void OnPressBindChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Button view)
        {
            return;
        }

        if (oldValue is not null)
        {
            var behavior = view.Behaviors.FirstOrDefault(static x => x is PressBindBehavior);
            if (behavior is not null)
            {
                view.Behaviors.Remove(behavior);
            }
        }

        if (newValue is not null)
        {
            view.Behaviors.Add(new PressBindBehavior());
        }
    }

    private sealed class PressBindBehavior : BehaviorBase<Button>
    {
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += OnPressed;
            bindable.Released += OnReleased;
            bindable.Unfocused += OnReleased;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            bindable.Pressed -= OnPressed;
            bindable.Released -= OnReleased;
            bindable.Unfocused -= OnReleased;

            base.OnDetachingFrom(bindable);
        }

        private void OnPressed(object? sender, EventArgs e)
        {
            var button = AssociatedObject;
            if (button is not null)
            {
                SetIsPressed(button, true);
            }
        }

        private void OnReleased(object? sender, EventArgs e)
        {
            var button = AssociatedObject;
            if (button is not null)
            {
                SetIsPressed(button, false);
            }
        }
    }

    public static readonly BindableProperty PressEffectProperty = BindableProperty.CreateAttached(
        "PressEffect",
        typeof(bool),
        typeof(ButtonOption),
        defaultValue: false,
        propertyChanged: OnPressEffectChanged);

    public static bool GetPressEffect(BindableObject obj) =>
        (bool)obj.GetValue(PressEffectProperty);

    public static void SetPressEffect(BindableObject obj, bool value) =>
        obj.SetValue(PressEffectProperty, value);

    private static void OnPressEffectChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // Button と ImageButton のどちらにも適用できるようにする
        switch (bindable)
        {
            case Button button:
                {
                    var behavior = button.Behaviors.FirstOrDefault(static x => x is PressEffectBehavior);
                    if (behavior is not null)
                    {
                        button.Behaviors.Remove(behavior);
                    }
                    if (newValue is true)
                    {
                        button.Behaviors.Add(new PressEffectBehavior());
                    }
                    break;
                }

            case ImageButton imageButton:
                {
                    var behavior = imageButton.Behaviors.FirstOrDefault(static x => x is ImagePressEffectBehavior);
                    if (behavior is not null)
                    {
                        imageButton.Behaviors.Remove(behavior);
                    }
                    if (newValue is true)
                    {
                        imageButton.Behaviors.Add(new ImagePressEffectBehavior());
                    }
                    break;
                }

            default:
                break;
        }
    }

    private static void ApplyPressed(VisualElement element)
    {
        element.ScaleToAsync(0.9, 50, Easing.CubicOut);
        element.FadeToAsync(0.8, 50, Easing.CubicOut);
    }

    private static void ApplyReleased(VisualElement element)
    {
        element.ScaleToAsync(1.0, 100, Easing.CubicOut);
        element.FadeToAsync(1.0, 100, Easing.CubicOut);
    }

    private sealed class PressEffectBehavior : BehaviorBase<Button>
    {
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += OnButtonPressed;
            bindable.Released += OnButtonReleased;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.Pressed -= OnButtonPressed;
            bindable.Released -= OnButtonReleased;
        }

        private static void OnButtonPressed(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                ApplyPressed(button);
            }
        }

        private static void OnButtonReleased(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                ApplyReleased(button);
            }
        }
    }

    private sealed class ImagePressEffectBehavior : BehaviorBase<ImageButton>
    {
        protected override void OnAttachedTo(ImageButton bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += OnButtonPressed;
            bindable.Released += OnButtonReleased;
        }

        protected override void OnDetachingFrom(ImageButton bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.Pressed -= OnButtonPressed;
            bindable.Released -= OnButtonReleased;
        }

        private static void OnButtonPressed(object? sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                ApplyPressed(button);
            }
        }

        private static void OnButtonReleased(object? sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                ApplyReleased(button);
            }
        }
    }

    // ------------------------------------------------------------
    // HapticFeedback (押下時にクリックのハプティクスを発生)
    // ------------------------------------------------------------

    public static readonly BindableProperty HapticFeedbackProperty = BindableProperty.CreateAttached(
        "HapticFeedback",
        typeof(bool),
        typeof(ButtonOption),
        defaultValue: false,
        propertyChanged: OnHapticFeedbackChanged);

    public static bool GetHapticFeedback(BindableObject obj) =>
        (bool)obj.GetValue(HapticFeedbackProperty);

    public static void SetHapticFeedback(BindableObject obj, bool value) =>
        obj.SetValue(HapticFeedbackProperty, value);

    private static void OnHapticFeedbackChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // Button と ImageButton のどちらにも適用できるようにする
        switch (bindable)
        {
            case Button button:
                {
                    var behavior = button.Behaviors.FirstOrDefault(static x => x is HapticFeedbackBehavior);
                    if (behavior is not null)
                    {
                        button.Behaviors.Remove(behavior);
                    }
                    if (newValue is true)
                    {
                        button.Behaviors.Add(new HapticFeedbackBehavior());
                    }
                    break;
                }

            case ImageButton imageButton:
                {
                    var behavior = imageButton.Behaviors.FirstOrDefault(static x => x is ImageHapticFeedbackBehavior);
                    if (behavior is not null)
                    {
                        imageButton.Behaviors.Remove(behavior);
                    }
                    if (newValue is true)
                    {
                        imageButton.Behaviors.Add(new ImageHapticFeedbackBehavior());
                    }
                    break;
                }

            default:
                break;
        }
    }

    private static void PerformClickFeedback()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            // Ignore
        }
    }

    private sealed class HapticFeedbackBehavior : BehaviorBase<Button>
    {
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += OnButtonPressed;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.Pressed -= OnButtonPressed;
        }

        private static void OnButtonPressed(object? sender, EventArgs e) => PerformClickFeedback();
    }

    private sealed class ImageHapticFeedbackBehavior : BehaviorBase<ImageButton>
    {
        protected override void OnAttachedTo(ImageButton bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.Pressed += OnButtonPressed;
        }

        protected override void OnDetachingFrom(ImageButton bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.Pressed -= OnButtonPressed;
        }

        private static void OnButtonPressed(object? sender, EventArgs e) => PerformClickFeedback();
    }
}
