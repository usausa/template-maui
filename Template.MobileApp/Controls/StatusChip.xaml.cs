namespace Template.MobileApp.Controls;

public partial class StatusChip
{
    private static readonly Color NeutralChipColor = Color.FromArgb("#F5F5F5");

    private static readonly Color NeutralIconColor = Color.FromArgb("#546E7A");

    private static readonly Color NeutralTextColor = Color.FromArgb("#455A64");

    private static readonly Color SuccessChipColor = Color.FromArgb("#C8E6C9");

    private static readonly Color SuccessColor = Color.FromArgb("#388E3C");

    private static readonly Color WarningChipColor = Color.FromArgb("#FFECB3");

    private static readonly Color WarningColor = Color.FromArgb("#FF8F00");

    private static readonly Color ErrorChipColor = Color.FromArgb("#FFCDD2");

    private static readonly Color ErrorColor = Color.FromArgb("#D32F2F");

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StatusChip),
        string.Empty);

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(StatusChip),
        string.Empty);

    public static readonly BindableProperty ChipColorProperty = BindableProperty.Create(
        nameof(ChipColor),
        typeof(Color),
        typeof(StatusChip),
        NeutralChipColor);

    public static readonly BindableProperty IconSpinProperty = BindableProperty.Create(
        nameof(IconSpin),
        typeof(bool),
        typeof(StatusChip),
        false);

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(StatusChip),
        NeutralIconColor);

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(StatusChip),
        NeutralTextColor);

    public static readonly BindableProperty ToneProperty = BindableProperty.Create(
        nameof(Tone),
        typeof(StatusTone),
        typeof(StatusChip),
        StatusTone.Neutral,
        propertyChanged: OnToneChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color ChipColor
    {
        get => (Color)GetValue(ChipColorProperty);
        set => SetValue(ChipColorProperty, value);
    }

    public bool IconSpin
    {
        get => (bool)GetValue(IconSpinProperty);
        set => SetValue(IconSpinProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public StatusTone Tone
    {
        get => (StatusTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public StatusChip()
    {
        InitializeComponent();
    }

    //--------------------------------------------------------------------------------
    // Tone
    //--------------------------------------------------------------------------------

    private static void OnToneChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var chip = (StatusChip)bindable;
        var (chipColor, iconColor, textColor) = (StatusTone)newValue! switch
        {
            StatusTone.Success => (SuccessChipColor, SuccessColor, SuccessColor),
            StatusTone.Warning => (WarningChipColor, WarningColor, WarningColor),
            StatusTone.Error => (ErrorChipColor, ErrorColor, ErrorColor),
            _ => (NeutralChipColor, NeutralIconColor, NeutralTextColor)
        };
        chip.ChipColor = chipColor;
        chip.IconColor = iconColor;
        chip.TextColor = textColor;
    }
}
