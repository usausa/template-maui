namespace Template.MobileApp.Controls;

// 見出し(アイコン+タイトル)付きの角丸カード。Name/Value 羅列などの本文を Content に載せる
public partial class InfoCard
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(InfoCard),
        string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(InfoCard),
        string.Empty);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // 既定値は BlueGrayDarken2 相当
    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(InfoCard),
        Color.FromArgb("#455A64"));

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public InfoCard()
    {
        InitializeComponent();
    }
}
