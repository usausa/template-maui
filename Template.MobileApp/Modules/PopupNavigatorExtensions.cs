namespace Template.MobileApp.Modules;

public static class PopupNavigatorExtensions
{
    public static ValueTask<string?> InputNumberAsync(this IPopupNavigator popupNavigator, string title, string value, int maxLength)
    {
        return popupNavigator.PopupAsync<NumberInputParameter, string?>(
                DialogId.InputNumber,
                new NumberInputParameter(title, value, maxLength));
    }
}
