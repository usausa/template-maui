namespace Template.MobileApp.Services;

using System.Reactive;

using Microsoft.AspNetCore.SignalR;

using Mofucat.ReactiveHub;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

// Client -> Server
public sealed class DeviceStatusMessage
{
    public string DeviceId { get; set; } = default!;

    public string Model { get; set; } = default!;

    public string Platform { get; set; } = default!;

    public double Battery { get; set; }

    public string BatteryState { get; set; } = default!;

    public string Network { get; set; } = default!;
}

// Server -> Client
public sealed class ServerStatusMessage
{
    public DateTimeOffset Time { get; set; }

    public double CpuPercent { get; set; }

    public long WorkingSet { get; set; }

    public int Connections { get; set; }
}

// Server -> Client
public sealed class NotificationMessage
{
    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    public DateTimeOffset SentAt { get; set; }
}

//--------------------------------------------------------------------------------
// Connection
//--------------------------------------------------------------------------------

public sealed class MonitorConnection : IDisposable
{
    private const string HubPath = "hubs/monitor";

    private readonly ILogger<MonitorConnection> log;

    private readonly IConnectivity connectivity;

    private readonly ReactiveHubConnection hub = new(serverTimeout: TimeSpan.FromSeconds(30), keepAliveInterval: TimeSpan.FromSeconds(15));

    public IObservable<ServerStatusMessage> ServerStatus => hub.On<ServerStatusMessage>("ServerStatus");

    public IObservable<NotificationMessage> Notifications => hub.On<NotificationMessage>("Notify").Do(x => log.InfoMonitorNotified(x.Title));

    public bool IsConnected => hub.IsConnected;

    public MonitorConnection(
        ILogger<MonitorConnection> log,
        IConnectivity connectivity)
    {
        this.log = log;
        this.connectivity = connectivity;
    }

    public void Dispose()
    {
        hub.Dispose();
    }

    public IObservable<HubStatus> Connect(Uri baseAddress)
    {
        var resume = connectivity.ConnectivityChangedAsObservable()
            .Where(static x => x.NetworkAccess == NetworkAccess.Internet)
            .Select(static _ => Unit.Default);

        return hub.Connect(new Uri(baseAddress, HubPath), resume: resume)
            .Do(LogStatus)
            .Finally(log.InfoMonitorStopped);
    }

    public async ValueTask ReportDeviceStatusAsync(DeviceStatusMessage status, CancellationToken cancellationToken = default)
    {
        try
        {
            await hub.TryInvokeAsync("ReportDeviceStatus", status, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HubException or InvalidOperationException or IOException or OperationCanceledException)
        {
            log.WarnMonitorReportFailed(ex);
        }
    }

    private void LogStatus(HubStatus status)
    {
        switch (status.Kind)
        {
            case HubStatusKind.Connected:
                log.InfoMonitorConnected(status.ConnectionId);
                break;
            case HubStatusKind.Reconnecting:
                log.WarnMonitorReconnecting(status.Error);
                break;
            default:
                if (status.Error is not null)
                {
                    log.WarnMonitorConnectFailed(status.Error);
                }

                break;
        }
    }
}
