namespace Template.MobileApp.Modules.Navigation.Shared;

using Template.MobileApp;

public class SharedInputViewModel : AppViewModelBase
{
    private ViewId nextViewId;

    public NotificationValue<string> No { get; } = new();

    public SharedInputViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    public override void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            nextViewId = context.Parameter.GetNextViewId();
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(nextViewId, Parameters.MakeNextViewId(nextViewId).WithNo(No.Value));
}
