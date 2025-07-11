namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

public sealed class PopupFocusPlugin : IPopupPlugin
{
    public void Extend(ContentView view)
    {
        if (view is Popup popup)
        {
            popup.Opened += (_, _) =>
            {
                Application.Current?.Dispatcher.Dispatch(() => view.Content?.SetDefaultFocus());
            };
        }
    }
}
