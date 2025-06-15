namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;

public static partial class ItemsViewOption
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        // DisableShowSoftInputOnFocus
        if (options.DisableOverScroll)
        {
            CollectionViewHandler.Mapper.AppendToMapping(nameof(DisableOverScrollProperty.PropertyName), static (handler, _) => UpdateOverScroll(handler.PlatformView, handler.VirtualView));
            ListViewRenderer.Mapper.AppendToMapping(DisableOverScrollProperty.PropertyName, static (handler, element) => UpdateOverScroll(handler.Control!, element));
        }
    }

    private static void UpdateOverScroll(View view, BindableObject element)
    {
        var value = GetDisableOverScroll(element);
        view.OverScrollMode = value ? OverScrollMode.Never : OverScrollMode.IfContentScrolls;
    }
}
