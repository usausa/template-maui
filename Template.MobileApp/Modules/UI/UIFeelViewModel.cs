namespace Template.MobileApp.Modules.UI;

public sealed partial class UIFeelViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string SelectedMood { get; set; } = "Happy";

    public IObserveCommand SelectCommand { get; }

    public UIFeelViewModel()
    {
        SelectCommand = MakeDelegateCommand<string>(x => SelectedMood = x);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
