namespace Template.MobileApp.Usecase;

using Rester;

using Template.MobileApp.Services;

public class NetworkOperator
{
    private readonly IDialog dialog;

    private readonly ApplicationState applicationState;

    private readonly NetworkService networkService;

    public NetworkOperator(
        IDialog dialog,
        ApplicationState applicationState,
        NetworkService networkService)
    {
        this.dialog = dialog;
        this.applicationState = applicationState;
        this.networkService = networkService;
    }

    public ValueTask<IResult<T>> ExecuteVerbose<T>(Func<NetworkService, ValueTask<IRestResponse<T>>> func) => Execute(func, true);

    public ValueTask<IResult<T>> Execute<T>(Func<NetworkService, ValueTask<IRestResponse<T>>> func) => Execute(func, false);

    public ValueTask<bool> ExecuteVerbose(Func<NetworkService, ValueTask<IRestResponse>> func) => Execute(func, true);

    public ValueTask<bool> Execute(Func<NetworkService, ValueTask<IRestResponse>> func) => Execute(func, false);

    private async ValueTask<IResult<T>> Execute<T>(Func<NetworkService, ValueTask<IRestResponse<T>>> func, bool verbose)
    {
        while (true)
        {
            if (!applicationState.NetworkState.IsConnected())
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
                response = await func(networkService);
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

    private async ValueTask<bool> Execute(Func<NetworkService, ValueTask<IRestResponse>> func, bool verbose)
    {
        while (true)
        {
            if (!applicationState.NetworkState.IsConnected())
            {
                if (verbose)
                {
                    await dialog.InformationAsync("Network is not connected.");
                }
                return false;
            }

            IRestResponse response;
            using (dialog.Indicator())
            {
                response = await func(networkService);
            }

            switch (response.RestResult)
            {
                case RestResult.Success:
                    return true;
                case RestResult.Cancel:
                    if (!verbose || !await dialog.ConfirmAsync("Canceled.\r\nRetry ?"))
                    {
                        return false;
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
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    if (verbose)
                    {
                        await dialog.InformationAsync("Unknown error.");
                    }
                    return false;
            }
        }
    }
}
