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
        if (bindable is not (Button or ImageButton))
        {
            return;
        }

        var view = (View)bindable;
        var behavior = view.Behaviors.FirstOrDefault(static x => x is PressEffectBehavior);
        if (behavior is not null)
        {
            view.Behaviors.Remove(behavior);
        }
        if (newValue is true)
        {
            view.Behaviors.Add(new PressEffectBehavior());
        }
    }

    // ------------------------------------------------------------
    // LongPress (押したまま LongPressDuration ミリ秒でコマンドを実行)
    // ------------------------------------------------------------

    public static readonly BindableProperty LongPressCommandProperty = BindableProperty.CreateAttached(
        "LongPressCommand",
        typeof(ICommand),
        typeof(ButtonOption),
        null,
        propertyChanged: OnLongPressCommandChanged);

    public static ICommand? GetLongPressCommand(BindableObject obj) =>
        (ICommand?)obj.GetValue(LongPressCommandProperty);

    public static void SetLongPressCommand(BindableObject obj, ICommand? value) =>
        obj.SetValue(LongPressCommandProperty, value);

    public static readonly BindableProperty LongPressDurationProperty = BindableProperty.CreateAttached(
        "LongPressDuration",
        typeof(int),
        typeof(ButtonOption),
        500,
        propertyChanged: OnLongPressDurationChanged);

    public static int GetLongPressDuration(BindableObject obj) =>
        (int)obj.GetValue(LongPressDurationProperty);

    public static void SetLongPressDuration(BindableObject obj, int value) =>
        obj.SetValue(LongPressDurationProperty, value);

    private static void OnLongPressCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not (Button or ImageButton))
        {
            return;
        }

        var view = (View)bindable;
        var behavior = view.Behaviors.OfType<LongPressBehavior>().FirstOrDefault();
        if (newValue is ICommand command)
        {
            if (behavior is null)
            {
                behavior = new LongPressBehavior { Duration = TimeSpan.FromMilliseconds(GetLongPressDuration(view)) };
                view.Behaviors.Add(behavior);
            }

            behavior.Command = command;
        }
        else if (behavior is not null)
        {
            view.Behaviors.Remove(behavior);
        }
    }

    private static void OnLongPressDurationChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if ((bindable is View view) && (view.Behaviors.OfType<LongPressBehavior>().FirstOrDefault() is { } behavior))
        {
            behavior.Duration = TimeSpan.FromMilliseconds((int)newValue!);
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
        if (bindable is not (Button or ImageButton))
        {
            return;
        }

        var view = (View)bindable;
        var behavior = view.Behaviors.FirstOrDefault(static x => x is HapticFeedbackBehavior);
        if (behavior is not null)
        {
            view.Behaviors.Remove(behavior);
        }
        if (newValue is true)
        {
            view.Behaviors.Add(new HapticFeedbackBehavior());
        }
    }
}
