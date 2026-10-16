namespace Template.MobileApp.Extender;

using Smart.Mvvm.Resolver;
using Smart.Navigation.Plugins;

public sealed class LeakDetectionPlugin : PluginBase
{
    private static readonly TimeSpan CheckDelay = TimeSpan.FromSeconds(5);

    private ILogger Logger => field ??= ResolveProvider.Default.GetRequiredService<ILogger<LeakDetectionPlugin>>();

    public override void OnClose(IPluginContext pluginContext, object view, object? target)
    {
        var references = new List<KeyValuePair<string, WeakReference>>(2)
        {
            new(view.GetType().Name, new WeakReference(view))
        };
        if (target is not null)
        {
            references.Add(new(target.GetType().Name, new WeakReference(target)));
        }

        Application.Current?.Dispatcher.DispatchDelayed(CheckDelay, () => Check(references));
    }

    private void Check(List<KeyValuePair<string, WeakReference>> references)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
#if ANDROID
        Java.Lang.JavaSystem.Gc();
#endif
        GC.Collect();

        foreach (var (name, reference) in references)
        {
            if (reference.IsAlive)
            {
                Logger.WarnLeakSuspected(name);
            }
            else
            {
                Logger.DebugClosedObjectCollected(name);
            }
        }
    }
}
