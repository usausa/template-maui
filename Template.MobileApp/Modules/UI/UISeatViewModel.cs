namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UISeatViewModel : AppViewModelBase
{
    // Field

    private const double ZoomStep = 0.5;

    private const double ZoomMinimum = 1;

    private const double ZoomMaximum = 4;

    private readonly TimeProvider timeProvider;

    private SeatMap map = default!;

    // Property

    public SeatMapDrawing Drawing { get; } = new();

    [ObservableProperty]
    public partial SeatScreening Screening { get; set; } = default!;

    [ObservableProperty]
    public partial int ShowingIndex { get; set; }

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial string SelectedText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TotalPrice { get; set; }

    [ObservableProperty]
    public partial double ZoomScale { get; set; } = ZoomMinimum;

    [ObservableProperty]
    public partial Point ZoomOffset { get; set; }

    // Command

    public IObserveCommand ZoomInCommand { get; }

    public IObserveCommand ZoomOutCommand { get; }

    public IObserveCommand PurchaseCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UISeatViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        Drawing.SeatTapped = OnSeatTapped;

        ZoomInCommand = MakeDelegateCommand(() => Zoom(ZoomStep), () => ZoomScale < ZoomMaximum);
        ZoomOutCommand = MakeDelegateCommand(() => Zoom(-ZoomStep), () => ZoomScale > ZoomMinimum);
        PurchaseCommand = MakeDelegateCommand(Purchase, () => SelectedCount > 0);

        Load(0);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Load(ShowingIndex + 1);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        ResetZoom();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(int index)
    {
        ShowingIndex = index % 4;
        Screening = SeatSample.LoadScreening(ShowingIndex, timeProvider.GetLocalNow().DateTime);
        map = SeatSample.LoadMap(ShowingIndex);
        Drawing.Update(map);
        ResetZoom();
        UpdateSelection();
    }

    private void OnSeatTapped(SeatPlace place)
    {
        if (map.Toggle(place))
        {
            Drawing.Invalidate();
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        var selected = map.SelectedPlaces();
        SelectedCount = selected.Count;
        SelectedText = String.Join(", ", selected.Select(static x => x.Name));
        TotalPrice = selected.Sum(static x => SeatMap.PriceOf(x.Kind));
        PurchaseCommand.RaiseCanExecuteChanged();
    }

    private void Purchase()
    {
        if (SelectedCount > 0)
        {
            map.Confirm();
            Drawing.Invalidate();
            UpdateSelection();
        }
    }

    private void Zoom(double step)
    {
        ZoomScale = Math.Clamp(ZoomScale + step, ZoomMinimum, ZoomMaximum);
        ZoomInCommand.RaiseCanExecuteChanged();
        ZoomOutCommand.RaiseCanExecuteChanged();
    }

    private void ResetZoom()
    {
        ZoomScale = ZoomMinimum;
        ZoomOffset = Point.Zero;
        ZoomInCommand.RaiseCanExecuteChanged();
        ZoomOutCommand.RaiseCanExecuteChanged();
    }
}
