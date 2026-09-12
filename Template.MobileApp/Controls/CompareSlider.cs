namespace Template.MobileApp.Controls;

using Microsoft.Maui.Controls.Shapes;

// Before / After の 2 コンテンツを仕切りのドラッグで見比べるコントロール (Nova.Avalonia.UI の CompareSlider 相当)。
// Before を全面に、After を仕切りより後ろ側 (横なら右、縦なら下) だけクリップして重ね、Pan / Tap で Position (0〜1) を動かす
public sealed class CompareSlider : ContentView
{
    public static readonly BindableProperty BeforeContentProperty = BindableProperty.Create(
        nameof(BeforeContent),
        typeof(View),
        typeof(CompareSlider),
        null,
        propertyChanged: static (bindable, _, newValue) => ((CompareSlider)bindable).beforeHost.Content = newValue as View);

    public static readonly BindableProperty AfterContentProperty = BindableProperty.Create(
        nameof(AfterContent),
        typeof(View),
        typeof(CompareSlider),
        null,
        propertyChanged: static (bindable, _, newValue) => ((CompareSlider)bindable).afterHost.Content = newValue as View);

    public static readonly BindableProperty PositionProperty = BindableProperty.Create(
        nameof(Position),
        typeof(double),
        typeof(CompareSlider),
        0.5d,
        BindingMode.TwoWay,
        coerceValue: static (_, value) => Math.Clamp((double)value, 0d, 1d),
        propertyChanged: static (bindable, _, _) => ((CompareSlider)bindable).UpdateDivider());

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation),
        typeof(StackOrientation),
        typeof(CompareSlider),
        StackOrientation.Horizontal,
        propertyChanged: static (bindable, _, _) => ((CompareSlider)bindable).UpdateOrientation());

    public static readonly BindableProperty HandleColorProperty = BindableProperty.Create(
        nameof(HandleColor),
        typeof(Color),
        typeof(CompareSlider),
        Colors.White,
        propertyChanged: static (bindable, _, _) => ((CompareSlider)bindable).UpdateColors());

    public static readonly BindableProperty HandleSizeProperty = BindableProperty.Create(
        nameof(HandleSize),
        typeof(double),
        typeof(CompareSlider),
        36d,
        propertyChanged: static (bindable, _, _) => ((CompareSlider)bindable).UpdateHandleSize());

    public View? BeforeContent
    {
        get => (View?)GetValue(BeforeContentProperty);
        set => SetValue(BeforeContentProperty, value);
    }

    public View? AfterContent
    {
        get => (View?)GetValue(AfterContentProperty);
        set => SetValue(AfterContentProperty, value);
    }

    public double Position
    {
        get => (double)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Color HandleColor
    {
        get => (Color)GetValue(HandleColorProperty);
        set => SetValue(HandleColorProperty, value);
    }

    public double HandleSize
    {
        get => (double)GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    private const double DividerThickness = 2d;

    // ハンドルのアイコン色は BlueGrayDarken2 相当
    private static readonly Color HandleIconColor = Color.FromArgb("#455A64");

    private readonly Grid root;
    private readonly ContentView beforeHost;
    private readonly ContentView afterHost;
    private readonly BoxView divider;
    private readonly Border handle;
    private readonly Label handleIcon;

    private double panStart;

    public CompareSlider()
    {
        beforeHost = new ContentView();
        afterHost = new ContentView();

        // 仕切りとハンドルはタッチを透過させ、ジェスチャは root で受ける
        divider = new BoxView { InputTransparent = true };
        handleIcon = new Label
        {
            FontFamily = Fonts.MaterialIcons.FontFamily,
            FontSize = 20,
            TextColor = HandleIconColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        handle = new Border
        {
            StrokeThickness = 0,
            InputTransparent = true,
            Content = handleIcon
        };

        root = [beforeHost, afterHost, divider, handle];
        root.SizeChanged += (_, _) => UpdateDivider();

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        root.GestureRecognizers.Add(pan);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        root.GestureRecognizers.Add(tap);

        Content = root;

        UpdateHandleSize();
        UpdateColors();
        UpdateOrientation();
    }

    private bool IsHorizontal => Orientation == StackOrientation.Horizontal;

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panStart = Position;
                break;
            case GestureStatus.Running:
                var length = IsHorizontal ? root.Width : root.Height;
                if (length > 0d)
                {
                    Position = panStart + ((IsHorizontal ? e.TotalX : e.TotalY) / length);
                }

                break;
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        var point = e.GetPosition(root);
        if (point is null)
        {
            return;
        }

        var length = IsHorizontal ? root.Width : root.Height;
        if (length > 0d)
        {
            Position = (IsHorizontal ? point.Value.X : point.Value.Y) / length;
        }
    }

    private void UpdateOrientation()
    {
        if (IsHorizontal)
        {
            divider.WidthRequest = DividerThickness;
            divider.HeightRequest = -1;
            divider.HorizontalOptions = LayoutOptions.Start;
            divider.VerticalOptions = LayoutOptions.Fill;
            handle.HorizontalOptions = LayoutOptions.Start;
            handle.VerticalOptions = LayoutOptions.Center;
            handleIcon.Text = Fonts.MaterialIcons.Compare_arrows;
        }
        else
        {
            divider.WidthRequest = -1;
            divider.HeightRequest = DividerThickness;
            divider.HorizontalOptions = LayoutOptions.Fill;
            divider.VerticalOptions = LayoutOptions.Start;
            handle.HorizontalOptions = LayoutOptions.Center;
            handle.VerticalOptions = LayoutOptions.Start;
            handleIcon.Text = Fonts.MaterialIcons.Unfold_more;
        }

        UpdateDivider();
    }

    private void UpdateColors()
    {
        divider.Color = HandleColor;
        handle.BackgroundColor = HandleColor;
    }

    private void UpdateHandleSize()
    {
        handle.WidthRequest = HandleSize;
        handle.HeightRequest = HandleSize;
        handle.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(HandleSize / 2d) };
        UpdateDivider();
    }

    // Position に合わせて After のクリップ領域と仕切り・ハンドルの位置を更新する
    private void UpdateDivider()
    {
        var width = root.Width;
        var height = root.Height;
        if ((width <= 0d) || (height <= 0d))
        {
            return;
        }

        if (IsHorizontal)
        {
            var x = width * Position;
            afterHost.Clip = new RectangleGeometry(new Rect(x, 0d, Math.Max(0d, width - x), height));
            divider.TranslationX = x - (DividerThickness / 2d);
            divider.TranslationY = 0d;
            handle.TranslationX = x - (HandleSize / 2d);
            handle.TranslationY = 0d;
        }
        else
        {
            var y = height * Position;
            afterHost.Clip = new RectangleGeometry(new Rect(0d, y, width, Math.Max(0d, height - y)));
            divider.TranslationX = 0d;
            divider.TranslationY = y - (DividerThickness / 2d);
            handle.TranslationX = 0d;
            handle.TranslationY = y - (HandleSize / 2d);
        }
    }
}
