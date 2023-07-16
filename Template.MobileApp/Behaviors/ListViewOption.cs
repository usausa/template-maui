namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Controls.Handlers.Compatibility;

public static class ListViewOption
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

    public static void UseCustomMapper(BehaviorOptions options)
    {
#if ANDROID
        // DisableShowSoftInputOnFocus
        if (options.DisableOverScroll)
        {
            ListViewRenderer.Mapper.Add("DisableOverScroll", static (handler, element) => Action(handler.Control!, element));
        }
#endif
    }

    private static void Action(Android.Widget.ListView listView, BindableObject element)
    {
        var value = GetDisableOverScroll(element);
        listView.OverScrollMode = value ? OverScrollMode.Never : OverScrollMode.IfContentScrolls;
    }
}
