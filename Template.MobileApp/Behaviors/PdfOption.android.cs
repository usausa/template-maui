namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

using AndroidX.RecyclerView.Widget;

using Maui.PDFView;
using Maui.PDFView.Platforms.Android;

public static partial class PdfOption
{
    private const string LastPageKey = "LastPage";

    private static readonly ConditionalWeakTable<RecyclerView, LastPageListener> Listeners = [];

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        PdfViewHandler.PropertyMapper.AppendToMapping(LastPageKey, UpdateLastPage);
    }

    private static void UpdateLastPage(PdfViewHandler handler, PdfView view)
    {
        if (handler.PlatformView.GetChildAt(0) is RecyclerView recyclerView)
        {
            Listeners.GetValue(recyclerView, x =>
            {
                var listener = new LastPageListener(view);
                x.AddOnScrollListener(listener);
                return listener;
            });
        }
    }

    // Maui.PDFView は上から見て最初に全部見えているページを今のページにするため、最後の数ページが 1 画面に収まると、
    // 指で最後まで送っても最後のページにならない。指で送ってこれ以上送れなくなったら最後のページを今のページにする
    // (ページの指定での移動は位置が飛ぶだけで dx / dy が 0 なので対象外。最後のページの手前へ戻すのを妨げない)。
    // RecyclerView は後から加えたリスナーを先に呼ぶので、ライブラリのリスナーが今のページを決めた後に上書きするよう Post する
    private sealed class LastPageListener : RecyclerView.OnScrollListener
    {
        private readonly PdfView view;

        public LastPageListener(PdfView view)
        {
            this.view = view;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            if (((dx != 0) || (dy != 0)) && IsAtEnd(recyclerView, out _))
            {
                recyclerView.Post(() => ApplyLastPage(recyclerView));
            }
        }

        private void ApplyLastPage(RecyclerView recyclerView)
        {
            if (!IsAtEnd(recyclerView, out var count))
            {
                return;
            }

            var last = (uint)(count - 1);
            if (view.PageIndex != last)
            {
                view.PageIndex = last;
            }
        }

        private static bool IsAtEnd(RecyclerView recyclerView, out int count)
        {
            count = recyclerView.GetAdapter()?.ItemCount ?? 0;
            var horizontal = recyclerView.GetLayoutManager() is LinearLayoutManager { Orientation: LinearLayoutManager.Horizontal };
            return (count > 0) && !(horizontal ? recyclerView.CanScrollHorizontally(1) : recyclerView.CanScrollVertically(1));
        }
    }
}
