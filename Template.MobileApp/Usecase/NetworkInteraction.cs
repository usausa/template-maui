namespace Template.MobileApp.Usecase;

// 通信実行時のユーザー対話の抽象。NetworkOperatorをUI(IDialog)から切り離し、UIなしテスト・再利用を可能にする
public interface INetworkInteraction
{
    IDisposable Indicator();

    MauiComponents.IProgress Progress();

    ValueTask NotifyNetworkUnavailableAsync();

    ValueTask NotifyErrorAsync(NetworkErrorKind kind, HttpStatusCode statusCode);

    ValueTask<bool> ConfirmRetryAsync(NetworkErrorKind kind, HttpStatusCode statusCode);
}

public sealed class DialogNetworkInteraction : INetworkInteraction
{
    private readonly IDialog dialog;

    public DialogNetworkInteraction(IDialog dialog)
    {
        this.dialog = dialog;
    }

    public IDisposable Indicator() => dialog.Indicator();

    public MauiComponents.IProgress Progress() => dialog.Progress();

    public ValueTask NotifyNetworkUnavailableAsync() => dialog.InformationAsync("Network is not connected.");

    public ValueTask NotifyErrorAsync(NetworkErrorKind kind, HttpStatusCode statusCode) =>
        kind switch
        {
            NetworkErrorKind.Canceled => dialog.InformationAsync("Canceled."),
            NetworkErrorKind.NotFound or NetworkErrorKind.HttpError => dialog.InformationAsync(MakeErrorMessage(statusCode, withRetry: false)),
            _ => dialog.InformationAsync("Unknown error.")
        };

    public ValueTask<bool> ConfirmRetryAsync(NetworkErrorKind kind, HttpStatusCode statusCode) =>
        kind == NetworkErrorKind.Canceled
            ? dialog.ConfirmAsync("Canceled.\r\nRetry ?")
            : dialog.ConfirmAsync(MakeErrorMessage(statusCode, withRetry: true));

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
