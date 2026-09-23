namespace Template.MobileApp.Services;

using Rester;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public class AccountLoginRequest
{
    public string Id { get; set; } = default!;
}

public class AccountLoginResponse
{
    public string Token { get; set; } = default!;
}

public sealed class ServerTimeResponse
{
    public DateTime DateTime { get; set; }
}

public class SecretMessageResponse
{
    public string Message { get; set; } = default!;
}

public sealed class DataListEntry
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;
}

#pragma warning disable CA1819
public sealed class DataListResponse
{
    public DataListEntry[] Entries { get; set; } = default!;

    public int Total { get; set; }
}
#pragma warning restore CA1819

public sealed class DataResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public int Value { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class DataCreateRequest
{
    public string Name { get; set; } = default!;

    public int Value { get; set; }
}

public sealed class DataCreateResponse
{
    public long Id { get; set; }
}

public sealed class DataUpdateRequest
{
    public string Name { get; set; } = default!;

    public int Value { get; set; }
}

public sealed class StorageListEntry
{
    public string Name { get; set; } = default!;

    public bool Directory { get; set; }

    // ディレクトリは null
    public long? Size { get; set; }

    public DateTime LastModified { get; set; }
}

#pragma warning disable CA1819
public sealed class StorageListResponse
{
    public StorageListEntry[] Entries { get; set; } = default!;
}
#pragma warning restore CA1819

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class HttpService
{
    private readonly IHttpClientFactory httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    //--------------------------------------------------------------------------------
    // Account
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<AccountLoginResponse>> PostAccountLoginAsync(AccountLoginRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PostAsync<AccountLoginResponse>("api/account/login", request, cancel: cancellationToken);
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

    public ValueTask<IRestResponse<DataListResponse>> GetDataListAsync(int offset, int size, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<DataListResponse>($"api/data/list?offset={offset}&size={size}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<DataResponse>> GetDataAsync(long id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<DataResponse>($"api/data/{id}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<DataCreateResponse>> PostDataAsync(DataCreateRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PostAsync<DataCreateResponse>("api/data", request, cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> PutDataAsync(long id, DataUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PutAsync($"api/data/{id}", request, cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> DeleteDataAsync(long id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.SendAsync(HttpMethod.Delete, $"api/data/{id}", cancel: cancellationToken);
    }

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<SecretMessageResponse>> GetSecretMessageAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<SecretMessageResponse>("api/secret/message", cancel: cancellationToken);
    }

    //--------------------------------------------------------------------------------
    // Storage
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<StorageListResponse>> GetStorageListAsync(string path, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<StorageListResponse>($"api/storage/{path}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> DeleteStorageAsync(string path, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.SendAsync(HttpMethod.Delete, $"api/storage/{path}", cancel: cancellationToken);
    }

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

    public ValueTask<IRestResponse> UploadAsync(string path, Stream stream, Action<double> action, bool compress, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Transfer);
        return client.UploadAsync(
            $"api/storage/{path}",
            stream,
            compress: compress ? CompressOption.Gzip : CompressOption.None,
            progress: CreateProgressCallback(action),
            cancel: cancellationToken);
    }

    private static Action<long, long> CreateProgressCallback(Action<double> action)
    {
        var progress = -1d;
        return (processed, total) =>
        {
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

    public ValueTask<IRestResponse> GetTestErrorAsync(int code, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.SendAsync(HttpMethod.Get, $"api/test/error/{code}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> GetTestDelayAsync(int timeout, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.SendAsync(HttpMethod.Get, $"api/test/delay/{timeout}", cancel: cancellationToken);
    }
}
