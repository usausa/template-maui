namespace Template.MobileApp.Modules.Sample;

public sealed partial class SampleMarkdownViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Text { get; set; }

    public IObserveCommand LinkCommand { get; }

    public IObserveCommand EmailCommand { get; }

    public SampleMarkdownViewModel(IDialog dialog)
    {
        LinkCommand = MakeAsyncCommand<string>(async url =>
        {
            await dialog.InformationAsync($"Link: {url}");
        });
        EmailCommand = MakeAsyncCommand<string>(async email =>
        {
            await dialog.InformationAsync($"Email: {email}");
        });

        Text = """
               # Title1

               ## Title2

               ### Title3

               ### Style

               - Default
               - **Bold**
               - *Italic*
               - ~~strike~~

               ### List

               - Item 1
                 - Subitem 1.1
                   - Sub-subitem 1.1.1
                 - Subitem 1.2
               - Item 2

               1. First
                  1.1. Sub-first
                      1.1.1. Sub-sub-first
               2. Second

               ### Block quote

               > 111
               > 222
               > 333

               ### Code block

               ```csharp
               for(int n= 0; n<10; n++)
               {
                   Console.WriteLine(n);
               }
               ```

               ### Table

               Left  |  Center |  Right
               :---------|:--------:|---------:
               A1 | B1 | C1
               A2 | B2 | C2
               A3 | B3 | C3

               ### Horizontal rules

               ---

               ### Link

               [Maui](https://learn.microsoft.com/en-us/dotnet/maui/)

               """;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.SampleMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
