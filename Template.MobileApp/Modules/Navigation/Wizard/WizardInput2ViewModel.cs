namespace Template.MobileApp.Modules.Navigation.Wizard;

using Template.MobileApp;

public class WizardInput2ViewModel : AppViewModelBase
{
    [Scope]
    public NotificationValue<WizardContext> Context { get; } = new();

    public WizardInput2ViewModel(ApplicationState applicationState)
        : base(applicationState)
    {
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationWizardInput1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationWizardResult);
}
