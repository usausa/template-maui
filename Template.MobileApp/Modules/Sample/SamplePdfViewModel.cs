namespace Template.MobileApp.Modules.Sample;

using Maui.PDFView.Events;

public sealed partial class SamplePdfViewModel : AppViewModelBase
{
    private readonly IFileSystem fileSystem;

    [ObservableProperty]
    public partial string PdfSource { get; set; } = default!;

    [ObservableProperty]
    public partial int PageIndex { get; set; }

    [ObservableProperty]
    public partial int TotalPages { get; set; }

    [ObservableProperty]
    public partial string PageInformation { get; set; } = default!;

    [ObservableProperty]
    public partial bool CanMovePrev { get; set; }

    [ObservableProperty]
    public partial bool CanMoveNext { get; set; }

    public ICommand PageChangedCommand { get; }

    public SamplePdfViewModel(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;

        PageChangedCommand = MakeDelegateCommand<PageChangedEventArgs>(x =>
        {
            TotalPages = x.TotalPages;
            PageInformation = $"{x.CurrentPage} / {x.TotalPages}";
            CanMovePrev = x.CurrentPage > 1;
            CanMoveNext = x.CurrentPage < x.TotalPages;
        });
    }

    public override async Task OnNavigatingToAsync(INavigationContext context)
    {
        // Extract pdf
        var filename = Path.Combine(Path.GetTempPath(), "sample.pdf");
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        await using var output = File.OpenWrite(filename);
        await using var input = await fileSystem.OpenAppPackageFileAsync("sample.pdf");
        await input.CopyToAsync(output);

        PdfSource = filename;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        PageIndex--;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        PageIndex++;
        return Task.CompletedTask;
    }
}
