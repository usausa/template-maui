namespace Template.MobileApp.Services;

public sealed class PushService : IDisposable
{
    private readonly ILogger<PushService> log;

    private readonly ApiContext apiContext;

    private readonly IDialog dialog;

    private readonly PushConnection connection;

    private IDisposable? receiving;

    private IDisposable? connecting;

    private Uri? connectedAddress;

    public PushService(
        ILogger<PushService> log,
        ApiContext apiContext,
        IDialog dialog,
        PushConnection connection)
    {
        this.log = log;
        this.apiContext = apiContext;
        this.dialog = dialog;
        this.connection = connection;
    }

    public void Dispose()
    {
        connecting?.Dispose();
        receiving?.Dispose();
    }

    public void Connect()
    {
        var baseAddress = apiContext.BaseAddress;
        if (baseAddress is null)
        {
            log.WarnPushNotConfigured();
        }
        else
        {
            // Receive before connecting (the pending messages are sent right after the connection)
            receiving ??= connection.Messages.ObserveOnCurrentContext().Subscribe(OnMessage);
            if (connectedAddress != baseAddress)
            {
                connectedAddress = baseAddress;
                connecting?.Dispose();
                connecting = connection.Connect(baseAddress);
                log.InfoPushServiceStarted(baseAddress);
            }
        }
    }

    public void Disconnect()
    {
        if (connecting is not null)
        {
            connecting.Dispose();
            connecting = null;
            log.InfoPushServiceStopped();
        }

        receiving?.Dispose();
        receiving = null;
        connectedAddress = null;
    }

    private void OnMessage(PushMessage message)
    {
        if (connection.TryMarkShown(message.Id))
        {
            _ = dialog.Toast(FormatToast(message), true).AsTask();
            log.InfoPushShown(message.Id);
        }

        _ = connection.AcknowledgeAsync(message.Id).AsTask();
    }

    private static string FormatToast(PushMessage message) =>
        String.IsNullOrEmpty(message.Body) ? message.Title : $"{message.Title}: {message.Body}";
}
