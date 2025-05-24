namespace Template.MobileApp.Modules.Navigation.Wizard;

public sealed partial class WizardResultViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial WizardContext Context { get; set; } = default!;

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationWizardInput2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.Menu);
}
