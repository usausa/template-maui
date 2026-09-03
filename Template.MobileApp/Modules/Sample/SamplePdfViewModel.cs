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
    public partial int MaxPageIndex { get; set; }

    [ObservableProperty]
    public partial string PageInformation { get; set; } = "1 / -";

    [ObservableProperty]
    public partial bool CanMovePrev { get; set; }

    [ObservableProperty]
    public partial bool CanMoveNext { get; set; }

    public ICommand PageChangedCommand { get; }

    public ICommand PrevPageCommand { get; }

    public ICommand NextPageCommand { get; }

    public SamplePdfViewModel(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;

        PageChangedCommand = MakeDelegateCommand<PageChangedEventArgs>(x =>
        {
            TotalPages = x.TotalPages;
            MaxPageIndex = Math.Max(0, x.TotalPages - 1);
            PageInformation = $"{x.CurrentPage} / {x.TotalPages}";
            CanMovePrev = x.CurrentPage > 1;
            CanMoveNext = x.CurrentPage < x.TotalPages;
        });
        PrevPageCommand = MakeDelegateCommand(() => PageIndex--);
        NextPageCommand = MakeDelegateCommand(() => PageIndex++);
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
        await using var input = await fileSystem.OpenAppPackageFileAsync(Path.Combine("Documents", "sample.pdf"));
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
