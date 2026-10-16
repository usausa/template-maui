namespace Template.MobileApp.Modules.UI;

public sealed partial class UIMailContext : ObservableObject
{
    private readonly MailBox box = new(MailSample.LoadMails(DateTime.Now));

    public IReadOnlyList<Selectable<MailViewCount>> Folders { get; }

    [ObservableProperty]
    public partial MailView View { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<MailGroup> Groups { get; set; } = [];

    [ObservableProperty]
    public partial MailMessage Selected { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    public UIMailContext()
    {
        Folders = box.Counts.Select(static x => new Selectable<MailViewCount>(x)).ToArray();
    }

    public void Load()
    {
        IsLoaded = true;
        Refresh();
    }

    public void Show(MailView view)
    {
        View = view;
        Refresh();
    }

    public void Open(MailMessage message)
    {
        Selected = message;
        box.Read(message);
    }

    public void ToggleStar(MailMessage message)
    {
        box.ToggleStar(message);
        if (View == MailView.Starred)
        {
            Refresh();
        }
    }

    public void Delete(MailMessage message)
    {
        box.Delete(message);
        Refresh();
    }

    public void Archive(MailMessage message)
    {
        box.Archive(message);
        Refresh();
    }

    private void Refresh()
    {
        Groups = box.Find(View, DateTime.Now);
        foreach (var folder in Folders)
        {
            folder.IsSelected = folder.Item.View == View;
        }
    }
}
