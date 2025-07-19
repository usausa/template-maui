namespace Template.MobileApp.Modules.Sample;

public sealed partial class SampleWebBasicViewModel : AppViewModelBase
{
    public WebViewController<SampleWebBasicViewModel> Controller { get; }

    [ObservableProperty]
    public partial string? Result { get; set; }

    public SampleWebBasicViewModel()
    {
        Controller = new WebViewController<SampleWebBasicViewModel>(this);
        Disposables.Add(Controller.RawMessageReceivedAsObservable().Subscribe(x => Result = x.Message));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Controller.SendRawMessage("Hello from C#!");
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction3()
    {
        var result = await Controller.InvokeJavaScriptAsync(
            "Add",
            SampleWebJsonContext.Default.Int32,
            [1, 2],
            [SampleWebJsonContext.Default.Int32, SampleWebJsonContext.Default.Int32]);
        Result = $"{result}";
    }

    protected override Task OnNotifyFunction4()
    {
        Controller.GoBack();
        return Task.CompletedTask;
    }

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
}
