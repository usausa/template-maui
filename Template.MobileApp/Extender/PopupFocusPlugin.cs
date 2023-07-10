namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

using Template.MobileApp.Input;

public sealed class PopupFocusPlugin : IPopupPlugin
{
    public void Extend(Popup popup)
    {
        popup.Content?.Behaviors.Add(new InputRootBehavior());
        popup.Opened += (_, _) =>
        {
            MainThread.BeginInvokeOnMainThread(() => popup.Content?.SetDefaultFocus());
        };
    }
}
