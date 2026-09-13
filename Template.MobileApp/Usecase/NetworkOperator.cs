namespace Template.MobileApp.Usecase;

using Rester;

using Template.MobileApp.Services;

public enum NetworkOperationResult
{
    Success,
    Error,
    NotFound,
    Canceled
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

// 通信失敗の理由 (Smart.Results の Error 派生。呼び出し側が Kind / Status で分岐できる)
public sealed record NetworkError(NetworkErrorKind Kind, HttpStatusCode Status)
    : Error($"Network error. kind=[{Kind}] status=[{(int)Status}]");

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

    public ValueTask<Result<T>> ExecuteVerbose<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, CancellationToken cancellationToken = default) => Execute(func, true, cancellationToken);

    public ValueTask<Result<T>> Execute<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, CancellationToken cancellationToken = default) => Execute(func, false, cancellationToken);

    private async ValueTask<Result<T>> Execute<T>(Func<HttpService, CancellationToken, ValueTask<IRestResponse<T>>> func, bool verbose, CancellationToken cancellationToken)
    {
        var response = default(IRestResponse<T>);

        // 型付き版では404も通常のHTTPエラーとして扱う
        var result = await ExecuteCore(async (h, t) => response = await func(h, t), verbose, notFoundAsResult: false, cancellationToken);
        if (result == NetworkOperationResult.Success)
        {
            return Result.Success(response!.Content!);
        }

        // 失敗理由を Error として返す (応答自体が無い場合はネットワーク未接続)
        return response is null
            ? Result.Failure<T>("Network is unavailable.")
            : Result.Failure<T>(new NetworkError(ClassifyError(response), response.StatusCode));
    }

    //--------------------------------------------------------------------------------
    // Plain
    //--------------------------------------------------------------------------------

    public ValueTask<NetworkOperationResult> ExecuteVerbose(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, CancellationToken cancellationToken = default) => ExecuteCore(func, true, notFoundAsResult: true, cancellationToken);

    public ValueTask<NetworkOperationResult> Execute(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, CancellationToken cancellationToken = default) => ExecuteCore(func, false, notFoundAsResult: true, cancellationToken);

    private async ValueTask<NetworkOperationResult> ExecuteCore(Func<HttpService, CancellationToken, ValueTask<IRestResponse>> func, bool verbose, bool notFoundAsResult, CancellationToken cancellationToken)
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
                response = await func(httpService, cancellationToken);
            }

            var kind = ClassifyError(response);
            if (kind == NetworkErrorKind.None)
            {
                return NetworkOperationResult.Success;
            }

            // 呼び出し側の中断は失敗として扱わない (通知・再試行確認なし)
            if (cancellationToken.IsCancellationRequested)
            {
                return NetworkOperationResult.Canceled;
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

    public ValueTask<NetworkOperationResult> ExecuteProgressVerbose(Func<HttpService, MauiComponents.IProgress, CancellationToken, ValueTask<IRestResponse>> func, CancellationToken cancellationToken = default) => ExecuteProgress(func, true, cancellationToken);

    public ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, CancellationToken, ValueTask<IRestResponse>> func, CancellationToken cancellationToken = default) => ExecuteProgress(func, false, cancellationToken);

    // 進捗付き転送は途中失敗時の再実行コストが大きいため自動・人力ともリトライしない仕様
    private async ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, CancellationToken, ValueTask<IRestResponse>> func, bool verbose, CancellationToken cancellationToken)
    {
        using var progress = interaction.Progress();

        var response = await func(httpService, progress, cancellationToken);

        var kind = ClassifyError(response);
        if (kind == NetworkErrorKind.None)
        {
            return NetworkOperationResult.Success;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return NetworkOperationResult.Canceled;
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
