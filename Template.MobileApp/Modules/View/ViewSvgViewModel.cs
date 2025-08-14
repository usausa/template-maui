namespace Template.MobileApp.Modules.View;

using Svg.Skia;

public sealed partial class ViewSvgViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    [ObservableProperty]
    public partial SKSvg? Svg { get; set; }

    public ViewSvgViewModel(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        var svg = new SKSvg();
        await using var stream = await fileSystem.OpenAppPackageFileAsync(Path.Combine("Svg", "dotnet_bot.svg"));
        svg.Load(stream);
        Svg = svg;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
