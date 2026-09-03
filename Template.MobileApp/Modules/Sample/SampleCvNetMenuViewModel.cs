namespace Template.MobileApp.Modules.Sample;

public sealed class SampleCvNetMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public SampleCvNetMenuViewModel(
        IDialog dialog,
        Settings settings)
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(async x =>
        {
            var configured = !String.IsNullOrEmpty(settings.AIServiceEndPoint) &&
                             !String.IsNullOrEmpty(await settings.GetAIServiceKeyAsync());
            if (!configured)
            {
                await dialog.InformationAsync("AI end point is not configured.");
                return;
            }

            await Navigator.ForwardAsync(x);
        });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
