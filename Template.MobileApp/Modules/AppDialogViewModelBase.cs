namespace Template.MobileApp.Modules;

public abstract class AppDialogViewModelBase : ViewModelBase
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        System.Diagnostics.Debug.WriteLine($"{GetType()} is Disposed");
    }
}
