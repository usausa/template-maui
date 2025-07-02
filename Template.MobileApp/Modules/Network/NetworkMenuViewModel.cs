namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Services;
using Template.MobileApp.Usecase;

public sealed class NetworkMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }
    public IObserveCommand ServerTimeCommand { get; }
    public IObserveCommand DataListCommand { get; }
    public IObserveCommand SecureCommand { get; }
    public IObserveCommand LoginCommand { get; }
    public IObserveCommand LogoutCommand { get; }
    public IObserveCommand TestErrorCommand { get; }
    public IObserveCommand TestDelayCommand { get; }
    public IObserveCommand DownloadCommand { get; }
    public IObserveCommand UploadCommand { get; }

    public NetworkMenuViewModel(
        ApiContext apiContext,
        NetworkUsecase networkUsecase)
    {
        var configured = apiContext.BaseAddress is not null;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        ServerTimeCommand = MakeAsyncCommand(async () => await networkUsecase.GetServerTimeAsync(), () => configured);
        DataListCommand = MakeAsyncCommand(async () => await networkUsecase.GetDataListAsync(), () => configured);
        DownloadCommand = MakeAsyncCommand(async () => await networkUsecase.DownloadAsync(), () => configured);
        SecureCommand = MakeAsyncCommand(async () => await networkUsecase.GetSecretMessageAsync(), () => configured);
        LoginCommand = MakeAsyncCommand(async () => await networkUsecase.PostAccountLoginAsync("user"), () => configured);
        LogoutCommand = MakeDelegateCommand(networkUsecase.AccountLogout, () => configured);
        UploadCommand = MakeAsyncCommand(async () => await networkUsecase.UploadAsync(), () => configured);
        TestErrorCommand = MakeAsyncCommand<int>(async x => await networkUsecase.GetTestErrorAsync(x), _ => configured);
        TestDelayCommand = MakeAsyncCommand<int>(async x => await networkUsecase.GetTestDelayAsync(x), _ => configured);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
