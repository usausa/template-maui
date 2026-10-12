namespace Template.MobileApp.Controls;

public partial class JoyStickView
{
    private const int ValueRange = 255;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ModeProperty = BindableProperty.Create(
        nameof(Mode),
        typeof(JoyStickMode),
        typeof(JoyStickView),
        JoyStickMode.Both,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbSizeProperty = BindableProperty.Create(
        nameof(ThumbSize),
        typeof(double),
        typeof(JoyStickView),
        80.0);

    public static readonly BindableProperty RangeOfMotionProperty = BindableProperty.Create(
        nameof(RangeOfMotion),
        typeof(double),
        typeof(JoyStickView),
        56.0);

    public static readonly BindableProperty ThumbBorderWidthProperty = BindableProperty.Create(
        nameof(ThumbBorderWidth),
        typeof(double),
        typeof(JoyStickView),
        8.0);

    public static readonly BindableProperty BaseBackgroundProperty = BindableProperty.Create(
        nameof(BaseBackground),
        typeof(Brush),
        typeof(JoyStickView),
        Brush.LightGray,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbBackgroundProperty = BindableProperty.Create(
        nameof(ThumbBackground),
        typeof(Brush),
        typeof(JoyStickView),
        Brush.Red,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ThumbBorderBackgroundProperty = BindableProperty.Create(
        nameof(ThumbBorderBackground),
        typeof(Brush),
        typeof(JoyStickView),
        Brush.DarkRed,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
        nameof(ArrowColor),
        typeof(Color),
        typeof(JoyStickView),
        Colors.White);

    public static readonly BindableProperty ArrowMarginRatioProperty = BindableProperty.Create(
        nameof(ArrowMarginRatio),
        typeof(double),
        typeof(JoyStickView),
        0.1d,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ArrowHeightRatioProperty = BindableProperty.Create(
        nameof(ArrowHeightRatio),
        typeof(double),
        typeof(JoyStickView),
        0.2d,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ArrowHalfWidthRatioProperty = BindableProperty.Create(
        nameof(ArrowHalfWidthRatio),
        typeof(double),
        typeof(JoyStickView),
        0.2d,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty XValueProperty = BindableProperty.Create(
        nameof(XValue),
        typeof(int),
        typeof(JoyStickView),
        defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly BindableProperty YValueProperty = BindableProperty.Create(
        nameof(YValue),
        typeof(int),
        typeof(JoyStickView),
        defaultBindingMode: BindingMode.OneWayToSource);

    public JoyStickMode Mode
    {
        get => (JoyStickMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    public double RangeOfMotion
    {
        get => (double)GetValue(RangeOfMotionProperty);
        set => SetValue(RangeOfMotionProperty, value);
    }

    public double ThumbBorderWidth
    {
        get => (double)GetValue(ThumbBorderWidthProperty);
        set => SetValue(ThumbBorderWidthProperty, value);
    }

    public Brush BaseBackground
    {
        get => (Brush)GetValue(BaseBackgroundProperty);
        set => SetValue(BaseBackgroundProperty, value);
    }

    public Brush ThumbBackground
    {
        get => (Brush)GetValue(ThumbBackgroundProperty);
        set => SetValue(ThumbBackgroundProperty, value);
    }

    public Brush ThumbBorderBackground
    {
        get => (Brush)GetValue(ThumbBorderBackgroundProperty);
        set => SetValue(ThumbBorderBackgroundProperty, value);
    }

    public Color ArrowColor
    {
        get => (Color)GetValue(ArrowColorProperty);
        set => SetValue(ArrowColorProperty, value);
    }

    public double ArrowMarginRatio
    {
        get => (double)GetValue(ArrowMarginRatioProperty);
        set => SetValue(ArrowMarginRatioProperty, value);
    }

    public double ArrowHeightRatio
    {
        get => (double)GetValue(ArrowHeightRatioProperty);
        set => SetValue(ArrowHeightRatioProperty, value);
    }

    public double ArrowHalfWidthRatio
    {
        get => (double)GetValue(ArrowHalfWidthRatioProperty);
        set => SetValue(ArrowHalfWidthRatioProperty, value);
    }

    public int XValue
    {
        get => (int)GetValue(XValueProperty);
        private set => SetValue(XValueProperty, value);
    }

    public int YValue
    {
        get => (int)GetValue(YValueProperty);
        private set => SetValue(YValueProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public JoyStickView()
    {
        InitializeComponent();

        SizeChanged += (_, _) => Update();

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        ThumbGrid.GestureRecognizers.Add(panGesture);
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((JoyStickView)bindable).Update();
    }

    private void Update()
    {
        if ((Width <= 0) || (Height <= 0))
        {
            return;
        }

        var size = Math.Min(Width, Height);
        var r = size / 2.0;
        var cx = Width / 2.0;
        var cy = Height / 2.0;

        BaseEllipse.WidthRequest = size;
        BaseEllipse.HeightRequest = size;

        var margin = r * ArrowMarginRatio;
        var h = r * ArrowHeightRatio;
        var w = r * ArrowHalfWidthRatio;

        var horizontal = (Mode & JoyStickMode.Horizontal) == JoyStickMode.Horizontal;
        var vertical = (Mode & JoyStickMode.Vertical) == JoyStickMode.Vertical;

        var apexUp = new Point(cx, cy - r + margin);
        UpArrow.Points = [apexUp, new Point(cx - w, apexUp.Y + h), new Point(cx + w, apexUp.Y + h)];
        UpArrow.IsVisible = vertical;
        var apexDown = new Point(cx, cy + r - margin);
        DownArrow.Points = [apexDown, new Point(cx + w, apexDown.Y - h), new Point(cx - w, apexDown.Y - h)];
        DownArrow.IsVisible = vertical;
        var apexLeft = new Point(cx - r + margin, cy);
        LeftArrow.Points = [apexLeft, new Point(apexLeft.X + h, cy - w), new Point(apexLeft.X + h, cy + w)];
        LeftArrow.IsVisible = horizontal;
        var apexRight = new Point(cx + r - margin, cy);
        RightArrow.Points = [apexRight, new Point(apexRight.X - h, cy + w), new Point(apexRight.X - h, cy - w)];
        RightArrow.IsVisible = horizontal;
    }

    //--------------------------------------------------------------------------------
    // Drag
    //--------------------------------------------------------------------------------

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        var radius = RangeOfMotion;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
            case GestureStatus.Running:
                var (x, y) = ClampToCircle(radius, e.TotalX, e.TotalY);
                ThumbBorder.TranslationX = x;
                ThumbBorder.TranslationY = y;
                Thumb.TranslationX = x;
                Thumb.TranslationY = y;

                XValue = (int)Math.Round(ValueRange * (x / radius), MidpointRounding.AwayFromZero);
                YValue = (int)Math.Round(ValueRange * (y / radius), MidpointRounding.AwayFromZero);
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                ThumbBorder.TranslationX = 0;
                ThumbBorder.TranslationY = 0;
                Thumb.TranslationX = 0;
                Thumb.TranslationY = 0;

                XValue = 0;
                YValue = 0;
                break;
        }
    }

    public (double X, double Y) ClampToCircle(double radius, double x, double y)
    {
        switch (Mode)
        {
            case JoyStickMode.Both:
                var distance = Math.Sqrt((x * x) + (y * y));
                return distance > radius ? (x * (radius / distance), y * (radius / distance)) : (x, y);
            case JoyStickMode.Horizontal:
                return (Math.Clamp(x, -radius, radius), 0);
            case JoyStickMode.Vertical:
                return (0, Math.Clamp(y, -radius, radius));
        }

        return (0, 0);
    }
}
