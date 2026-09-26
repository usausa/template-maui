namespace Template.MobileApp.Diagnostics;

using System.Net;

using Template.MobileApp.Helpers;

internal sealed class TelemetrySendHandler : DelegatingHandler
{
    private const int ResendPerSend = 5;

    private readonly Lock sync = new();

    private readonly RingBuffer<Payload>? payloads;

    private readonly Action<bool, string?>? sent;

    private long succeededCount;

    private int resending;

    public long SucceededCount => Interlocked.Read(ref succeededCount);

    public int WaitingCount
    {
        get
        {
            lock (sync)
            {
                return payloads?.Count ?? 0;
            }
        }
    }

    public TelemetrySendHandler(HttpMessageHandler innerHandler, int capacity, Action<bool, string?>? sent)
        : base(innerHandler)
    {
        payloads = capacity > 0 ? new RingBuffer<Payload>(capacity) : null;
        this.sent = sent;
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
            await ResendAsync(cancellationToken).ConfigureAwait(false);
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
        if ((payloads is null) || (request.Content is null) || (request.RequestUri is null))
        {
            return;
        }

        var body = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var headers = request.Content.Headers
            .Where(static x => !String.Equals(x.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        lock (sync)
        {
            payloads.Add(new Payload(request.RequestUri, headers, body));
        }
    }

    private async Task ResendAsync(CancellationToken cancellationToken)
    {
        if ((payloads is null) || (Interlocked.Exchange(ref resending, 1) == 1))
        {
            return;
        }

        try
        {
            await ResendCoreAsync(payloads, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Volatile.Write(ref resending, 0);
        }
    }

    private async Task ResendCoreAsync(RingBuffer<Payload> buffer, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ResendPerSend; i++)
        {
            Payload payload;
            lock (sync)
            {
                if (buffer.Count == 0)
                {
                    return;
                }

                payload = buffer[0];
            }

            try
            {
                using var request = payload.CreateRequest();
                using var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode && IsRetryable(response.StatusCode))
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or WebException or IOException or OperationCanceledException)
            {
                return;
            }

            lock (sync)
            {
                if ((buffer.Count > 0) && ReferenceEquals(buffer[0], payload))
                {
                    buffer.RemoveFirst();
                }
            }
        }
    }

    private static bool IsRetryable(HttpStatusCode code) =>
        code is HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

    private sealed class Payload
    {
        private readonly Uri uri;

        private readonly KeyValuePair<string, IEnumerable<string>>[] headers;

        private readonly byte[] body;

        public Payload(Uri uri, KeyValuePair<string, IEnumerable<string>>[] headers, byte[] body)
        {
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
