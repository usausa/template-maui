namespace Template.MobileApp.Behaviors;

using Microsoft.Maui.Handlers;

public static partial class ButtonOption
{
    public static partial void UseCustomMapper()
    {
        ButtonHandler.Mapper.AppendToMapping("TextAlignment", UpdateTextAlignment);
    }

    private static void UpdateTextAlignment(IButtonHandler handler, IButton view)
    {
        if ((view is Button button) && GetEnableTextAlignment(button))
        {
            handler.PlatformView.Gravity = GetHorizontalTextAlignment(button).ToHorizontalGravity() |
                                           GetVerticalTextAlignment(button).ToVerticalGravity();
        }
    }
}
