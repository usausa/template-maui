namespace Template.MobileApp.Modules.Sample;

using System.Text;
using System.Text.Json;

public sealed partial class SampleWebBasicViewModel : AppViewModelBase
{
    private readonly IAppInfo appInfo;

    private readonly IDeviceInfo deviceInfo;

    public WebViewController<SampleWebBasicViewModel> Controller { get; }

    [ObservableProperty]
    public partial string? Result { get; set; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SampleWebBasicViewModel(
        IAppInfo appInfo,
        IDeviceInfo deviceInfo)
    {
        this.appInfo = appInfo;
        this.deviceInfo = deviceInfo;

        Controller = new WebViewController<SampleWebBasicViewModel>(this);
        Controller.WebResourceRequested += OnWebResourceRequested;
        Controller.WebViewInitialized += OnWebViewInitialized;
        Disposables.Add(Controller.RawMessageReceivedAsObservable().Subscribe(x => Result = x.Message));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.SendRawMessage("Hello from C#!");
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction3()
    {
        var sum = await Controller.InvokeJavaScriptAsync(
            "Add",
            SampleWebJsonContext.Default.Int32,
            [1, 2],
            [SampleWebJsonContext.Default.Int32, SampleWebJsonContext.Default.Int32]);
        await Controller.InvokeJavaScriptAsync(
            "UpdateStatus",
            [$"Add(1, 2) = {sum} (from C#)"],
            [SampleWebJsonContext.Default.String]);

        try
        {
            await Controller.InvokeJavaScriptAsync("ThrowError");
            Result = $"Add(1, 2) = {sum}";
        }
        catch (Exception ex) when (ex.GetType().Name == "HybridWebViewInvokeJavaScriptException")
        {
            Result = $"Add(1, 2) = {sum}, JS error: {ex.Message}";
        }
    }

    protected override Task OnNotifyFunction4()
    {
        Controller.GoBack();
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

#pragma warning disable CA1822
    public int Calc(int x, int y) => x + y;
#pragma warning restore CA1822

    public async Task<DataEntity> ExecuteAsync(int id, string name)
    {
        using (BusyState.Begin())
        {
            await Task.Delay(1000);
            return new DataEntity { Id = id, Name = name };
        }
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
    {
        if (!e.Uri.AbsolutePath.EndsWith("/local/info.json", StringComparison.Ordinal))
        {
            return;
        }

        e.Handled = true;
        var info = new WebLocalInfo(appInfo.Name, appInfo.VersionString, $"{deviceInfo.Manufacturer} {deviceInfo.Model}", DateTime.Now);
        var json = JsonSerializer.Serialize(info, SampleWebJsonContext.Default.WebLocalInfo);
        e.SetResponse(200, "OK", "application/json", new MemoryStream(Encoding.UTF8.GetBytes(json)));
    }

    private void OnWebViewInitialized(object? sender, WebViewInitializedEventArgs e)
    {
#if ANDROID
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Result = $"Initialized: {e.PlatformArgs?.Settings?.UserAgentString}";
#else
        Result = "Initialized";
#endif
    }
}
