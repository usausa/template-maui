namespace Template.MobileApp.Usecase;

using Rester;

using Template.MobileApp.Services;

public enum NetworkOperationResult
{
    Success,
    Error,
    NotFound
}

// 通信結果の分類 (UI非依存の純粋ロジック)
public enum NetworkErrorKind
{
    None,
    Canceled,
    NotFound,
    HttpError,
    Unknown
}

public sealed class NetworkOperator
{
    // 接続実行の最大試行回数 (初回 + 人力リトライ)
    private const int MaxAttempts = 3;

    private readonly ILogger<NetworkOperator> log;

    private readonly INetworkInteraction interaction;

    private readonly DeviceState deviceState;

    private readonly HttpService httpService;

    public NetworkOperator(
        ILogger<NetworkOperator> log,
        INetworkInteraction interaction,
        DeviceState deviceState,
        HttpService httpService)
    {
        this.log = log;
        this.interaction = interaction;
        this.deviceState = deviceState;
        this.httpService = httpService;
    }

    public static NetworkErrorKind ClassifyError(IRestResponse response) =>
        response.RestResult switch
        {
            RestResult.Success => NetworkErrorKind.None,
            RestResult.Cancel => NetworkErrorKind.Canceled,
            RestResult.RequestError or RestResult.HttpError =>
                response.StatusCode == HttpStatusCode.NotFound ? NetworkErrorKind.NotFound : NetworkErrorKind.HttpError,
            _ => NetworkErrorKind.Unknown
        };

    //--------------------------------------------------------------------------------
    // Typed
    //--------------------------------------------------------------------------------

    public ValueTask<IResult<T>> ExecuteVerbose<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func) => Execute(func, true);

    public ValueTask<IResult<T>> Execute<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func) => Execute(func, false);

    private async ValueTask<IResult<T>> Execute<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func, bool verbose)
    {
        var response = default(IRestResponse<T>);

        // 型付き版では404も通常のHTTPエラーとして扱う
        var result = await ExecuteCore(async h => response = await func(h), verbose, notFoundAsResult: false);
        return result == NetworkOperationResult.Success ? Result.Success(response!.Content!) : Result.Failed<T>();
    }

    //--------------------------------------------------------------------------------
    // Plain
    //--------------------------------------------------------------------------------

    public ValueTask<NetworkOperationResult> ExecuteVerbose(Func<HttpService, ValueTask<IRestResponse>> func) => ExecuteCore(func, true, notFoundAsResult: true);

    public ValueTask<NetworkOperationResult> Execute(Func<HttpService, ValueTask<IRestResponse>> func) => ExecuteCore(func, false, notFoundAsResult: true);

    private async ValueTask<NetworkOperationResult> ExecuteCore(Func<HttpService, ValueTask<IRestResponse>> func, bool verbose, bool notFoundAsResult)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            if (!deviceState.NetworkState.IsConnected())
            {
                if (verbose)
                {
                    await interaction.NotifyNetworkUnavailableAsync();
                }
                return NetworkOperationResult.Error;
            }

            IRestResponse response;
            using (interaction.Indicator())
            {
                response = await func(httpService);
            }

            var kind = ClassifyError(response);
            if (kind == NetworkErrorKind.None)
            {
                return NetworkOperationResult.Success;
            }

            log.WarnNetworkOperationFailed(response.RestResult, (int)response.StatusCode, response.InnerException);

            if (notFoundAsResult && (kind == NetworkErrorKind.NotFound))
            {
                return NetworkOperationResult.NotFound;
            }

            switch (kind)
            {
                case NetworkErrorKind.Canceled:
                case NetworkErrorKind.NotFound:
                case NetworkErrorKind.HttpError:
                    if (!verbose)
                    {
                        return NetworkOperationResult.Error;
                    }

                    // 試行上限に達した場合はリトライ確認せずに打ち切る
                    if (attempt == MaxAttempts)
                    {
                        if (kind != NetworkErrorKind.Canceled)
                        {
                            await interaction.NotifyErrorAsync(kind, response.StatusCode);
                        }
                        return NetworkOperationResult.Error;
                    }

                    if (!await interaction.ConfirmRetryAsync(kind, response.StatusCode))
                    {
                        return NetworkOperationResult.Error;
                    }
                    break;
                default:
                    if (verbose)
                    {
                        await interaction.NotifyErrorAsync(kind, response.StatusCode);
                    }
                    return NetworkOperationResult.Error;
            }
        }

        return NetworkOperationResult.Error;
    }

    //--------------------------------------------------------------------------------
    // Progress
    //--------------------------------------------------------------------------------

    public ValueTask<NetworkOperationResult> ExecuteProgressVerbose(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func) => ExecuteProgress(func, true);

    public ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func) => ExecuteProgress(func, false);

    // 進捗付き転送は途中失敗時の再実行コストが大きいため自動・人力ともリトライしない仕様
    private async ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func, bool verbose)
    {
        using var progress = interaction.Progress();

        var response = await func(httpService, progress);

        var kind = ClassifyError(response);
        if (kind == NetworkErrorKind.None)
        {
            return NetworkOperationResult.Success;
        }

        log.WarnNetworkOperationFailed(response.RestResult, (int)response.StatusCode, response.InnerException);

        if (kind == NetworkErrorKind.NotFound)
        {
            return NetworkOperationResult.NotFound;
        }

        if (verbose)
        {
            await interaction.NotifyErrorAsync(kind, response.StatusCode);
        }

        return NetworkOperationResult.Error;
    }
}
