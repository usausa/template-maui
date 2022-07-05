namespace Template.MobileApp;

using System;

using Microsoft.Extensions.DependencyInjection;

using Smart.Maui.Resolver;

using Template.MobileApp.Services;

public sealed class ApplicationInitializer : IMauiInitializeService
{
    public async void Initialize(IServiceProvider services)
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

        // Service
        var dataService = services.GetRequiredService<DataService>();
        await dataService.RebuildAsync();
    }
}
