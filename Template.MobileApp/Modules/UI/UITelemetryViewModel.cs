namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Scene;

public sealed class UITelemetryViewModel : AppViewModelBase
{
    public TelemetryScene Scene { get; } = new();

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITelemetryViewModel()
    {
        Disposables.Add(Scene);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        SceneObject.FrameStatsEnabled = true;
        Scene.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Scene.Stop();
        SceneObject.FrameStatsEnabled = false;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Scene.UseDoubleBuffer = !Scene.UseDoubleBuffer;
        return Task.CompletedTask;
    }
}
