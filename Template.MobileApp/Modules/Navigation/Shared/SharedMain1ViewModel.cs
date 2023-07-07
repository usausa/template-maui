namespace Template.MobileApp.Modules.Navigation.Shared;

using Template.MobileApp;

public class SharedMain1ViewModel : AppViewModelBase
{
    public NotificationValue<string> No { get; } = new();

    public SharedMain1ViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    public override void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            No.Value = context.Parameter.GetNo();
        }
    }

    protected override Task OnNotifyBackAsync() =>
        Navigator.ForwardAsync(ViewId.NavigationSharedInput, Parameters.MakeNextViewId(ViewId.NavigationSharedMain1));

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationMenu);
}
