namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Usecase;

public class NetworkViewModel : AppViewModelBase
{
    private readonly Settings settings;

    private readonly IDialog dialog;

    public ICommand ServerTimeCommand { get; }

    public NetworkViewModel(
        ApplicationState applicationState,
        Settings settings,
        IDialog dialog,
        SampleUsecase sampleUsecase)
        : base(applicationState)
    {
        this.settings = settings;
        this.dialog = dialog;

        ServerTimeCommand = MakeAsyncCommand(async () =>
        {
            var result = await sampleUsecase.GetServerTimeAsync();
            if (result.IsSuccess)
            {
                await dialog.InformationAsync($"Access success.\r\ntime=[{result.Value.DateTime:yyyy/MM/dd HH:mm:ss}]");
            }
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

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
