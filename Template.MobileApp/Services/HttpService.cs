namespace Template.MobileApp.Services;

using Rester;

public sealed class HttpService
{
    // IHttpClientFactoryが返すクライアントはハンドラがプール管理されるためDispose不要
    private readonly IHttpClientFactory httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    //--------------------------------------------------------------------------------
    // Basic
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<ServerTimeResponse>> GetServerTimeAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<ServerTimeResponse>("api/server/time", cancel: cancellationToken);
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<DataListResponse>> GetDataListAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<DataListResponse>("api/data/list", cancel: cancellationToken);
    }

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<SecretMessageResponse>> GetSecretMessageAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<SecretMessageResponse>("api/secret/message", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<AccountLoginResponse>> PostAccountLoginAsync(AccountLoginRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PostAsync<AccountLoginResponse>("api/account/login", request, cancel: cancellationToken);
    }

    //--------------------------------------------------------------------------------
    // Storage
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse> DownloadAsync(string path, string filename, Action<double> action, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Transfer);
        return client.DownloadAsync(
            $"api/storage/{path}",
            filename,
            progress: CreateProgressCallback(action),
            cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> DownloadAsync(string path, Stream stream, Action<double> action, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Transfer);
        return client.DownloadAsync(
            $"api/storage/{path}",
            stream,
            progress: CreateProgressCallback(action),
            cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> UploadAsync(string path, string filename, Action<double> action, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Transfer);
        return client.UploadAsync(
            $"api/storage/{path}",
            filename,
            compress: CompressOption.Gzip,
            progress: CreateProgressCallback(action),
            cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> UploadAsync(string path, Stream stream, Action<double> action, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Transfer);
        return client.UploadAsync(
            $"api/storage/{path}",
            stream,
            compress: CompressOption.Gzip,
            progress: CreateProgressCallback(action),
            cancel: cancellationToken);
    }

    private static Action<long, long> CreateProgressCallback(Action<double> action)
    {
        var progress = -1d;
        return (processed, total) =>
        {
            // Content-Length不明時は進捗を通知しない
            if (total <= 0)
            {
                return;
            }

            var percent = Math.Floor((double)processed / total * 100);
            if (percent > progress)
            {
                progress = percent;
                action(percent);
            }
        };
    }

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<object>> GetTestErrorAsync(int code, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<object>($"api/test/error/{code}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<object>> GetTestDelayAsync(int timeout, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<object>($"api/test/delay/{timeout}", cancel: cancellationToken);
    }
}
