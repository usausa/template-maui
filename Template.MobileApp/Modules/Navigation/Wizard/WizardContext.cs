namespace Template.MobileApp.Modules.Navigation.Wizard;

public sealed partial class WizardContext : ObservableObject, IScopeLifecycle
{
    private readonly ILogger<WizardContext> log;

    public WizardContext(ILogger<WizardContext> log)
    {
        this.log = log;
    }

    [ObservableProperty]
    public partial string? Data1 { get; set; }

    [ObservableProperty]
    public partial string? Data2 { get; set; }

    public void OnScopeInitialize()
    {
        // TODO Extension
#pragma warning disable CA1848
        log.LogInformation("**** WizardContext OnScopeInitialize ****");
#pragma warning restore CA1848
    }

    public void OnScopeTerminate()
    {
#pragma warning disable CA1848
        log.LogInformation("**** WizardContext OnScopeTerminate ****");
#pragma warning restore CA1848
    }
}
