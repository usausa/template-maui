namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Scene;

public sealed class UITelemetryViewModel : AppViewModelBase
{
    public TelemetryScene Scene { get; } = new();

    public UITelemetryViewModel()
    {
        Disposables.Add(Scene);
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        // ダブルバッファ試験 (D8) の計測。滞在中のみフレーム統計を logcat (mono-stdout) へ出す
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

    // ダブルバッファの ON/OFF を切り替える (画面左上に MODE 表示)
    protected override Task OnNotifyFunction2()
    {
        Scene.UseDoubleBuffer = !Scene.UseDoubleBuffer;
        return Task.CompletedTask;
    }
}
