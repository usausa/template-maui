namespace Template.MobileApp.Modules.Navigation.Shared;

public sealed partial class SharedInputViewModel : AppViewModelBase
{
    [ObservableProperty(NotifyAlso = [nameof(NextName)])]
    public partial ViewId NextViewId { get; set; }

    public string NextName => NextViewId == ViewId.NavigationSharedMain1 ? "Shared1" : "Shared2";

    [ObservableProperty]
    public partial string No { get; set; } = default!;

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            NextViewId = context.Parameter.GetNextViewId();
        }
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(NextViewId, Parameters.Make().WithNo(No));
}
