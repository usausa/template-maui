namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Controls.Handlers.Compatibility;

public static partial class ListViewOption
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        // DisableShowSoftInputOnFocus
        if (options.DisableOverScroll)
        {
            ListViewRenderer.Mapper.AppendToMapping(DisableOverScrollProperty.PropertyName, static (handler, element) => Action(handler.Control!, element));
        }
    }

    private static void Action(Android.Widget.ListView listView, BindableObject element)
    {
        var value = GetDisableOverScroll(element);
        listView.OverScrollMode = value ? OverScrollMode.Never : OverScrollMode.IfContentScrolls;
    }
}
