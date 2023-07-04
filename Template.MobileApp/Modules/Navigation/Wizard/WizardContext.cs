namespace Template.MobileApp.Modules.Navigation.Wizard;

public sealed class WizardContext : IInitializable, IDisposable
{
    private readonly ILogger<WizardContext> log;

    public WizardContext(ILogger<WizardContext> log)
    {
        this.log = log;
    }

    public string? Data1 { get; set; }

    public string? Data2 { get; set; }

    public void Initialize()
    {
        // TODO Extension
#pragma warning disable CA1848
        log.LogInformation("**** WizardContext Initialize ****");
#pragma warning restore CA1848
    }

    public void Dispose()
    {
#pragma warning disable CA1848
        log.LogInformation("**** WizardContext Dispose ****");
#pragma warning restore CA1848
    }
}
