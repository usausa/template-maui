namespace Template.MobileApp.Behaviors;

public static partial class TouchOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    //--------------------------------------------------------------------------------
    // Ripple
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RippleProperty = BindableProperty.CreateAttached(
        "Ripple",
        typeof(bool),
        typeof(TouchOption),
        false);

    public static bool GetRipple(BindableObject bindable) => (bool)bindable.GetValue(RippleProperty);

    public static void SetRipple(BindableObject bindable, bool value) => bindable.SetValue(RippleProperty, value);

    //--------------------------------------------------------------------------------
    // RippleColor
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RippleColorProperty = BindableProperty.CreateAttached(
        "RippleColor",
        typeof(Color),
        typeof(TouchOption),
        Color.FromArgb("#2E1C1B1F"));

    public static Color GetRippleColor(BindableObject bindable) => (Color)bindable.GetValue(RippleColorProperty);

    public static void SetRippleColor(BindableObject bindable, Color value) => bindable.SetValue(RippleColorProperty, value);

    //--------------------------------------------------------------------------------
    // ClickCommand
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ClickCommandProperty = BindableProperty.CreateAttached(
        "ClickCommand",
        typeof(ICommand),
        typeof(TouchOption),
        null);

    public static ICommand? GetClickCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(ClickCommandProperty);

    public static void SetClickCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(ClickCommandProperty, value);

    public static readonly BindableProperty ClickCommandParameterProperty = BindableProperty.CreateAttached(
        "ClickCommandParameter",
        typeof(object),
        typeof(TouchOption),
        null);

    public static object? GetClickCommandParameter(BindableObject bindable) => bindable.GetValue(ClickCommandParameterProperty);

    public static void SetClickCommandParameter(BindableObject bindable, object? value) => bindable.SetValue(ClickCommandParameterProperty, value);
}
