namespace Template.MobileApp.Modules;

using System.ComponentModel.DataAnnotations;

using Smart.Mvvm.Resolver;

internal static class ValidationHelper
{
    [ThreadStatic]
    private static List<ValidationResult>? validationResults;

    public static void Validate(object target, string name, ErrorInfo errors)
    {
        var accessor = AccessorProvider.FindAccessor(target.GetType());
        if (accessor is null)
        {
            throw new InvalidOperationException($"Accessor is not supported. type=[{target.GetType()}]");
        }

        var value = accessor.GetValue(target, name);
        var context = new ValidationContext(target, ResolveProvider.Default, null)
        {
            MemberName = name
        };
        validationResults ??= [];

        if (!Validator.TryValidateProperty(value, context, validationResults))
        {
            errors.AddError(name, validationResults[0].ErrorMessage!);
        }

        validationResults.Clear();
    }
}
