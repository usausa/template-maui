namespace Template.MobileApp.Controls;

using Microsoft.Maui.Handlers;

internal static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureCustomControls(this MauiAppBuilder builder)
    {
        return builder;
    }

    public static MauiAppBuilder FixIssue11662(this MauiAppBuilder builder)
    {
        // [MEMO] https://github.com/dotnet/maui/issues/11662
        return builder
            .ConfigureMauiHandlers(_ =>
            {
                LabelHandler.Mapper.AppendToMapping(
                    nameof(View.BackgroundColor),
                    (handler, _) => handler.UpdateValue(nameof(IView.Background)));
                ButtonHandler.Mapper.AppendToMapping(
                    nameof(View.BackgroundColor),
                    (handler, _) => handler.UpdateValue(nameof(IView.Background)));
            });
    }
}
