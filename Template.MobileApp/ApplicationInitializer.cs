namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Smart.Maui.Resolver;

public sealed class ApplicationInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
{
        // Setup provider
        ResolveProvider.Default.Provider = services;

        // Setup navigator
        var navigator = services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine(
                $"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };
    }
}
