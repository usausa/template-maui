namespace Template.MobileApp.Modules.UI;

public sealed class UITimelineViewModel : AppViewModelBase
{
    public ObservableCollection<TimelineRow> Rows { get; } = new();

    public UITimelineViewModel()
    {
        Rows.Add(new TimelineRow { No = 0, LineNos = [0], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 0, LineNos = [0], In = 1, Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 0, LineNos = [0, 1], In = 1, Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1], In = 2, Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1, 2], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 2, LineNos = [0, 1, 2], Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 1, LineNos = [0, 1], Out = 2, Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 0, LineNos = [0], Out = 1, Id = "0000000000000000", Text = "🐛Message" });
        Rows.Add(new TimelineRow { No = 0, LineNos = [0], Id = "0000000000000000", Text = "🐛Message" });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
