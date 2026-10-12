namespace Template.MobileApp.Behaviors;

public static partial class StepperOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty OutlinedProperty = BindableProperty.CreateAttached(
        "Outlined",
        typeof(bool),
        typeof(StepperOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetOutlined(BindableObject bindable) => (bool)bindable.GetValue(OutlinedProperty);

    public static void SetOutlined(BindableObject bindable, bool value) => bindable.SetValue(OutlinedProperty, value);
}
