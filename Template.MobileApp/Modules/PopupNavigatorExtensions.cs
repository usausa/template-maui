namespace Template.MobileApp.Modules;

using Template.MobileApp.Input;

public static class PopupNavigatorExtensions
{
    public static ValueTask<string?> InputNumberAsync(this IPopupNavigator popupNavigator, string title, string value, int maxLength)
    {
        return FocusHelper.WithRestoreFocus(() =>
            popupNavigator.PopupAsync<NumberInputParameter, string?>(
                DialogId.InputNumber,
                new NumberInputParameter(title, value, maxLength)));
    }
}
