namespace Template.MobileApp.Behaviors;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

public static partial class ButtonOption
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        ButtonHandler.Mapper.AppendToMapping(EnableTextAlignmentProperty.PropertyName, UpdateTextAlignment);

        if (options.RippleEffect)
        {
            ButtonHandler.Mapper.AppendToMapping(DisableRippleEffectProperty.PropertyName, UpdateDisableRippleEffect);
        }
    }

    private static void UpdateTextAlignment(IButtonHandler handler, IButton view)
    {
        if ((view is Button button) && GetEnableTextAlignment(button))
        {
            handler.PlatformView.Gravity = GetHorizontalTextAlignment(button).ToHorizontalGravity() |
                                           GetVerticalTextAlignment(button).ToVerticalGravity();
        }
    }

    private static void UpdateDisableRippleEffect(IButtonHandler handler, IButton view)
    {
        if ((view is Button button) && GetDisableRippleEffect(button))
        {
            var color = button.BackgroundColor.ToPlatform();
            handler.PlatformView.SetBackgroundColor(color);
        }
    }
}
