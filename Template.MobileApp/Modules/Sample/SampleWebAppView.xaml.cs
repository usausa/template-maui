namespace Template.MobileApp.Modules.Sample;

[View(ViewId.SampleWebApp)]
public sealed partial class SampleWebAppView
{
    public SampleWebAppView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        // HybridWebView に確実な読み込み完了イベントがないため固定時間でフェードアウトする
        await Task.Delay(800);
        await LoadingOverlay.FadeToAsync(0, 400, Easing.CubicOut);
        LoadingOverlay.IsVisible = false;
    }
}
