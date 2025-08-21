namespace Template.MobileApp.Modules.View;

public sealed class ViewEasingViewModel : AppViewModelBase
{
    public EventRequest AnimationRequest { get; } = new();

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4()
    {
        AnimationRequest.Request();
        return Task.CompletedTask;
    }
}
