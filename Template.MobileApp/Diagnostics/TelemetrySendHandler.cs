namespace Template.MobileApp.Diagnostics;

using System.Net;

using Template.MobileApp.Helpers;

internal sealed class TelemetrySendHandler : DelegatingHandler
{
    private readonly Lock sync = new();

    private readonly Store[] stores;

    private readonly TimeSpan resendTimeout;

    private readonly Action<bool, string?>? sent;

    private long succeededCount;

    private long sequence;

    private int resending;

    private volatile bool disposed;

    public long SucceededCount => Interlocked.Read(ref succeededCount);

    public int WaitingCount
    {
        get
        {
            lock (sync)
            {
                return stores.Sum(static x => x.Payloads.Count);
            }
        }
    }

    public TelemetrySendHandler(HttpMessageHandler innerHandler, IEnumerable<KeyValuePair<string, int>> capacities, TimeSpan resendTimeout, Action<bool, string?>? sent)
        : base(innerHandler)
    {
        stores = capacities
            .Where(static x => x.Value > 0)
            .Select(static x => new Store(x.Key, new RingBuffer<Payload>(x.Value)))
            .ToArray();
        this.resendTimeout = resendTimeout;
        this.sent = sent;
    }

    protected override void Dispose(bool disposing)
    {
        disposed = true;
        base.Dispose(disposing);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await KeepAsync(request).ConfigureAwait(false);
            sent?.Invoke(false, ex.Message);
            throw;
        }

        if (response.IsSuccessStatusCode)
        {
            Interlocked.Increment(ref succeededCount);
            sent?.Invoke(true, null);
            StartResend();
        }
        else
        {
            if (IsRetryable(response.StatusCode))
            {
                await KeepAsync(request).ConfigureAwait(false);
            }

            sent?.Invoke(false, $"HTTP {(int)response.StatusCode}");
        }

        return response;
    }

    private async Task KeepAsync(HttpRequestMessage request)
    {
        if ((request.Content is null) || (request.RequestUri is null) || (FindStore(request.RequestUri) is not { } store))
        {
            return;
        }

        var body = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var headers = request.Content.Headers
            .Where(static x => !String.Equals(x.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        lock (sync)
        {
            sequence++;
            store.Payloads.Add(new Payload(sequence, request.RequestUri, headers, body));
        }
    }

    private Store? FindStore(Uri uri)
    {
        foreach (var store in stores)
        {
            if (uri.AbsolutePath.EndsWith(store.Path, StringComparison.Ordinal))
            {
                return store;
            }
        }

        return null;
    }

    private void StartResend()
    {
        if ((WaitingCount == 0) || (Interlocked.Exchange(ref resending, 1) == 1))
        {
            return;
        }

        _ = Task.Run(ResendAllAsync);
    }

    private async Task ResendAllAsync()
    {
        try
        {
            while (!disposed && (PeekOldest() is { } oldest))
            {
                if (!await ResendAsync(oldest.Payload).ConfigureAwait(false))
                {
                    return;
                }

                lock (sync)
                {
                    var payloads = oldest.Store.Payloads;
                    if ((payloads.Count > 0) && ReferenceEquals(payloads[0], oldest.Payload))
                    {
                        payloads.RemoveFirst();
                    }
                }
            }
        }
        finally
        {
            Volatile.Write(ref resending, 0);
        }
    }

    private (Store Store, Payload Payload)? PeekOldest()
    {
        lock (sync)
        {
            Store? oldest = null;
            foreach (var store in stores)
            {
                if ((store.Payloads.Count > 0) && ((oldest is null) || (store.Payloads[0].Sequence < oldest.Payloads[0].Sequence)))
                {
                    oldest = store;
                }
            }

            return oldest is null ? null : (oldest, oldest.Payloads[0]);
        }
    }

    private async Task<bool> ResendAsync(Payload payload)
    {
        try
        {
            using var timeout = new CancellationTokenSource(resendTimeout);
            using var request = payload.CreateRequest();
            using var response = await base.SendAsync(request, timeout.Token).ConfigureAwait(false);
            return response.IsSuccessStatusCode || !IsRetryable(response.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or WebException or IOException or OperationCanceledException or ObjectDisposedException)
        {
            return false;
        }
    }

    private static bool IsRetryable(HttpStatusCode code) =>
        code is HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

    private sealed record Store(string Path, RingBuffer<Payload> Payloads);

    private sealed class Payload
    {
        private readonly Uri uri;

        private readonly KeyValuePair<string, IEnumerable<string>>[] headers;

        private readonly byte[] body;

        public long Sequence { get; }

        public Payload(long sequence, Uri uri, KeyValuePair<string, IEnumerable<string>>[] headers, byte[] body)
        {
            Sequence = sequence;
            this.uri = uri;
            this.headers = headers;
            this.body = body;
        }

        public HttpRequestMessage CreateRequest()
        {
            var content = new ByteArrayContent(body);
            foreach (var header in headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        }
    }
}
