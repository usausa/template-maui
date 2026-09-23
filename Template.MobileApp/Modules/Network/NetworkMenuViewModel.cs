namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public sealed class NetworkMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }
    public IObserveCommand ServerTimeCommand { get; }

    public NetworkMenuViewModel(
        Settings settings,
        NetworkUsecase networkUsecase)
    {
        var configured = settings.IsApiConfigured();

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        ServerTimeCommand = MakeAsyncCommand(async () => await networkUsecase.GetServerTimeAsync(), () => configured);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
