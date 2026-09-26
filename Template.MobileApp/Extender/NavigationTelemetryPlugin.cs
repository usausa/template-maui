namespace Template.MobileApp.Extender;

using System.Diagnostics;

using Smart.Mvvm.Resolver;
using Smart.Navigation.Plugins;

using Template.MobileApp.Diagnostics;

// 画面遷移のスパン (遷移の開始から表示まで)。テレメトリの送信中だけ記録される
public sealed class NavigationTelemetryPlugin : PluginBase
{
    private DateTimeOffset navigatingAt;

    private ActivitySource Source => field ??= ResolveProvider.Default.GetRequiredService<DiagnosticsInstrumentation>().Source;

    public override void OnNavigatingTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        navigatingAt = DateTimeOffset.UtcNow;
    }

    public override void OnNavigatedTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        // Created after the fact with the start time so that the span does not become the parent of later work
        using var activity = Source.StartActivity("Navigate", ActivityKind.Internal, default(ActivityContext), startTime: navigatingAt);
        if (activity is not null)
        {
            activity.SetTag("app.screen.name", navigationContext.ToId.ToString());
            activity.SetTag("application.navigation.from", navigationContext.FromId?.ToString());
            activity.SetTag("application.navigation.attribute", navigationContext.Attribute.ToString());
        }
    }
}
