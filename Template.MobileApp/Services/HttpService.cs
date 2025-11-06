namespace Template.MobileApp.Services;

using Rester;

public sealed class HttpService
{
    private readonly IHttpClientFactory httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    //--------------------------------------------------------------------------------
    // Basic
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse<ServerTimeResponse>> GetServerTimeAsync()
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.GetAsync<ServerTimeResponse>("api/server/time");
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse<DataListResponse>> GetDataListAsync()
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.GetAsync<DataListResponse>("api/data/list");
    }

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse<SecretMessageResponse>> GetSecretMessageAsync()
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.GetAsync<SecretMessageResponse>("api/secret/message");
    }

    public async ValueTask<IRestResponse<AccountLoginResponse>> PostAccountLoginAsync(AccountLoginRequest request)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.PostAsync<AccountLoginResponse>("api/account/login", request);
    }

    //--------------------------------------------------------------------------------
    // Storage
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse> DownloadAsync(string path, string filename, Action<double> action)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        var progress = -1d;
        return await client.DownloadAsync(
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

    public async ValueTask<IRestResponse> DownloadAsync(string path, Stream stream, Action<double> action)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        var progress = -1d;
        return await client.DownloadAsync(
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

    public async ValueTask<IRestResponse> UploadAsync(string path, string filename, Action<double> action)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        var progress = -1d;
        return await client.UploadAsync(
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

    public async ValueTask<IRestResponse> UploadAsync(string path, Stream stream, Action<double> action)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        var progress = -1d;
        return await client.UploadAsync(
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

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public async ValueTask<IRestResponse<object>> GetTestErrorAsync(int code)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.GetAsync<object>($"api/test/error/{code}");
    }

    public async ValueTask<IRestResponse<object>> GetTestDelayAsync(int timeout)
    {
        using var client = httpClientFactory.CreateClient(ApiNames.Default);
        return await client.GetAsync<object>($"api/test/delay/{timeout}");
    }
}
