namespace Template.MobileApp.Services;

using Rester;

public sealed class NetworkService : IDisposable
{
    private HttpClient client;

    private readonly Dictionary<string, object> headers = [];

    public NetworkService()
    {
        client = CreateHttpClient();
    }

    public void Dispose()
    {
        client.Dispose();
    }

    [SuppressMessage("Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Ignore")]
    private static HttpClient CreateHttpClient()
    {
        return new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = static (_, _, _, _) => true
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

    public ValueTask<IRestResponse<ServerTimeResponse>> GetServerTimeAsync() =>
        client.GetAsync<ServerTimeResponse>(
            "api/server/time",
            headers);

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<object>> GetTestErrorAsync(int code) =>
        client.GetAsync<object>(
            $"api/test/error/{code}",
            headers);

    public ValueTask<IRestResponse<object>> GetTestDelayAsync(int timeout) =>
        client.GetAsync<object>(
            $"api/test/delay/{timeout}",
            headers);

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<DataListResponse>> GetDataListAsync() =>
        client.GetAsync<DataListResponse>(
            "api/data/list",
            headers);

    //--------------------------------------------------------------------------------
    // Storage
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse> DownloadAsync(string path, string filename, Action<double> action)
    {
        var progress = -1d;
        return client.DownloadAsync(
            $"api/storage/{path}",
            filename,
            progress: (processed, total) =>
            {
                var percent = Math.Floor((double)processed / total * 100);
                if (percent > progress)
                {
                    progress = percent;
                    action(percent);
                }
            });
    }

    public ValueTask<IRestResponse> DownloadAsync(string path, Stream stream, Action<double> action)
    {
        var progress = -1d;
        return client.DownloadAsync(
            $"api/storage/{path}",
            stream,
            progress: (processed, total) =>
            {
                var percent = Math.Floor((double)processed / total * 100);
                if (percent > progress)
                {
                    progress = percent;
                    action(percent);
                }
            });
    }

    public ValueTask<IRestResponse> UploadAsync(string path, string filename, Action<double> action)
    {
        var progress = -1d;
        return client.UploadAsync(
            $"api/storage/{path}",
            filename,
            compress: CompressOption.Gzip,
            progress: (processed, total) =>
            {
                var percent = Math.Floor((double)processed / total * 100);
                if (percent > progress)
                {
                    progress = percent;
                    action(percent);
                }
            });
    }

    public ValueTask<IRestResponse> UploadAsync(string path, Stream stream, Action<double> action)
    {
        var progress = -1d;
        return client.UploadAsync(
            $"api/storage/{path}",
            stream,
            compress: CompressOption.Gzip,
            progress: (processed, total) =>
            {
                var percent = Math.Floor((double)processed / total * 100);
                if (percent > progress)
                {
                    progress = percent;
                    action(percent);
                }
            });
    }
}
