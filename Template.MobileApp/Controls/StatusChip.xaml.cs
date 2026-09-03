namespace Template.MobileApp.Controls;

// アイコン+テキストの小型状態チップ。状態に応じて ChipColor / TextColor を差し替えて使う
public partial class StatusChip
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StatusChip),
        string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(StatusChip),
        string.Empty);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // 既定値は GrayLighten4 相当
    public static readonly BindableProperty ChipColorProperty = BindableProperty.Create(
        nameof(ChipColor),
        typeof(Color),
        typeof(StatusChip),
        Color.FromArgb("#F5F5F5"));

    public Color ChipColor
    {
        get => (Color)GetValue(ChipColorProperty);
        set => SetValue(ChipColorProperty, value);
    }

    // 既定値は BlueGrayDarken1 相当
    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(StatusChip),
        Color.FromArgb("#546E7A"));

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    // 既定値は BlueGrayDarken2 相当
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(StatusChip),
        Color.FromArgb("#455A64"));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public StatusChip()
    {
        InitializeComponent();
    }
}
