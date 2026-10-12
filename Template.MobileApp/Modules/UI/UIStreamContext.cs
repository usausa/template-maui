namespace Template.MobileApp.Modules.UI;

// 一覧と詳細が共有する (一覧を開いている間)
public sealed partial class UIStreamContext : ObservableObject
{
    public StreamLibrary Library { get; } = new(StreamSample.LoadItems());

    // 詳細の画面の作品 (Push の前に設定する。関連の作品を選ぶと入れ替わる)
    [ObservableProperty]
    public partial StreamItem Selected { get; set; } = default!;

    [ObservableProperty]
    public partial IReadOnlyList<StreamItem> Related { get; set; } = [];

    public void Open(StreamItem item)
    {
        Selected = item;
        Related = Library.Related(item);
    }
}
