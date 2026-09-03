namespace Template.MobileApp.Modules;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppDialogViewModelBase : ExtendViewModelBase, IValidatable
{
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
}
