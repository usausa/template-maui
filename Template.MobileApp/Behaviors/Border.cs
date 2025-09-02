namespace Template.MobileApp.Behaviors;

public static partial class Border
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty WidthProperty =
        BindableProperty.CreateAttached(
            "Width",
            typeof(double?),
            typeof(Border),
            default(double?));

    public static readonly BindableProperty ColorProperty =
        BindableProperty.CreateAttached(
            "Color",
            typeof(Color),
            typeof(Border),
            Colors.Transparent);

    public static readonly BindableProperty PaddingProperty =
        BindableProperty.CreateAttached(
            "Padding",
            typeof(Thickness),
            typeof(Border),
            default(Thickness));

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.CreateAttached(
            "Radius",
            typeof(double?),
            typeof(Border),
            default(double?));
    // ReSharper restore InconsistentNaming

    public static void SetWidth(BindableObject bindable, double? value) => bindable.SetValue(WidthProperty, value);

    public static double? GetWidth(BindableObject bindable) => (double?)bindable.GetValue(WidthProperty);

    public static void SetColor(BindableObject bindable, Color value) => bindable.SetValue(ColorProperty, value);

    public static Color GetColor(BindableObject bindable) => (Color)bindable.GetValue(ColorProperty);

    public static void SetPadding(BindableObject bindable, Thickness value) => bindable.SetValue(PaddingProperty, value);

    public static Thickness GetPadding(BindableObject bindable) => (Thickness)bindable.GetValue(PaddingProperty);

    public static void SetRadius(BindableObject bindable, double? value) => bindable.SetValue(RadiusProperty, value);

    public static double? GetRadius(BindableObject bindable) => (double?)bindable.GetValue(RadiusProperty);
}
