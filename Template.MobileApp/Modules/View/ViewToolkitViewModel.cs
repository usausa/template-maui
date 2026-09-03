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

    [ObservableProperty]
    public partial bool IsSheetOpen { get; set; }

    public IReadOnlyList<string> Segments { get; } = ["日", "週", "月"];

    public IReadOnlyList<string> Chips { get; } = ["MAUI", "Skia", "Blazor", "gRPC"];

    public IObserveCommand OpenSheetCommand { get; }

    public IObserveCommand CloseSheetCommand { get; }

    public ViewToolkitViewModel()
    {
        OpenSheetCommand = MakeDelegateCommand(() => IsSheetOpen = true);
        CloseSheetCommand = MakeDelegateCommand(() => IsSheetOpen = false);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
