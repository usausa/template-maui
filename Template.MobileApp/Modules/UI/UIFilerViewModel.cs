namespace Template.MobileApp.Modules.UI;

public sealed partial class UIFilerViewModel : AppViewModelBase
{
    // Field

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly IDispatcherTimer timer;

    // Property

    public CloudDrive Drive { get; }

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial long SelectedSize { get; set; }

    [ObservableProperty]
    public partial bool IsGrid { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    // Command

    public IObserveCommand TapCommand { get; }

    public IObserveCommand ViewCommand { get; }

    public IObserveCommand CancelCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIFilerViewModel(TimeProvider timeProvider, IDispatcher dispatcher)
    {
        Drive = DriveSample.LoadDrive(timeProvider.GetLocalNow().DateTime);

        TapCommand = MakeDelegateCommand<DriveEntry>(CommandMode.Simple, Toggle);
        ViewCommand = MakeDelegateCommand(() => IsGrid = !IsGrid);
        CancelCommand = MakeDelegateCommand(() => Drive.Transfer.IsActive = false);

        timer = dispatcher.CreateTimer();
        timer.Interval = Interval;
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        timer.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Drive.SelectAll(SelectedCount < Drive.Entries.Count);
        UpdateSelection();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Message = Drive.Move() ?? "移動先のフォルダがありません";
        UpdateSelection();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        var count = SelectedCount;
        Drive.Remove();
        Message = count > 0 ? $"{count} 項目を削除しました" : "削除する項目がありません";
        UpdateSelection();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Toggle(DriveEntry entry)
    {
        entry.IsSelected = !entry.IsSelected;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        SelectedCount = Drive.SelectedCount;
        SelectedSize = Drive.SelectedSize;
    }

    private void Tick()
    {
        // Dummy
        var transfer = Drive.Transfer;
        if (!transfer.IsActive)
        {
            return;
        }

        var progress = transfer.Progress + 0.04;
        if (progress < 1)
        {
            transfer.Progress = progress;
            return;
        }

        transfer.Progress = 0;
        if (transfer.Current < transfer.Total)
        {
            transfer.Current++;
        }
        else
        {
            transfer.IsActive = false;
            Message = "アップロードが完了しました";
        }
    }
}
