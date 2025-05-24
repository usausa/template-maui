namespace Template.MobileApp.Modules.Navigation.Shared;

public sealed partial class SharedInputViewModel : AppViewModelBase
{
    private ViewId nextViewId;

    [ObservableProperty]
    public partial string No { get; set; } = default!;

    public override void OnNavigatedTo(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            nextViewId = context.Parameter.GetNextViewId();
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(nextViewId, Parameters.MakeNextViewId(nextViewId).WithNo(No));
}
