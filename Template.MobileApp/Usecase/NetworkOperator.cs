namespace Template.MobileApp.Usecase;

using Rester;

using Template.MobileApp.Services;

public enum NetworkOperationResult
{
    Success,
    Error,
    NotFound
}

public sealed class NetworkOperator
{
    private readonly IDialog dialog;

    private readonly DeviceState deviceState;

    private readonly HttpService httpService;

    public NetworkOperator(
        IDialog dialog,
        DeviceState deviceState,
        HttpService httpService)
    {
        this.dialog = dialog;
        this.deviceState = deviceState;
        this.httpService = httpService;
    }

    public ValueTask<IResult<T>> ExecuteVerbose<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func) => Execute(func, true);

    public ValueTask<IResult<T>> Execute<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func) => Execute(func, false);

    private async ValueTask<IResult<T>> Execute<T>(Func<HttpService, ValueTask<IRestResponse<T>>> func, bool verbose)
    {
        while (true)
        {
            if (!deviceState.NetworkState.IsConnected())
            {
                if (verbose)
                {
                    await dialog.InformationAsync("Network is not connected.");
                }
                return Result.Failed<T>();
            }

            IRestResponse<T> response;
            using (dialog.Indicator())
            {
                response = await func(httpService);
            }

            switch (response.RestResult)
            {
                case RestResult.Success:
                    return Result.Success(response.Content!);
                case RestResult.Cancel:
                    if (!verbose || !await dialog.ConfirmAsync("Canceled.\r\nRetry ?"))
                    {
                        return Result.Failed<T>();
                    }
                    break;
                case RestResult.RequestError:
                case RestResult.HttpError:
                    if (verbose)
                    {
                        var message = new StringBuilder();
                        message.AppendLine("Network error.");
                        if (response.StatusCode > 0)
                        {
                            message.AppendLine($"StatusCode={(int)response.StatusCode}");
                        }
                        message.AppendLine("Retry ?");
                        if (!await dialog.ConfirmAsync(message.ToString()))
                        {
                            return Result.Failed<T>();
                        }
                    }
                    else
                    {
                        return Result.Failed<T>();
                    }
                    break;
                default:
                    if (verbose)
                    {
                        await dialog.InformationAsync("Unknown error.");
                    }
                    return Result.Failed<T>();
            }
        }
    }

    public ValueTask<NetworkOperationResult> ExecuteVerbose(Func<HttpService, ValueTask<IRestResponse>> func) => Execute(func, true);

    public ValueTask<NetworkOperationResult> Execute(Func<HttpService, ValueTask<IRestResponse>> func) => Execute(func, false);

    private async ValueTask<NetworkOperationResult> Execute(Func<HttpService, ValueTask<IRestResponse>> func, bool verbose)
    {
        while (true)
        {
            if (!deviceState.NetworkState.IsConnected())
            {
                if (verbose)
                {
                    await dialog.InformationAsync("Network is not connected.");
                }
                return NetworkOperationResult.Error;
            }

            IRestResponse response;
            using (dialog.Indicator())
            {
                response = await func(httpService);
            }

            switch (response.RestResult)
            {
                case RestResult.Success:
                    return NetworkOperationResult.Success;
                case RestResult.Cancel:
                    if (!verbose || !await dialog.ConfirmAsync("Canceled.\r\nRetry ?"))
                    {
                        return NetworkOperationResult.Error;
                    }
                    break;
                case RestResult.RequestError:
                case RestResult.HttpError:
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return NetworkOperationResult.NotFound;
                    }

                    if (verbose)
                    {
                        var message = new StringBuilder();
                        message.AppendLine("Network error.");
                        if (response.StatusCode > 0)
                        {
                            message.AppendLine($"StatusCode={(int)response.StatusCode}");
                        }
                        message.AppendLine("Retry ?");
                        if (!await dialog.ConfirmAsync(message.ToString()))
                        {
                            return NetworkOperationResult.Error;
                        }
                    }
                    else
                    {
                        return NetworkOperationResult.Error;
                    }
                    break;
                default:
                    if (verbose)
                    {
                        await dialog.InformationAsync("Unknown error.");
                    }
                    return NetworkOperationResult.Error;
            }
        }
    }

    public ValueTask<NetworkOperationResult> ExecuteProgressVerbose(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func) => ExecuteProgress(func, true);

    public ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func) => ExecuteProgress(func, false);

    private async ValueTask<NetworkOperationResult> ExecuteProgress(Func<HttpService, MauiComponents.IProgress, ValueTask<IRestResponse>> func, bool verbose)
    {
        using var progress = dialog.Progress();

        var response = await func(httpService, progress);

        switch (response.RestResult)
        {
            case RestResult.Success:
                return NetworkOperationResult.Success;
            case RestResult.Cancel:
                if (verbose)
                {
                    await dialog.InformationAsync("Canceled.");
                }
                return NetworkOperationResult.Error;
            case RestResult.RequestError:
            case RestResult.HttpError:
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NetworkOperationResult.NotFound;
                }

                if (verbose)
                {
                    var message = new StringBuilder();
                    message.AppendLine("Network error.");
                    if (response.StatusCode > 0)
                    {
                        message.AppendLine($"statusCode={(int)response.StatusCode}");
                    }
                    await dialog.InformationAsync(message.ToString());
                }
                return NetworkOperationResult.Error;
            default:
                if (verbose)
                {
                    await dialog.InformationAsync("Unknown error.");
                }
                return NetworkOperationResult.Error;
        }
    }
}
