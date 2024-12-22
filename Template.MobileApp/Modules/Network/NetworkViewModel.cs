namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public class NetworkViewModel : AppViewModelBase
{
    private readonly Settings settings;

    private readonly IDialog dialog;

    public ICommand ServerTimeCommand { get; }
    public ICommand TestErrorCommand { get; }
    public ICommand TestDelayCommand { get; }
    public ICommand DataListCommand { get; }
    public ICommand DownloadCommand { get; }
    public ICommand UploadCommand { get; }

    public NetworkViewModel(
        ApplicationState applicationState,
        Settings settings,
        IDialog dialog,
        SampleUsecase sampleUsecase)
        : base(applicationState)
    {
        this.settings = settings;
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
        if (String.IsNullOrEmpty(settings.ApiEndPoint))
        {
            await Navigator.PostActionAsync(() => BusyState.Using(async () =>
            {
                await dialog.InformationAsync("API EndPoint not configured.");

                await Navigator.ForwardAsync(ViewId.Menu);
            }));
        }
    }
}
