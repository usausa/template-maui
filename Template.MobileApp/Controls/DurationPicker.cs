namespace Template.MobileApp.Controls;

public sealed class DurationPicker : ContentView
{
    private const int HourCount = 24;

    private const int MinuteStep = 5;

    private const int MinuteCount = 60 / MinuteStep;

    private const double PickerWidth = 64;

    private const double ItemSpacing = 4;

    private const double CaptionFontSize = 14;

    private static readonly Color CaptionColor = Color.FromArgb("#78909C");

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly Picker hoursPicker;

    private readonly Picker minutesPicker;

    private bool updating;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DurationPicker()
    {
        hoursPicker = new Picker
        {
            ItemsSource = Enumerable.Range(0, HourCount).Select(static x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            WidthRequest = PickerWidth,
            HorizontalTextAlignment = TextAlignment.Center
        };
        minutesPicker = new Picker
        {
            ItemsSource = Enumerable.Range(0, MinuteCount).Select(static x => (x * MinuteStep).ToString("00", System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            WidthRequest = PickerWidth,
            HorizontalTextAlignment = TextAlignment.Center
        };

        hoursPicker.SelectedIndexChanged += OnSelectionChanged;
        minutesPicker.SelectedIndexChanged += OnSelectionChanged;

        Content = new HorizontalStackLayout
        {
            Spacing = ItemSpacing,
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

    //--------------------------------------------------------------------------------
    // Content
    //--------------------------------------------------------------------------------

    private static Label CreateCaption(string text) => new()
    {
        Text = text,
        FontSize = CaptionFontSize,
        TextColor = CaptionColor,
        VerticalTextAlignment = TextAlignment.Center
    };

    //--------------------------------------------------------------------------------
    // Selection
    //--------------------------------------------------------------------------------

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (updating)
        {
            return;
        }

        Duration = new TimeSpan(
            Math.Max(0, hoursPicker.SelectedIndex),
            Math.Max(0, minutesPicker.SelectedIndex) * MinuteStep,
            0);
    }

    private void UpdateFromDuration()
    {
        updating = true;
        hoursPicker.SelectedIndex = Math.Clamp(Duration.Hours, 0, HourCount - 1);
        minutesPicker.SelectedIndex = Math.Clamp(Duration.Minutes / MinuteStep, 0, MinuteCount - 1);
        updating = false;
    }
}
