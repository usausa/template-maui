namespace Template.MobileApp.Modules.View;

public sealed partial class ViewEasingViewModel : AppViewModelBase
{
    public EventRequest AnimationRequest { get; } = new();

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override async Task OnNotifyFunction4()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        AnimationRequest.Request();
        await Task.Delay(2200);
        IsRunning = false;
    }
}
