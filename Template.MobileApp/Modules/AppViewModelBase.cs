namespace Template.MobileApp.Modules;

using System.ComponentModel.DataAnnotations;

using Smart.Mvvm.Resolver;

using Template.MobileApp.Shell;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppViewModelBase : ExtendViewModelBase, IValidatable, INavigatorAware, INavigationEventSupport, INotifySupportAsync<ShellEvent>
{
    private List<ValidationResult>? validationResults;

    public INavigator Navigator { get; set; } = default!;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        System.Diagnostics.Debug.WriteLine($"{GetType()} is Disposed");
    }

    // TODO BunnyTail version
    public void Validate(string name)
    {
        var pi = GetType().GetProperty(name);
        if (pi is null)
        {
            return;
        }

        validationResults ??= new List<ValidationResult>();

        var value = pi.GetValue(this, null);
        var context = new ValidationContext(this, DefaultResolveProvider.Default, null)
        {
            MemberName = name
        };
        if (!Validator.TryValidateProperty(value, context, validationResults))
        {
            Errors.AddError(name, validationResults[0].ErrorMessage!);
        }

        validationResults.Clear();
    }

    public virtual void OnNavigatingFrom(INavigationContext context)
    {
    }

    public virtual void OnNavigatingTo(INavigationContext context)
    {
    }

    public virtual void OnNavigatedTo(INavigationContext context)
    {
    }

    public Task NavigatorNotifyAsync(ShellEvent parameter)
    {
        return parameter switch
        {
            ShellEvent.Back => OnNotifyBackAsync(),
            ShellEvent.Function1 => OnNotifyFunction1(),
            ShellEvent.Function2 => OnNotifyFunction2(),
            ShellEvent.Function3 => OnNotifyFunction3(),
            ShellEvent.Function4 => OnNotifyFunction4(),
            _ => Task.CompletedTask
        };
    }

    protected virtual Task OnNotifyBackAsync() => Task.CompletedTask;

    protected virtual Task OnNotifyFunction1() => Task.CompletedTask;

    protected virtual Task OnNotifyFunction2() => Task.CompletedTask;

    protected virtual Task OnNotifyFunction3() => Task.CompletedTask;

    protected virtual Task OnNotifyFunction4() => Task.CompletedTask;
}
