namespace Template.MobileApp.Modules.Network;

using Template.MobileApp.Components;
using Template.MobileApp.Usecase;

public sealed class NetworkMenuViewModel : AppViewModelBase
{
    private const string ApiNotConfigured = "API end point is not configured.";

    private readonly IDialog dialog;

    private readonly Settings settings;

    public IObserveCommand ForwardCommand { get; }
    public IObserveCommand ServerTimeCommand { get; }
    public IObserveCommand RegisterDeviceCommand { get; }

    public NetworkMenuViewModel(
        IDialog dialog,
        IDeviceInfo deviceInfo,
        DeviceInformation deviceInformation,
        Settings settings,
        NetworkUsecase networkUsecase)
    {
        this.dialog = dialog;
        this.settings = settings;

        ForwardCommand = MakeAsyncCommand<ViewId>(ForwardAsync);
        ServerTimeCommand = MakeAsyncCommand(() => ExecuteApiAsync(async () => await networkUsecase.GetServerTimeAsync()));
        RegisterDeviceCommand = MakeAsyncCommand(() => ExecuteApiAsync(async () => await networkUsecase.RegisterDeviceAsync(deviceInformation.DeviceId, deviceInfo.Name)));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private async Task ForwardAsync(ViewId id)
    {
        var message = FindMissingSetting(id);
        if (message is not null)
        {
            await dialog.InformationAsync(message);
        }
        else
        {
            await Navigator.ForwardAsync(id);
        }
    }

    private async Task ExecuteApiAsync(Func<Task> action)
    {
        if (settings.IsApiConfigured())
        {
            await action();
        }
        else
        {
            await dialog.InformationAsync(ApiNotConfigured);
        }
    }

    private string? FindMissingSetting(ViewId id) => id switch
    {
        ViewId.NetworkHttp or ViewId.NetworkAuth or ViewId.NetworkStorage or ViewId.NetworkRealtime when !settings.IsApiConfigured() => ApiNotConfigured,
        ViewId.NetworkGrpc when !settings.IsGrpcConfigured() => "gRPC end point is not configured.",
        ViewId.NetworkSftp when !settings.IsSshConfigured() => "SSH is not configured.",
        ViewId.NetworkTelemetry when !settings.IsOtelConfigured() => "Telemetry end point is not configured.",
        _ => null
    };
}
