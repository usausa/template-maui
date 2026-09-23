namespace Template.MobileApp.Messaging;

using System.Text.Json.Serialization.Metadata;

public interface IWebViewController
{
    void Attach(HybridWebView view);

    void Detach();
}

#pragma warning disable CA1033
public abstract class WebViewControllerBase : IWebViewController
{
    public event EventHandler<HybridWebViewRawMessageReceivedEventArgs>? RawMessageReceived;

    public event EventHandler<WebViewWebResourceRequestedEventArgs>? WebResourceRequested;

    public event EventHandler<WebViewInitializedEventArgs>? WebViewInitialized;

    private HybridWebView? webView;

    void IWebViewController.Attach(HybridWebView view)
    {
        webView = view;
        webView.RawMessageReceived += RaiseRawMessageReceived;
        webView.WebResourceRequested += RaiseWebResourceRequested;
        webView.WebViewInitialized += RaiseWebViewInitialized;
        Attached(view);
    }

    protected abstract void Attached(HybridWebView view);

    void IWebViewController.Detach()
    {
        if (webView is not null)
        {
            webView.RawMessageReceived -= RaiseRawMessageReceived;
            webView.WebResourceRequested -= RaiseWebResourceRequested;
            webView.WebViewInitialized -= RaiseWebViewInitialized;
        }
        webView = null;
    }

    private void RaiseRawMessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs e)
    {
        RawMessageReceived?.Invoke(sender, e);
    }

    private void RaiseWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
    {
        WebResourceRequested?.Invoke(sender, e);
    }

    private void RaiseWebViewInitialized(object? sender, WebViewInitializedEventArgs e)
    {
        WebViewInitialized?.Invoke(sender, e);
    }

    public void SendRawMessage(string rawMessage)
    {
        webView?.SendRawMessage(rawMessage);
    }

    public async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(
        string methodName,
        JsonTypeInfo<TReturnType> returnTypeJsonTypeInfo,
        object?[]? paramValues = null,
        JsonTypeInfo?[]? paramJsonTypeInfos = null)
    {
        if (webView is not null)
        {
            return await webView.InvokeJavaScriptAsync(methodName, returnTypeJsonTypeInfo, paramValues, paramJsonTypeInfos);
        }

        return default;
    }

    public Task InvokeJavaScriptAsync(
        string methodName,
        object?[]? paramValues = null,
        JsonTypeInfo?[]? paramJsonTypeInfos = null) =>
        webView?.InvokeJavaScriptAsync(methodName, paramValues, paramJsonTypeInfos) ?? Task.CompletedTask;

    public async Task<string?> EvaluateJavaScriptAsync(string script)
    {
        if (webView is not null)
        {
            return await webView.EvaluateJavaScriptAsync(script);
        }

        return null;
    }

    public void GoBack()
    {
#if ANDROID
        if (webView?.Handler?.PlatformView is Android.Webkit.WebView view)
        {
            if (view.CanGoBack())
            {
                view.GoBack();
            }
        }
#endif
    }
}
#pragma warning restore CA1033

public sealed class WebViewController : WebViewControllerBase
{
    protected override void Attached(HybridWebView view)
    {
    }
}

public sealed class WebViewController<T> : WebViewControllerBase
        where T : class
{
    private readonly T target;

    public WebViewController(T target)
    {
        this.target = target;
    }

    protected override void Attached(HybridWebView view)
    {
        view.SetInvokeJavaScriptTarget(target);
    }
}

public static class WebViewControllerExtensions
{
    public static IObservable<HybridWebViewRawMessageReceivedEventArgs> RawMessageReceivedAsObservable(this WebViewControllerBase controller) =>
        Observable.FromEvent<EventHandler<HybridWebViewRawMessageReceivedEventArgs>, HybridWebViewRawMessageReceivedEventArgs>(static h => (_, e) => h(e), h => controller.RawMessageReceived += h, h => controller.RawMessageReceived -= h);
}
