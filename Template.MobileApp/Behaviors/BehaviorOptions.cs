namespace Template.MobileApp.Behaviors;

public sealed class BehaviorOptions
{
    // View

    public bool Border { get; set; } = true;

    public bool DisableOverScroll { get; set; } = true;

    // Label

    public bool AutoSize { get; set; } = true;

    // Button

    public bool RippleEffect { get; set; } = true;

    // Entry

    public bool HandleEnterKey { get; set; }

    public bool DisableShowSoftInputOnFocus { get; set; }

    public bool SelectAllOnFocus { get; set; } = true;

    public bool InputFilter { get; set; } = true;
}
