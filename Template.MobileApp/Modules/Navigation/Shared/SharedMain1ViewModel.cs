namespace Template.MobileApp.Modules.Navigation.Shared;

public sealed partial class SharedMain1ViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string No { get; set; } = default!;

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            No = context.Parameter.GetNo();
        }
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() =>
        Navigator.ForwardAsync(ViewId.NavigationSharedInput, Parameters.MakeNextViewId(ViewId.NavigationSharedMain1));

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationMenu);
}
