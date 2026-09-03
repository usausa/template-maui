namespace Template.MobileApp.Controls;

// 時間 + 分 (5 分刻み) の 2 つの Picker を TimeSpan 1 つに束ねる自作部品
public sealed class DurationPicker : ContentView
{
    public static readonly BindableProperty DurationProperty = BindableProperty.Create(
        nameof(Duration),
        typeof(TimeSpan),
        typeof(DurationPicker),
        TimeSpan.Zero,
        BindingMode.TwoWay,
        propertyChanged: static (bindable, _, _) => ((DurationPicker)bindable).UpdateFromDuration());

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    private static readonly Color CaptionColor = Color.FromArgb("#78909C");

    private readonly Picker hoursPicker;
    private readonly Picker minutesPicker;

    private bool updating;

    public DurationPicker()
    {
        hoursPicker = new Picker
        {
            ItemsSource = Enumerable.Range(0, 24).Select(static x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            WidthRequest = 64,
            HorizontalTextAlignment = TextAlignment.Center
        };
        minutesPicker = new Picker
        {
            ItemsSource = Enumerable.Range(0, 12).Select(static x => (x * 5).ToString("00", System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            WidthRequest = 64,
            HorizontalTextAlignment = TextAlignment.Center
        };

        hoursPicker.SelectedIndexChanged += OnSelectionChanged;
        minutesPicker.SelectedIndexChanged += OnSelectionChanged;

        Content = new HorizontalStackLayout
        {
            Spacing = 4,
            Children =
            {
                hoursPicker,
                CreateCaption("時間"),
                minutesPicker,
                CreateCaption("分")
            }
        };

        UpdateFromDuration();
    }

    private static Label CreateCaption(string text) => new()
    {
        Text = text,
        FontSize = 14,
        TextColor = CaptionColor,
        VerticalTextAlignment = TextAlignment.Center
    };

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (updating)
        {
            return;
        }

        Duration = new TimeSpan(
            Math.Max(0, hoursPicker.SelectedIndex),
            Math.Max(0, minutesPicker.SelectedIndex) * 5,
            0);
    }

    private void UpdateFromDuration()
    {
        updating = true;
        hoursPicker.SelectedIndex = Math.Clamp(Duration.Hours, 0, 23);
        minutesPicker.SelectedIndex = Math.Clamp(Duration.Minutes / 5, 0, 11);
        updating = false;
    }
}
