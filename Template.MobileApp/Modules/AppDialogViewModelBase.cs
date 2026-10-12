namespace Template.MobileApp.Modules;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppDialogViewModelBase :
    ExtendViewModelBase,
    IValidatable,
    IPopupNavigatorAware,
    IPopupClosing
{
    private IPopupNavigator popupNavigator = default!;

    protected AppDialogViewModelBase()
        : base(new ExtendViewModelOptions { BusyState = new BusyState() })
    {
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        System.Diagnostics.Debug.WriteLine($"{GetType()} is Disposed");
    }

    public void Validate(string name)
    {
        ValidationHelper.Validate(this, name, Errors);
    }

    public void SetPopupNavigator(IPopupNavigator navigator) => popupNavigator = navigator;

    public void OnClosing() => AcceptsCommand = false;

    protected ValueTask CloseAsync() => popupNavigator.CloseAsync();

    protected ValueTask CloseAsync<TResult>(TResult result) => popupNavigator.CloseAsync(result);
}
