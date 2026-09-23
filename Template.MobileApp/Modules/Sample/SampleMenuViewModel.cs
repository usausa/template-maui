namespace Template.MobileApp.Modules.Sample;

public sealed class SampleMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public IObserveCommand CvNetCommand { get; }

    public IObserveCommand ChatCommand { get; }

    public SampleMenuViewModel(
        IDialog dialog,
        Settings settings)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
        CvNetCommand = MakeAsyncCommand(async () =>
        {
            if (!await settings.IsAIServiceConfiguredAsync())
            {
                await dialog.InformationAsync("AI end point is not configured.");
                return;
            }

            await Navigator.ForwardAsync(ViewId.SampleCvNet);
        });
        ChatCommand = MakeAsyncCommand(async () =>
        {
            if (!settings.IsOllamaConfigured())
            {
                await dialog.InformationAsync("Ollama end point is not configured.");
                return;
            }

            await Navigator.ForwardAsync(ViewId.SampleChat);
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.Menu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
