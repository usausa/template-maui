namespace Template.MobileApp.Services;

using System.Reactive;

using Microsoft.AspNetCore.SignalR;

using Mofucat.ReactiveHub;

using Template.MobileApp.Components;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

// Server -> Client
public sealed class PushMessage
{
    public long Id { get; set; }

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}

public enum PushStatus
{
    Stopped,
    Connecting,
    Connected,
    Retrying
}

//--------------------------------------------------------------------------------
// Connection
//--------------------------------------------------------------------------------

public sealed class PushConnection : IDisposable
{
    private const string HubPath = "hubs/push";

    private const int ShownCapacity = 64;

    private readonly Lock sync = new();

    private readonly long[] shown = new long[ShownCapacity];

    private readonly ILogger<PushConnection> log;

    private readonly DeviceInformation deviceInformation;

    private readonly ReactiveHubConnection hub = new(serverTimeout: TimeSpan.FromSeconds(30), keepAliveInterval: TimeSpan.FromSeconds(15));

    private int shownIndex;

    public IObservable<PushStatus> Status => hub.Status.Select(ToStatus).DistinctUntilChanged();

    public IObservable<PushMessage> Messages => hub.On<PushMessage>("Receive").Do(x => log.InfoPushReceived(x.Id, x.Title));

    public PushConnection(
        ILogger<PushConnection> log,
        DeviceInformation deviceInformation)
    {
        this.log = log;
        this.deviceInformation = deviceInformation;
    }

    public void Dispose()
    {
        hub.Dispose();
    }

    public IDisposable Connect(Uri baseAddress)
    {
        var resume = deviceInformation.NetworkChangedAsObservable()
            .Where(_ => deviceInformation.Network?.Access == NetworkAccess.Internet)
            .Select(static _ => Unit.Default);

        return hub.Connect(new Uri(baseAddress, $"{HubPath}?deviceId={Uri.EscapeDataString(deviceInformation.DeviceId)}"), resume: resume)
            .Finally(log.InfoPushStopped)
            .Subscribe(LogStatus, log.WarnPushConnectionError);
    }

    public bool TryMarkShown(long id)
    {
        lock (sync)
        {
            if (Array.IndexOf(shown, id) >= 0)
            {
                return false;
            }

            shown[shownIndex] = id;
            shownIndex = (shownIndex + 1) % shown.Length;
            return true;
        }
    }

    public async ValueTask AcknowledgeAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            await hub.TrySendAsync("Acknowledge", id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HubException or InvalidOperationException or IOException or OperationCanceledException)
        {
            log.WarnPushAcknowledgeFailed(ex);
        }
    }

    private void LogStatus(HubStatus status)
    {
        switch (status.Kind)
        {
            case HubStatusKind.Connected:
                log.InfoPushConnected(status.ConnectionId);
                break;
            case HubStatusKind.Reconnecting:
                log.WarnPushReconnecting(status.Error);
                break;
            default:
                if (status.Error is not null)
                {
                    log.WarnPushConnectFailed(status.Error);
                }

                break;
        }
    }

    private static PushStatus ToStatus(HubStatus status) => status.Kind switch
    {
        HubStatusKind.Connected => PushStatus.Connected,
        HubStatusKind.Disconnected => PushStatus.Stopped,
        _ => status.Error is null ? PushStatus.Connecting : PushStatus.Retrying
    };
}
