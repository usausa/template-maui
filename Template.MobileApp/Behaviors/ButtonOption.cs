namespace Template.MobileApp.Behaviors;

public static partial class ButtonOption
{
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

    public static partial void UseCustomMapper(BehaviorOptions options);
}
