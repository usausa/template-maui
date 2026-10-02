namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

using Android.Views;

using AndroidX.RecyclerView.Widget;

using Microsoft.Maui.Controls.Handlers.Items;

public static partial class CarouselOption
{
    private static readonly ConditionalWeakTable<RecyclerView, PageSnapListener> Listeners = [];

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        CarouselViewHandler.Mapper.AppendToMapping(PageSnapProperty.PropertyName, UpdatePageSnap);
        CarouselViewHandler.Mapper.AppendToMapping(CacheSizeProperty.PropertyName, UpdateCacheSize);
    }

    private static void UpdatePageSnap(CarouselViewHandler handler, CarouselView view)
    {
        var recyclerView = handler.PlatformView;
        if (GetPageSnap(view))
        {
            // ページの中にフォーカスがあると、位置を変えた後のレイアウトがフォーカスのあるページを基準にして前のページに戻る
            // (画面に戻ったときにページの中の一覧がフォーカスを受ける) ので、ViewPager2 と同じくページより先に自分が受ける
            recyclerView.DescendantFocusability = DescendantFocusability.BeforeDescendants;

            // 無ければ作って、スクロールの通知に加える
            Listeners.GetValue(recyclerView, x =>
            {
                var listener = new PageSnapListener(view);
                x.AddOnScrollListener(listener);
                return listener;
            });
        }
        else if (Listeners.TryGetValue(recyclerView, out var listener))
        {
            Listeners.Remove(recyclerView);
            recyclerView.RemoveOnScrollListener(listener);
            recyclerView.DescendantFocusability = DescendantFocusability.AfterDescendants;
        }
    }

    // 画面の外のページを作り直さずに残す数 (負の値は既定の 2 のまま)
    private static void UpdateCacheSize(CarouselViewHandler handler, CarouselView view)
    {
        var size = GetCacheSize(view);
        if (size >= 0)
        {
            handler.PlatformView.SetItemViewCacheSize(size);
        }
    }

    // 標準のスナップ (MandatorySingle) は、最初のフリングの前や位置の変更の後に、指を止めてから離したり、
    // 処理が重くてフリングにならなかったりすると、ページの途中で止まったままになる。
    // 標準の処理が終わった後 (Post) にまだ止まっていれば、中央にいちばん近いページを中央に合わせる
    private sealed class PageSnapListener : RecyclerView.OnScrollListener
    {
        private readonly WeakReference<CarouselView> view;

        public PageSnapListener(CarouselView view)
        {
            this.view = new WeakReference<CarouselView>(view);
        }

        // 先頭に見えているページとそのずれから、スクロールの位置を出す
        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            if ((recyclerView.GetLayoutManager() is LinearLayoutManager manager) &&
                (manager.FindFirstVisibleItemPosition() is var first and >= 0) &&
                (manager.FindViewByPosition(first) is { } child) &&
                view.TryGetTarget(out var target))
            {
                var horizontal = manager.Orientation == LinearLayoutManager.Horizontal;
                var size = horizontal ? manager.GetDecoratedMeasuredWidth(child) : manager.GetDecoratedMeasuredHeight(child);
                var offset = horizontal ? manager.GetDecoratedLeft(child) : manager.GetDecoratedTop(child);
                if (size > 0)
                {
                    SetPagePosition(target, first - (offset / (double)size));
                }
            }
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);
            if (newState == RecyclerView.ScrollStateIdle)
            {
                recyclerView.Post(() => Align(recyclerView));
            }
        }

        private static void Align(RecyclerView recyclerView)
        {
            if ((recyclerView.ScrollState == RecyclerView.ScrollStateIdle) &&
                (recyclerView.GetLayoutManager() is LinearLayoutManager { ChildCount: > 0 } manager))
            {
                var horizontal = manager.Orientation == LinearLayoutManager.Horizontal;
                var center = (horizontal ? recyclerView.Width : recyclerView.Height) / 2;
                var distance = Int32.MaxValue;
                for (var i = 0; i < manager.ChildCount; i++)
                {
                    if (manager.GetChildAt(i) is { } child)
                    {
                        var childCenter = horizontal
                            ? (manager.GetDecoratedLeft(child) + manager.GetDecoratedRight(child)) / 2
                            : (manager.GetDecoratedTop(child) + manager.GetDecoratedBottom(child)) / 2;
                        if (Math.Abs(childCenter - center) < Math.Abs(distance))
                        {
                            distance = childCenter - center;
                        }
                    }
                }

                if ((distance != 0) && (distance != Int32.MaxValue))
                {
                    recyclerView.SmoothScrollBy(horizontal ? distance : 0, horizontal ? 0 : distance);
                }
            }
        }
    }
}
