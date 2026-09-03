namespace Template.MobileApp.Models.Sample.Graph;

// タイムライン表示用の展開状態付きラッパー
public sealed partial class TimelineRow : ObservableObject
{
    public required GraphRow Row { get; init; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}
