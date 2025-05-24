namespace Template.MobileApp.Modules;

using System.ComponentModel.DataAnnotations;

using Smart.Mvvm.Resolver;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppDialogViewModelBase : ExtendViewModelBase, IValidatable
{
    private List<ValidationResult>? validationResults;

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
}
