namespace Template.MobileApp.Usecase;

using Rester;

using Smart.Mapper;

using Template.MobileApp.Services;

//--------------------------------------------------------------------------------
// Result
//--------------------------------------------------------------------------------

public enum NetworkResultType
{
    Success,
    Disconnected,   // Not connected
    Canceled,       // Canceled
    NotFound,       // 404
    HttpError,      // Other HTTP error
    Unknown         // Transport / deserialize failure
}

public class NetworkResult
{
    public NetworkResultType Type { get; }

    // 0 when no response was received
    public HttpStatusCode StatusCode { get; }

    public bool IsSuccess => Type == NetworkResultType.Success;

    public NetworkResult(NetworkResultType type, HttpStatusCode statusCode)
    {
        Type = type;
        StatusCode = statusCode;
    }
}

public sealed class NetworkResult<T> : NetworkResult
{
    public T Value { get; }

    public NetworkResult(NetworkResultType type, HttpStatusCode statusCode, T value)
        : base(type, statusCode)
    {
        Value = value;
    }
}

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class NetworkUsecaseMapper
{
    [Mapper]
    public static partial WorkEntity ToWorkEntity(this DataListEntry source);
}

//--------------------------------------------------------------------------------
// Usecase
//--------------------------------------------------------------------------------

public sealed class NetworkUsecase
{
    private const int MaxAttempts = 3;

    private readonly ILogger<NetworkUsecase> log;

    private readonly IDialog dialog;

    private readonly DeviceState deviceState;

    private readonly HttpService httpService;

    private readonly ApiContext apiContext;

    private readonly DataService dataService;

    public NetworkUsecase(
        ILogger<NetworkUsecase> log,
        IDialog dialog,
        DeviceState deviceState,
        HttpService httpService,
        ApiContext apiContext,
        DataService dataService)
    {
        this.log = log;
        this.dialog = dialog;
        this.deviceState = deviceState;
        this.httpService = httpService;
        this.apiContext = apiContext;
        this.dataService = dataService;
    }

    //--------------------------------------------------------------------------------
    // Simple
    //--------------------------------------------------------------------------------

    public async ValueTask GetServerTimeAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteVerboseAsync(static (h, t) => h.GetServerTimeAsync(t), cancellationToken: cancellationToken);
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\ntime=[{result.Value.DateTime.ToLocalTime():yyyy/MM/dd HH:mm:ss}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask GetDataListAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteVerboseAsync(static (h, t) => h.GetDataListAsync(t), cancellationToken: cancellationToken);
        if (result.IsSuccess)
        {
            await dataService.ReplaceWorkEnumerableAsync(result.Value.Entries.Select(static x => x.ToWorkEntity()));

            await dialog.InformationAsync($"Get success.\r\ncount=[{result.Value.Entries.Length}]\r\nSaved to Work table.");
        }
    }

    public ValueTask<NetworkResult<DataListResponse>> GetDataRangeAsync(int offset, int size, CancellationToken cancellationToken = default) =>
        ExecuteVerboseAsync((h, t) => h.GetDataListAsync(offset, size, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult<DataResponse>> GetDataAsync(long id, CancellationToken cancellationToken = default) =>
        ExecuteAsync((h, t) => h.GetDataAsync(id, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult<DataCreateResponse>> CreateDataAsync(string name, int value, CancellationToken cancellationToken = default)
    {
        var request = new DataCreateRequest { Name = name, Value = value };
        return ExecuteAsync((h, t) => h.PostDataAsync(request, t), cancellationToken: cancellationToken);
    }

    public ValueTask<NetworkResult> UpdateDataAsync(long id, string name, int value, CancellationToken cancellationToken = default)
    {
        var request = new DataUpdateRequest { Name = name, Value = value };
        return ExecuteAsync((h, t) => h.PutDataAsync(id, request, t), cancellationToken: cancellationToken);
    }

    public ValueTask<NetworkResult> DeleteDataAsync(long id, CancellationToken cancellationToken = default) =>
        ExecuteAsync((h, t) => h.DeleteDataAsync(id, t), cancellationToken: cancellationToken);

    //--------------------------------------------------------------------------------
    // Secret
    //--------------------------------------------------------------------------------

    public async ValueTask GetSecretMessageAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteVerboseAsync(static (h, t) => h.GetSecretMessageAsync(t), authenticated: true, cancellationToken: cancellationToken);
        if (result.IsSuccess)
        {
            await dialog.InformationAsync($"Get success.\r\nmessage=[{result.Value.Message}]");
        }
    }

    //--------------------------------------------------------------------------------
    // Login
    //--------------------------------------------------------------------------------

    public async ValueTask PostAccountLoginAsync(string id, CancellationToken cancellationToken = default)
    {
        var request = new AccountLoginRequest { Id = id };
        var result = await ExecuteVerboseAsync((h, t) => h.PostAccountLoginAsync(request, t), cancellationToken: cancellationToken);
        if (result.IsSuccess)
        {
            apiContext.LoginId = id;
            apiContext.SetToken(result.Value.Token);
            await dialog.InformationAsync($"Login success.\r\nexpires=[{apiContext.TokenExpires:yyyy/MM/dd HH:mm:ss}]");
        }
    }

    public void AccountLogout()
    {
        apiContext.ClearToken();
    }

    public void InvalidateToken()
    {
        apiContext.SetToken(string.Empty);
    }

    private async ValueTask<bool> TryReLoginAsync(CancellationToken cancellationToken)
    {
        var id = apiContext.LoginId;
        if (String.IsNullOrEmpty(id))
        {
            return false;
        }

        var response = await httpService.PostAccountLoginAsync(new AccountLoginRequest { Id = id }, cancellationToken);
        if (response.RestResult != RestResult.Success)
        {
            log.WarnLoginFailed(id, response.RestResult, (int)response.StatusCode);
            return false;
        }

        apiContext.SetToken(response.Content!.Token);
        log.InfoLogin(id, apiContext.TokenExpires);
        return true;
    }

    //--------------------------------------------------------------------------------
    // Storage (the screen shows the progress of the transfers)
    //--------------------------------------------------------------------------------

    public ValueTask<NetworkResult<StorageListResponse>> GetStorageListAsync(string path, CancellationToken cancellationToken = default) =>
        ExecuteVerboseAsync((h, t) => h.GetStorageListAsync(path, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult> DeleteStorageAsync(string path, CancellationToken cancellationToken = default) =>
        ExecuteAsync((h, t) => h.DeleteStorageAsync(path, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult> UploadStorageAsync(string path, Stream stream, Action<double> progress, CancellationToken cancellationToken = default) =>
        ExecuteTransferAsync((h, t) => h.UploadAsync(path, stream, progress, false, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult> DownloadStorageAsync(string path, string filename, Action<double> progress, CancellationToken cancellationToken = default) =>
        ExecuteTransferAsync((h, t) => h.DownloadAsync(path, filename, progress, t), cancellationToken: cancellationToken);

    //--------------------------------------------------------------------------------
    // Test
    //--------------------------------------------------------------------------------

    public ValueTask<NetworkResult> GetTestErrorAsync(int code, CancellationToken cancellationToken = default) =>
        ExecuteVerboseAsync((h, t) => h.GetTestErrorAsync(code, t), cancellationToken: cancellationToken);

    public ValueTask<NetworkResult> GetTestDelayAsync(int timeout, CancellationToken cancellationToken = default) =>
        ExecuteVerboseAsync((h, t) => h.GetTestDelayAsync(timeout, t), cancellationToken: cancellationToken);

    // Cancelable from the screen (no indicator)
    public ValueTask<NetworkResult> RunTestDelayAsync(int timeout, CancellationToken cancellationToken = default) =>
        ExecuteTransferAsync((h, t) => h.GetTestDelayAsync(timeout, t), cancellationToken: cancellationToken);

    //--------------------------------------------------------------------------------
    // Execute
    //--------------------------------------------------------------------------------

    private ValueTask<NetworkResult<T>> ExecuteVerboseAsync<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, bool authenticated = false, CancellationToken cancellationToken = default) =>
        ExecuteAsync(func, true, authenticated, cancellationToken);

    private ValueTask<NetworkResult> ExecuteVerboseAsync(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, bool authenticated = false, CancellationToken cancellationToken = default) =>
        ExecuteAsync(func, true, authenticated, cancellationToken);

    private ValueTask<NetworkResult<T>> ExecuteAsync<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, bool authenticated = false, CancellationToken cancellationToken = default) =>
        ExecuteAsync(func, false, authenticated, cancellationToken);

    private ValueTask<NetworkResult> ExecuteAsync(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, bool authenticated = false, CancellationToken cancellationToken = default) =>
        ExecuteAsync(func, false, authenticated, cancellationToken);

    private async ValueTask<NetworkResult<T>> ExecuteAsync<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, bool verbose, bool authenticated, CancellationToken cancellationToken)
    {
        var response = default(IRestResponse<T>);
        var result = await ExecuteCoreAsync(
            async t =>
            {
                using (dialog.Indicator())
                {
                    return response = await func(httpService, t);
                }
            },
            verbose,
            authenticated,
            cancellationToken);
        return new NetworkResult<T>(result.Type, result.StatusCode, result.IsSuccess ? response!.Content! : default!);
    }

    private ValueTask<NetworkResult> ExecuteAsync(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, bool verbose, bool authenticated, CancellationToken cancellationToken) =>
        ExecuteCoreAsync(
            async t =>
            {
                using (dialog.Indicator())
                {
                    return await func(httpService, t);
                }
            },
            verbose,
            authenticated,
            cancellationToken);

    private ValueTask<NetworkResult> ExecuteTransferAsync(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, bool authenticated = false, CancellationToken cancellationToken = default) =>
        ExecuteCoreAsync(t => func(httpService, t), false, authenticated, cancellationToken);

    private async ValueTask<NetworkResult> ExecuteCoreAsync(Func<CancellationToken, ValueTask<IRestResponse>> func, bool verbose, bool authenticated, CancellationToken cancellationToken)
    {
        var reLogged = false;
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            if (!deviceState.NetworkState.IsConnected())
            {
                if (verbose)
                {
                    await dialog.InformationAsync("Network is not connected.");
                }
                return new NetworkResult(NetworkResultType.Disconnected, 0);
            }

            var response = await func(cancellationToken);

            var type = ClassifyResponse(response);
            var result = new NetworkResult(type, response.StatusCode);
            if (type == NetworkResultType.Success)
            {
                return result;
            }

            // Cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                return new NetworkResult(NetworkResultType.Canceled, response.StatusCode);
            }

            log.WarnNetworkOperationFailed(response.RestResult, (int)response.StatusCode, response.InnerException);

            // Re-login once with the saved id on 401
            if (authenticated && !reLogged && (response.StatusCode == HttpStatusCode.Unauthorized) && await TryReLoginAsync(cancellationToken))
            {
                reLogged = true;
                attempt--;
                continue;
            }

            // 404
            if (type == NetworkResultType.NotFound)
            {
                return result;
            }

            if (!verbose)
            {
                return result;
            }

            // Unknown errors
            if ((type == NetworkResultType.Unknown) || (attempt == MaxAttempts))
            {
                await NotifyErrorAsync(type, response.StatusCode);
                return result;
            }

            if (!await ConfirmRetryAsync(type, response.StatusCode))
            {
                return result;
            }
        }

        return new NetworkResult(NetworkResultType.Unknown, 0);
    }

    private static NetworkResultType ClassifyResponse(IRestResponse response) =>
        response.RestResult switch
        {
            RestResult.Success => NetworkResultType.Success,
            RestResult.Cancel => NetworkResultType.Canceled,
            RestResult.RequestError or RestResult.HttpError => response.StatusCode == HttpStatusCode.NotFound ? NetworkResultType.NotFound : NetworkResultType.HttpError,
            _ => NetworkResultType.Unknown
        };

    //--------------------------------------------------------------------------------
    // Dialog
    //--------------------------------------------------------------------------------

    private ValueTask NotifyErrorAsync(NetworkResultType type, HttpStatusCode statusCode) =>
        dialog.InformationAsync(type switch
        {
            NetworkResultType.Canceled => "Canceled.",
            NetworkResultType.HttpError => MakeErrorMessage(statusCode, withRetry: false),
            _ => "Unknown error."
        });

    private ValueTask<bool> ConfirmRetryAsync(NetworkResultType type, HttpStatusCode statusCode) =>
        dialog.ConfirmAsync(type == NetworkResultType.Canceled ? "Canceled.\r\nRetry ?" : MakeErrorMessage(statusCode, withRetry: true));

    private static string MakeErrorMessage(HttpStatusCode statusCode, bool withRetry)
    {
        var message = new StringBuilder();
        message.AppendLine("Network error.");
        if (statusCode > 0)
        {
            message.AppendLine($"StatusCode={(int)statusCode}");
        }

        if (withRetry)
        {
            message.AppendLine("Retry ?");
        }

        return message.ToString();
    }
}
