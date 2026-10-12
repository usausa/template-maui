namespace Template.MobileApp.Extender;

using System.Diagnostics;

using Smart.Navigation.Plugins;

// 画面遷移の時間と画面の要素の数をデバッグ出力に書く (MauiProgram.cs の先頭で NAVIGATION_PERFORMANCE を定義して使う)。
// 時間は遷移の始まりからの ms。frame は表示の後の最初の処理で、画面を開く時間の目安
public sealed class NavigationPerformancePlugin : PluginBase
{
    private const string Tag = "[NavigationPerformance]";

    private static long start;

    public static void Attach(INavigator navigator)
    {
        navigator.ExecutingChanged += (_, _) =>
        {
            if (navigator.Executing)
            {
                start = Stopwatch.GetTimestamp();
                Debug.WriteLine($"{Tag} start");
            }
        };
    }

    public override void OnCreate(IPluginContext pluginContext, object view, object? target)
    {
        Debug.WriteLine($"{Tag} create {view.GetType().Name} {Elapsed()}");
    }

    public override void OnNavigatingTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        Debug.WriteLine($"{Tag} navigatingTo {navigationContext.ToId} {Elapsed()}");
    }

    public override void OnNavigatedTo(IPluginContext pluginContext, INavigationContext navigationContext, object view, object? target)
    {
        var id = navigationContext.ToId;
        Debug.WriteLine($"{Tag} navigatedTo {id} {Elapsed()}");
        _ = Application.Current!.Dispatcher.DispatchAsync(() =>
        {
            var root = (IVisualTreeElement)view;
            Debug.WriteLine($"{Tag} frame {id} {Elapsed()} elements={Count(root)}");
            Analyze(root, id.ToString() ?? string.Empty);
        });
    }

    private static string Elapsed() => Stopwatch.GetElapsedTime(start).TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture);

    private static int Count(IVisualTreeElement element) => 1 + element.GetVisualChildren().Sum(Count);

    // 種類ごとの数、隠れている要素 (IsVisible が False の要素とその中) の数と種類、一覧 (ItemsView) の中の要素の数
    private static void Analyze(IVisualTreeElement root, string id)
    {
        var types = new Dictionary<string, int>();
        var hiddenTypes = new Dictionary<string, int>();
        var hidden = 0;
        var inList = 0;

        void Walk(IVisualTreeElement element, bool underHidden, bool underList)
        {
            var name = element.GetType().Name;
            types[name] = types.GetValueOrDefault(name) + 1;
            var isHidden = underHidden || (element is VisualElement { IsVisible: false });
            if (isHidden)
            {
                hidden++;
                hiddenTypes[name] = hiddenTypes.GetValueOrDefault(name) + 1;
            }

            if (underList)
            {
                inList++;
            }

            var isList = underList || (element is ItemsView);
            foreach (var child in element.GetVisualChildren())
            {
                Walk(child, isHidden, isList);
            }
        }

        Walk(root, false, false);
        Debug.WriteLine($"{Tag} tree {id} hidden={hidden} inList={inList} types=" + String.Join(",", types.OrderByDescending(static x => x.Value).Take(14).Select(static x => $"{x.Key}:{x.Value}")));
        Debug.WriteLine($"{Tag} hidden {id} " + String.Join(",", hiddenTypes.OrderByDescending(static x => x.Value).Take(10).Select(static x => $"{x.Key}:{x.Value}")));
    }
}
