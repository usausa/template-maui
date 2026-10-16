namespace Template.MobileApp.Modules.UI;

public sealed partial class UIStreamContext : ObservableObject
{
    public StreamLibrary Library { get; } = new(StreamSample.LoadItems());

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
