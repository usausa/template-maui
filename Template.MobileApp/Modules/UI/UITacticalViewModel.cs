namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Scene;

public sealed class UITacticalViewModel : AppViewModelBase
{
    public MechHudScene Scene { get; } = new();

    public UITacticalViewModel()
    {
        Disposables.Add(Scene);
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Scene.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Scene.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
