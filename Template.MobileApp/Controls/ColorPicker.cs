namespace Template.MobileApp.Controls;

// RGBA スライダ 4 本 + プレビューの色選択コントロール (両 Toolkit に無い自作部品)
public sealed class ColorPicker : ContentView
{
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

    private static readonly Color CaptionColor = Color.FromArgb("#78909C");

    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Slider alphaSlider;
    private readonly Label valueLabel;
    private readonly BoxView preview;

    private bool updating;

    public ColorPicker()
    {
        redSlider = CreateSlider();
        greenSlider = CreateSlider();
        blueSlider = CreateSlider();
        alphaSlider = CreateSlider();

        preview = new BoxView { CornerRadius = 8, HeightRequest = 44 };
        valueLabel = new Label
        {
            FontSize = 12,
            TextColor = CaptionColor,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.End
        };

        var grid = new Grid
        {
            RowSpacing = 2,
            ColumnSpacing = 8,
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
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        previewGrid.Add(preview, 0);
        previewGrid.Add(valueLabel, 1);
        grid.Add(previewGrid, 0, 4);
        Grid.SetColumnSpan(previewGrid, 2);

        Content = grid;

        UpdateFromColor();
    }

    private Slider CreateSlider()
    {
        var slider = new Slider { Minimum = 0, Maximum = 255 };
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
            FontSize = 12,
            TextColor = CaptionColor,
            WidthRequest = 16,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(label, 0, row);
        grid.Add(slider, 1, row);
    }

    private void UpdateFromColor()
    {
        var color = SelectedColor;

        updating = true;
        redSlider.Value = color.Red * 255d;
        greenSlider.Value = color.Green * 255d;
        blueSlider.Value = color.Blue * 255d;
        alphaSlider.Value = color.Alpha * 255d;
        updating = false;

        preview.Color = color;
        valueLabel.Text = color.ToArgbHex();
    }
}
