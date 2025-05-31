namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Services;
using Template.MobileApp.Usecase;

public sealed class NetworkMenuViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly ApiContext apiContext;

    public IObserveCommand ServerTimeCommand { get; }
    public IObserveCommand TestErrorCommand { get; }
    public IObserveCommand TestDelayCommand { get; }
    public IObserveCommand DataListCommand { get; }
    public IObserveCommand DownloadCommand { get; }
    public IObserveCommand UploadCommand { get; }

    public NetworkMenuViewModel(
        IDialog dialog,
        ApiContext apiContext,
        SampleUsecase sampleUsecase)
    {
        this.apiContext = apiContext;
        this.dialog = dialog;

        ServerTimeCommand = MakeAsyncCommand(async () => await sampleUsecase.GetServerTimeAsync());
        TestErrorCommand = MakeAsyncCommand<int>(async x => await sampleUsecase.GetTestErrorAsync(x));
        TestDelayCommand = MakeAsyncCommand<int>(async x => await sampleUsecase.GetTestDelayAsync(x));
        DataListCommand = MakeAsyncCommand(async () => await sampleUsecase.GetDataListAsync());
        DownloadCommand = MakeAsyncCommand(async () => await sampleUsecase.DownloadAsync());
        UploadCommand = MakeAsyncCommand(async () => await sampleUsecase.UploadAsync());
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // ReSharper disable once AsyncVoidMethod
    public override async void OnNavigatedTo(INavigationContext context)
    {
        if (apiContext.BaseAddress is null)
        {
            await Navigator.PostActionAsync(() => BusyState.Using(async () =>
            {
                await dialog.InformationAsync("API EndPoint not configured.");

                await Navigator.ForwardAsync(ViewId.Menu);
            }));
        }
    }
}
