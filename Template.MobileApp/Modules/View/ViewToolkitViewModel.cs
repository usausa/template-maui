namespace Template.MobileApp.Modules.View;

public sealed partial class ViewToolkitViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string OtpValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SegmentIndex { get; set; }

    [ObservableProperty]
    public partial object? SelectedChip { get; set; } = "Skia";

    [ObservableProperty]
    public partial double Rating { get; set; } = 3.5;

    public IReadOnlyList<string> Segments { get; } = ["日", "週", "月"];

    public IReadOnlyList<string> Chips { get; } = ["MAUI", "Skia", "Blazor", "gRPC"];

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
