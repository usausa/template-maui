namespace Template.MobileApp.Modules.Navigation.Effect;

public sealed class EffectMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }
    public IObserveCommand PushCommand { get; }
    public IObserveCommand DialogCommand { get; }

    public EffectMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<string>(x => Navigator.ForwardAsync(ViewId.NavigationEffectDemo, new NavigationParameter().WithEffect(x)));
        PushCommand = MakeAsyncCommand<string>(x => Navigator.PushAsync(ViewId.NavigationEffectDemo, new NavigationParameter().WithEffect(x)));
        DialogCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.NavigationEffectDialog));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
