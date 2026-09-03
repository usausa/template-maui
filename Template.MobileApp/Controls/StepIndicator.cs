namespace Template.MobileApp.Controls;

// ステップ/階層の進行表示。●を横に並べて現在位置をピル型に強調し、「n / N」ラベルを添える
public sealed class StepIndicator : ContentView
{
    public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(
        nameof(CurrentStep),
        typeof(int),
        typeof(StepIndicator),
        1,
        propertyChanged: Rebuild);

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public static readonly BindableProperty TotalStepsProperty = BindableProperty.Create(
        nameof(TotalSteps),
        typeof(int),
        typeof(StepIndicator),
        3,
        propertyChanged: Rebuild);

    public int TotalSteps
    {
        get => (int)GetValue(TotalStepsProperty);
        set => SetValue(TotalStepsProperty, value);
    }

    // 既定値は BlueDefault 相当
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(StepIndicator),
        Color.FromArgb("#2196F3"),
        propertyChanged: Rebuild);

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    // GrayLighten2 / BlueGrayDarken1 相当
    private static readonly Color UpcomingColor = Color.FromArgb("#E0E0E0");
    private static readonly Color LabelColor = Color.FromArgb("#546E7A");

    public StepIndicator()
    {
        BuildContent();
    }

    private static void Rebuild(BindableObject bindable, object oldValue, object newValue)
    {
        ((StepIndicator)bindable).BuildContent();
    }

    private void BuildContent()
    {
        var layout = new HorizontalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center
        };

        for (var i = 1; i <= TotalSteps; i++)
        {
            layout.Children.Add(new BoxView
            {
                WidthRequest = i == CurrentStep ? 24 : 10,
                HeightRequest = 10,
                CornerRadius = 5,
                Color = i <= CurrentStep ? AccentColor : UpcomingColor,
                VerticalOptions = LayoutOptions.Center
            });
        }

        layout.Children.Add(new Label
        {
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = LabelColor,
            VerticalOptions = LayoutOptions.Center,
            Text = String.Format(CultureInfo.CurrentCulture, "{0} / {1}", CurrentStep, TotalSteps)
        });

        Content = layout;
    }
}
