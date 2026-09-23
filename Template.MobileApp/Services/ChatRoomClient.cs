namespace Template.MobileApp.Services;

using System.Net.Sockets;
using System.Threading.Channels;

using Grpc.Core;
using Grpc.Net.Client;

//--------------------------------------------------------------------------------
// Events
//--------------------------------------------------------------------------------

public enum ChatConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

public sealed record ChatMessageEntry(string User, string Text, DateTime Timestamp);

public sealed class ChatMessageEventArgs : EventArgs
{
    public ChatMessageEntry Entry { get; }

    public ChatMessageEventArgs(ChatMessageEntry entry)
    {
        Entry = entry;
    }
}

public sealed class ChatStateEventArgs : EventArgs
{
    public ChatConnectionState State { get; }

    public ChatStateEventArgs(ChatConnectionState state)
    {
        State = state;
    }
}

//--------------------------------------------------------------------------------
// Client
//--------------------------------------------------------------------------------

public sealed class ChatRoomClient : IAsyncDisposable
{
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);

    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private readonly ILogger<ChatRoomClient> log;

    private readonly Channel<string> sendChannel = Channel.CreateUnbounded<string>();

    private readonly SemaphoreSlim lifecycleLock = new(1, 1);

    private CancellationTokenSource? cancellation;

    private Task? runTask;

    public event EventHandler<ChatMessageEventArgs>? MessageReceived;

    public event EventHandler<ChatStateEventArgs>? StateChanged;

    public ChatConnectionState State { get; private set; }

    public string? LastError { get; private set; }

    public int PendingCount => sendChannel.Reader.Count;

    public ChatRoomClient(ILogger<ChatRoomClient> log)
    {
        this.log = log;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        lifecycleLock.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    public static async Task<DateTime> GetServerTimeAsync(Uri address, CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(address);
        var client = new ServerInfo.ServerInfoClient(channel);
        var reply = await client.GetServerTimeAsync(new ServerTimeRequest(), cancellationToken: cancellationToken).ConfigureAwait(false);
        return DateTimeOffset.FromUnixTimeMilliseconds(reply.Timestamp).LocalDateTime;
    }

    public async Task ConnectAsync(Uri address, string user)
    {
        await lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await DisconnectCoreAsync().ConfigureAwait(false);

            cancellation = new CancellationTokenSource();
            runTask = RunAsync(address, user, cancellation.Token);
        }
        finally
        {
            lifecycleLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await DisconnectCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            lifecycleLock.Release();
        }
    }

    private async Task DisconnectCoreAsync()
    {
        if ((runTask is null) || (cancellation is null))
        {
            return;
        }

        try
        {
            await cancellation.CancelAsync().ConfigureAwait(false);
            await runTask.ConfigureAwait(false);
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
            runTask = null;
        }
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    public ValueTask SendAsync(string text) =>
        sendChannel.Writer.WriteAsync(text);

    //--------------------------------------------------------------------------------
    // Connection
    //--------------------------------------------------------------------------------

    private async Task RunAsync(Uri address, string user, CancellationToken cancellationToken)
    {
        var connectedOnce = false;
        var delay = InitialRetryDelay;

        while (!cancellationToken.IsCancellationRequested)
        {
            SetState(connectedOnce ? ChatConnectionState.Reconnecting : ChatConnectionState.Connecting, LastError);
            try
            {
                using var channel = GrpcChannel.ForAddress(address);
                var client = new ChatRoom.ChatRoomClient(channel);
                using var call = client.Connect(cancellationToken: cancellationToken);

                // Check the connection by waiting for the response headers
                await call.ResponseHeadersAsync.WaitAsync(cancellationToken).ConfigureAwait(false);
                SetState(ChatConnectionState.Connected, null);
                log.InfoChatConnected(address);
                connectedOnce = true;
                delay = InitialRetryDelay;

                // Run the send loop concurrently
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var sendTask = SendLoopAsync(call.RequestStream, user, linkedCts.Token);
                try
                {
                    // Receive loop
                    await foreach (var message in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                    {
                        MessageReceived?.Invoke(this, new ChatMessageEventArgs(ToEntry(message)));
                    }
                }
                finally
                {
                    await linkedCts.CancelAsync().ConfigureAwait(false);
                    await sendTask.ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (IsConnectionException(ex))
            {
                // Connection failed or disconnected, will retry
                if (!cancellationToken.IsCancellationRequested)
                {
                    log.WarnChatDisconnected(ex);
                    LastError = ex is RpcException rpc ? rpc.Status.Detail : ex.Message;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Exponential backoff for reconnection
            SetState(ChatConnectionState.Reconnecting, LastError);
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxRetryDelay.Ticks));
        }

        SetState(ChatConnectionState.Disconnected, null);
        log.InfoChatDisconnected();
    }

    private async Task SendLoopAsync(IClientStreamWriter<ChatMessage> writer, string user, CancellationToken cancellationToken)
    {
        try
        {
            while (await sendChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (sendChannel.Reader.TryPeek(out var text))
                {
                    await writer.WriteAsync(new ChatMessage { User = user, Text = text }, cancellationToken).ConfigureAwait(false);
                    sendChannel.Reader.TryRead(out _);
                }
            }
        }
        catch (Exception ex) when (IsConnectionException(ex))
        {
            // Disconnected
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static bool IsConnectionException(Exception ex) =>
        ex is RpcException or HttpRequestException or IOException or SocketException or OperationCanceledException or InvalidOperationException;

    private static ChatMessageEntry ToEntry(ChatMessage message) =>
        new(message.User, message.Text, DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp).LocalDateTime);

    private void SetState(ChatConnectionState state, string? error)
    {
        LastError = error;
        if (State != state)
        {
            State = state;
        }

        StateChanged?.Invoke(this, new ChatStateEventArgs(state));
    }
}
