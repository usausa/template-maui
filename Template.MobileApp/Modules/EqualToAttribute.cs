namespace Template.MobileApp.Modules;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EqualToAttribute : ValidationAttribute
{
    public string OtherProperty { get; }

    public EqualToAttribute(string otherProperty)
    {
        OtherProperty = otherProperty;
    }

    public override string FormatErrorMessage(string name) =>
        String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, OtherProperty);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var accessor = AccessorProvider.FindAccessor(validationContext.ObjectType);
        if (accessor is null)
        {
            throw new InvalidOperationException($"Accessor is not supported. type=[{validationContext.ObjectType}]");
        }

        var other = accessor.GetValue(validationContext.ObjectInstance, OtherProperty);
        if (other is EntryController input)
        {
            other = input.Text ?? string.Empty;
        }

        return Equals(value, other)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.MemberName is { } memberName ? [memberName] : null);
    }
}
