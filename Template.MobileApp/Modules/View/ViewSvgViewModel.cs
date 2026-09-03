namespace Template.MobileApp.Modules.View;

public sealed partial class ViewSvgViewModel : AppViewModelBase
{
    // SvgView.Source にパスを渡すだけでロード/キャッシュはコントロール側が行う
    [ObservableProperty]
    public partial string SvgSource { get; set; } = Path.Combine("Svg", "dotnet_bot.svg");

    [ObservableProperty]
    public partial string Selected { get; set; } = "dotnet_bot";

    public IObserveCommand SelectCommand { get; }

    public ViewSvgViewModel()
    {
        SelectCommand = MakeDelegateCommand<string>(Select);
    }

    private void Select(string name)
    {
        SvgSource = name switch
        {
            "vite" => Path.Combine("web-app", "vite.svg"),
            "react" => Path.Combine("web-app", "assets", "react-CHdo91hT.svg"),
            _ => Path.Combine("Svg", "dotnet_bot.svg")
        };
        Selected = name;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
