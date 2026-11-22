namespace Template.MobileApp.Modules;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Smart.Mvvm.Resolver;

internal static class ValidationHelper
{
    [ThreadStatic]
    private static List<ValidationResult>? validationResults;

    public static void Validate(object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string name, ErrorInfo errors)
    {
        validationResults ??= [];

        if (!TryValidate(target, type, name, validationResults))
        {
            errors.AddError(name, validationResults[0].ErrorMessage!);
        }

        validationResults.Clear();
    }

    public static bool IsValid(object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string name) =>
        TryValidate(target, type, name, null);

    public static async ValueTask<bool> ValidateInputsAsync(IValidatable target, ErrorInfo errors, IDialog dialog, string[] names)
    {
        foreach (var name in names)
        {
            errors.ClearErrors(name);
            target.Validate(name);
        }

        var error = Array.Find(names, errors.Contains);
        if (error is not null)
        {
            await dialog.InformationAsync(errors[error]!);
            (AccessorProvider.FindAccessor(target.GetType())?.GetValue(target, error) as EntryController)?.Focus();
        }

        return error is null;
    }

    private static bool TryValidate(object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string name, List<ValidationResult>? results)
    {
        var accessor = AccessorProvider.FindAccessor(type) ?? throw new InvalidOperationException($"Accessor is not supported. type=[{type}]");
        var value = accessor.GetValue(target, name);
        if (value is EntryController input)
        {
            value = input.Text ?? string.Empty;
        }

        var property = type.GetProperty(name);
        var context = new ValidationContext(target, property?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? name, ResolveProvider.Default, null)
        {
            MemberName = name
        };
        return Validator.TryValidateValue(value, context, results, property?.GetCustomAttributes<ValidationAttribute>() ?? []);
    }
}
