namespace Template.MobileApp.Services;

using Rester;

public sealed class NetworkService : IDisposable
{
    private HttpClient client;

    private readonly Dictionary<string, object> headers = new();

    public NetworkService()
    {
        client = CreateHttpClient();
    }

    public void Dispose()
    {
        client.Dispose();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Ignore")]
    private static HttpClient CreateHttpClient()
    {
        return new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
        {
            Timeout = new TimeSpan(0, 0, 0, 30)
        };
    }

    public void SetEndPoint(string address)
    {
        if (client.BaseAddress is not null)
        {
            client.Dispose();
            client = CreateHttpClient();
        }

        client.BaseAddress = String.IsNullOrEmpty(address) ? null : new Uri(address);
    }

    public void SetToken(string token)
    {
        headers["X-API-Token"] = token;
    }

    //--------------------------------------------------------------------------------
    // Basic
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse<ServerTimeResponse>> GetServerTimeAsync()
    {
        return await client.GetAsync<ServerTimeResponse>(
            "api/server/time",
            headers);
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    // TODO

    //--------------------------------------------------------------------------------
    // Storage
    //--------------------------------------------------------------------------------

    // TODO
}
