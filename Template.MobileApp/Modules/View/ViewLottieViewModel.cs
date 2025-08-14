namespace Template.MobileApp.Modules.View;

// ref https://lottiefiles.com/
public sealed partial class ViewLottieViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial bool IsAnimationEnabled { get; set; }

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; }

    [ObservableProperty]
    public partial TimeSpan Progress { get; set; }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        Progress = TimeSpan.Zero;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        IsAnimationEnabled = !IsAnimationEnabled;
        return Task.CompletedTask;
    }
}
