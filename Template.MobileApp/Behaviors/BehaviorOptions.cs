namespace Template.MobileApp.Behaviors;

public sealed class BehaviorOptions
{
    public bool Border { get; set; } = true;

    // Entry

    public bool HandleEnterKey { get; set; }

    public bool DisableShowSoftInputOnFocus { get; set; }

    public bool SelectAllOnFocus { get; set; } = true;

    public bool InputFilter { get; set; } = true;

    // ListView

    public bool DisableOverScroll { get; set; } = true;
}
