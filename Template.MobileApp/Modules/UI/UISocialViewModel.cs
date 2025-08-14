namespace Template.MobileApp.Modules.UI;

public sealed class UISocialViewModel : AppViewModelBase
{
    public IObserveCommand BackCommand { get; }

    public UISocialViewModel()
    {
        BackCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIMenu));
    }
}
