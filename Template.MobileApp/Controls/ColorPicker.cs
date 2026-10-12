namespace Template.MobileApp.Controls;

using Microsoft.Maui.Controls.Shapes;

public sealed class ColorPicker : ContentView
{
    private const double ChannelMaximum = 255;

    private const double SliderSpacing = 2;

    private const double ItemSpacing = 8;

    private const double CaptionFontSize = 12;

    private const double CaptionWidth = 16;

    private const double PreviewCornerRadius = 8;

    private const double PreviewHeight = 44;

    private const double ValueFontSize = 12;

    private const double ChipHorizontalPadding = 8;

    private const double ChipVerticalPadding = 2;

    private const double ChipCornerRadius = 10;

    private static readonly Color CaptionColor = Color.FromArgb("#78909C");

    private static readonly Color ChipColor = Color.FromArgb("#F5F5F5");

    private static readonly Color ChipTextColor = Color.FromArgb("#455A64");

    private static readonly Color RedColor = Color.FromArgb("#F44336");

    private static readonly Color GreenColor = Color.FromArgb("#4CAF50");

    private static readonly Color BlueColor = Color.FromArgb("#2196F3");

    private static readonly Color AlphaColor = Color.FromArgb("#9E9E9E");

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly Slider redSlider;

    private readonly Slider greenSlider;

    private readonly Slider blueSlider;

    private readonly Slider alphaSlider;

    private readonly Label valueLabel;

    private readonly BoxView preview;

    private bool updating;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
        nameof(SelectedColor),
        typeof(Color),
        typeof(ColorPicker),
        Colors.CornflowerBlue,
        BindingMode.TwoWay,
        propertyChanged: static (bindable, _, _) => ((ColorPicker)bindable).UpdateFromColor());

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public ColorPicker()
    {
        redSlider = CreateSlider(RedColor);
        greenSlider = CreateSlider(GreenColor);
        blueSlider = CreateSlider(BlueColor);
        alphaSlider = CreateSlider(AlphaColor);

        preview = new BoxView { CornerRadius = PreviewCornerRadius, HeightRequest = PreviewHeight };
        valueLabel = new Label
        {
            FontFamily = "JetBrainsMono",
            FontSize = ValueFontSize,
            TextColor = ChipTextColor
        };
        var valueChip = new Border
        {
            Padding = new Thickness(ChipHorizontalPadding, ChipVerticalPadding),
            BackgroundColor = ChipColor,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ChipCornerRadius },
            VerticalOptions = LayoutOptions.Center,
            Content = valueLabel
        };

        var grid = new Grid
        {
            RowSpacing = SliderSpacing,
            ColumnSpacing = ItemSpacing,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        AddRow(grid, 0, "R", redSlider);
        AddRow(grid, 1, "G", greenSlider);
        AddRow(grid, 2, "B", blueSlider);
        AddRow(grid, 3, "A", alphaSlider);

        var previewGrid = new Grid
        {
            ColumnSpacing = ItemSpacing,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        previewGrid.Add(preview, 0);
        previewGrid.Add(valueChip, 1);
        grid.Add(previewGrid, 0, 4);
        Grid.SetColumnSpan(previewGrid, 2);

        Content = grid;

        UpdateFromColor();
    }

    //--------------------------------------------------------------------------------
    // Content
    //--------------------------------------------------------------------------------

    private Slider CreateSlider(Color color)
    {
        var slider = new Slider { Minimum = 0, Maximum = ChannelMaximum, MinimumTrackColor = color, ThumbColor = color };
        slider.ValueChanged += (_, _) =>
        {
            if (updating)
            {
                return;
            }

            SelectedColor = Color.FromRgba(
                (int)redSlider.Value,
                (int)greenSlider.Value,
                (int)blueSlider.Value,
                (int)alphaSlider.Value);
        };
        return slider;
    }

    private static void AddRow(Grid grid, int row, string caption, Slider slider)
    {
        var label = new Label
        {
            Text = caption,
            FontSize = CaptionFontSize,
            TextColor = CaptionColor,
            WidthRequest = CaptionWidth,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(label, 0, row);
        grid.Add(slider, 1, row);
    }

    //--------------------------------------------------------------------------------
    // Selection
    //--------------------------------------------------------------------------------

    private void UpdateFromColor()
    {
        var color = SelectedColor;

        updating = true;
        redSlider.Value = color.Red * ChannelMaximum;
        greenSlider.Value = color.Green * ChannelMaximum;
        blueSlider.Value = color.Blue * ChannelMaximum;
        alphaSlider.Value = color.Alpha * ChannelMaximum;
        updating = false;

        preview.Color = color;
        valueLabel.Text = color.ToArgbHex();
    }
}
