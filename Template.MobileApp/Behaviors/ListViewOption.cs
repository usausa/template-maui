namespace Template.MobileApp.Behaviors;

public static partial class ListViewOption
{
    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DisableOverScrollProperty = BindableProperty.CreateAttached(
        "DisableOverScroll",
        typeof(bool),
        typeof(EntryOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDisableOverScroll(BindableObject bindable) => (bool)bindable.GetValue(DisableOverScrollProperty);

    public static void SetDisableOverScroll(BindableObject bindable, bool value) => bindable.SetValue(DisableOverScrollProperty, value);

    public static partial void UseCustomMapper(BehaviorOptions options);
}
