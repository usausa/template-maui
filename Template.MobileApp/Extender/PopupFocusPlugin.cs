namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

public sealed class PopupFocusPlugin : IPopupPlugin
{
    // TODO
    public void Extend(ContentView view)
    {
        if (view is Popup popup)
        {
            popup.Margin = new Thickness(0);
            popup.Padding = new Thickness(0);
            popup.Opened += (_, _) =>
            {
                Application.Current?.Dispatcher.Dispatch(() => view.Content?.SetDefaultFocus());
            };
        }
    }
}
