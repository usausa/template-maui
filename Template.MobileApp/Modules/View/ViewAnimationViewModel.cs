namespace Template.MobileApp.Modules.View;

public sealed partial class ViewAnimationViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial bool LoopEnabled { get; set; } = true;

    [ObservableProperty]
    public partial int JumpCount { get; set; }

    [ObservableProperty]
    public partial int BeatCount { get; set; }

    public IObserveCommand JumpCommand { get; }

    public IObserveCommand BeatCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ViewAnimationViewModel()
    {
        JumpCommand = MakeDelegateCommand(() => JumpCount++);
        BeatCommand = MakeDelegateCommand(() => BeatCount++);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
