namespace Template.MobileApp.Modules.UI;

public enum MailPage
{
    Mail,
    Schedule,
    Application
}

public sealed partial class UIMailViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial UIMailContext Context { get; set; } = default!;

    [ObservableProperty]
    public partial MailPage Selected { get; set; } = MailPage.Mail;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; set; }

    public string OwnerName { get; } = "うさうささん";

    public ICommand ArchiveCommand { get; }

    public ICommand DeleteCommand { get; }

    public IObserveCommand SelectCommand { get; }

    public IObserveCommand OpenCommand { get; }

    public IObserveCommand StarCommand { get; }

    public IObserveCommand DrawerCommand { get; }

    public IObserveCommand FolderCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIMailViewModel()
    {
        SelectCommand = MakeDelegateCommand<MailPage>(x => Selected = x);
        ArchiveCommand = MakeDelegateCommand<MailMessage>(Archive);
        DeleteCommand = MakeDelegateCommand<MailMessage>(Delete);
        OpenCommand = MakeAsyncCommand<MailMessage>(OpenAsync);
        StarCommand = MakeDelegateCommand<MailMessage>(ToggleStar);
        DrawerCommand = MakeDelegateCommand(() => IsDrawerOpen = true);
        FolderCommand = MakeDelegateCommand<Selectable<MailViewCount>>(ShowFolder);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // 本文の画面から戻ったときは読み込まない
    // ReSharper disable once ArrangeModifiersOrder
    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!Context.IsLoaded)
        {
            await Navigator.PostActionAsync(LoadMessagesAsync);
        }
    }

    protected override Task OnNotifyBackAsync()
    {
        if (IsDrawerOpen)
        {
            IsDrawerOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task OpenAsync(MailMessage message)
    {
        Context.Open(message);
        return Navigator.PushAsync(ViewId.UIMailDetail);
    }

    private void Archive(MailMessage message) => Context.Archive(message);

    private void Delete(MailMessage message) => Context.Delete(message);

    private void ToggleStar(MailMessage message) => Context.ToggleStar(message);

    private void ShowFolder(Selectable<MailViewCount> folder)
    {
        IsDrawerOpen = false;
        Context.Show(folder.Item.View);
    }

    private async Task LoadMessagesAsync()
    {
        IsLoading = true;

        // Simulate delay
        await Task.Delay(500);

        Context.Load();

        IsLoading = false;
    }
}
