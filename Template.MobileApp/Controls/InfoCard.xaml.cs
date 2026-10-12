namespace Template.MobileApp.Controls;

public partial class InfoCard
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(InfoCard),
        string.Empty);

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(InfoCard),
        string.Empty);

    public static readonly BindableProperty IconSpinProperty = BindableProperty.Create(
        nameof(IconSpin),
        typeof(bool),
        typeof(InfoCard),
        false);

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(InfoCard),
        Color.FromArgb("#455A64"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public InfoCard()
    {
        InitializeComponent();
    }
}
