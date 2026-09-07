namespace Template.MobileApp.Modules.Navigation.Effect;

using Template.MobileApp.Extender.Effects;

public sealed partial class EffectDemoViewModel : AppViewModelBase
{
    private const string NoEffect = "(none)";

    private string? effect;

    private bool stacked;

    [ObservableProperty]
    public partial string PlayedEffect { get; private set; } = NoEffect;

    [ObservableProperty]
    public partial string ReturnEffect { get; private set; } = NoEffect;

    [ObservableProperty]
    public partial string NavigationMode { get; private set; } = string.Empty;

    public IObserveCommand BackCommand { get; }
    public IObserveCommand ReplayCommand { get; }

    public EffectDemoViewModel()
    {
        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        ReplayCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.NavigationEffectDemo, MakeParameter(effect)));
    }

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        effect = context.Parameter.Effect;
        stacked = context.Attribute.IsStacked() || (Navigator.StackedCount > 1);

        PlayedEffect = effect ?? NoEffect;
        ReturnEffect = effect is null ? NoEffect : AppEffect.Reverse(effect);
        NavigationMode = stacked ? "Push (stacked)" : "Forward";

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync()
    {
        var parameter = MakeParameter(effect is null ? null : AppEffect.Reverse(effect));
        return stacked
            ? Navigator.PopAsync(parameter)
            : Navigator.ForwardAsync(ViewId.NavigationEffectMenu, parameter);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private static NavigationParameter MakeParameter(string? effect)
    {
        var parameter = new NavigationParameter();
        return effect is null ? parameter : parameter.WithEffect(effect);
    }
}
