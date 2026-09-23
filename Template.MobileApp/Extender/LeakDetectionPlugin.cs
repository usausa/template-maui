namespace Template.MobileApp.Extender;

using Smart.Mvvm.Resolver;
using Smart.Navigation.Plugins;

// DEBUG 限定。閉じたビューと ViewModel が一定時間後に GC で回収されているかを確認し、残っていればリーク疑いとして警告する
// 既知の誤検知: Entry にスペルチェック対象の単語が入っていた画面は、Android のスペルチェッカーが EditText をしばらく参照するため 5 秒では残る (30 秒程度で回収される)
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
        // Java 側からの参照 (プラットフォームビュー) も切れるよう両方の GC を回す
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
