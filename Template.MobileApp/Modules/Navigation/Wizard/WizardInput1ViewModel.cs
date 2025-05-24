namespace Template.MobileApp.Modules.Navigation.Wizard;

public sealed partial class WizardInput1ViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial WizardContext Context { get; set; } = default!;

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationWizardInput2);
}
