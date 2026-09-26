namespace Template.MobileApp.Modules.Navigation.Effect;

public sealed partial class EffectDialogViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string AppliedEffect { get; private set; } = "(none)";

    public IObserveCommand CloseCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public EffectDialogViewModel()
    {
        CloseCommand = MakeAsyncCommand(OnNotifyBackAsync);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            AppliedEffect = context.Parameter.Effect ?? "(none)";
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationEffectMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
