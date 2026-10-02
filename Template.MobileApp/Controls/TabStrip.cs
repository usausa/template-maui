namespace Template.MobileApp.Controls;

// 上部のタブの見出しの列。ItemsSource の項目ごとに ItemTemplate で見出しを作って横に並べる (画面の幅を超えると横にスクロール)。
// 見出しのタップで SelectedIndex を変え、選択の下線を選んだ見出しへ動かして、見出しが見える位置までスクロールする。
// 見出しには VisualState の Selected / Unselected を送るので、選んだ見出しの見た目はテンプレートの VisualStateManager で変える。
// PagePosition (ページのスクロールの位置) を渡すと、スワイプの途中は下線が次の見出しへ伸び縮みしながら追い、見出しの列も流れる。
// 項目の増減は ItemsSource の置き換えで反映する
public sealed class TabStrip : ContentView
{
    private const string AnimationName = "TabStripIndicator";

    private const string ColorAnimationName = "TabStripIndicatorColor";

    private const uint Duration = 250;

    private const string SelectedState = "Selected";

    private const string UnselectedState = "Unselected";

    // 下線は幅を固定して ScaleX で伸縮する (動かす間にレイアウトをやり直さない)
    private const double IndicatorWidth = 100;

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(TabStrip),
        propertyChanged: static (bindable, _, _) => ((TabStrip)bindable).CreateHeaders());

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(TabStrip),
        propertyChanged: static (bindable, _, _) => ((TabStrip)bindable).CreateHeaders());

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(TabStrip),
        0,
        BindingMode.TwoWay,
        propertyChanged: static (bindable, _, _) => ((TabStrip)bindable).UpdateSelection(true));

    public static readonly BindableProperty IndicatorColorProperty = BindableProperty.Create(
        nameof(IndicatorColor),
        typeof(Color),
        typeof(TabStrip),
        Colors.Black,
        propertyChanged: static (bindable, oldValue, newValue) => ((TabStrip)bindable).ChangeIndicatorColor((Color)oldValue, (Color)newValue));

    public static readonly BindableProperty IndicatorHeightProperty = BindableProperty.Create(
        nameof(IndicatorHeight),
        typeof(double),
        typeof(TabStrip),
        3d,
        propertyChanged: static (bindable, _, newValue) => ((TabStrip)bindable).indicator.HeightRequest = (double)newValue);

    // 下線を見出しの両端から内側に寄せる幅
    public static readonly BindableProperty IndicatorInsetProperty = BindableProperty.Create(
        nameof(IndicatorInset),
        typeof(double),
        typeof(TabStrip),
        12d,
        propertyChanged: static (bindable, _, _) => ((TabStrip)bindable).UpdateSelection(false));

    // 既定の NaN のときはスクロールに合わせない
    public static readonly BindableProperty PagePositionProperty = BindableProperty.Create(
        nameof(PagePosition),
        typeof(double),
        typeof(TabStrip),
        Double.NaN,
        propertyChanged: static (bindable, oldValue, newValue) => ((TabStrip)bindable).OnPagePositionChanged((double)oldValue, (double)newValue));

    private readonly ScrollView scroll;

    private readonly HorizontalStackLayout headers;

    private readonly BoxView indicator;

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public double IndicatorHeight
    {
        get => (double)GetValue(IndicatorHeightProperty);
        set => SetValue(IndicatorHeightProperty, value);
    }

    public double IndicatorInset
    {
        get => (double)GetValue(IndicatorInsetProperty);
        set => SetValue(IndicatorInsetProperty, value);
    }

    public double PagePosition
    {
        get => (double)GetValue(PagePositionProperty);
        set => SetValue(PagePositionProperty, value);
    }

    // ページのスクロールが選んだ見出しの近くにあれば、下線はスクロールに合わせる
    private bool IsFollowing => !Double.IsNaN(PagePosition) && (Math.Abs(PagePosition - SelectedIndex) < 0.99);

    public TabStrip()
    {
        headers = [];

        indicator = new BoxView
        {
            WidthRequest = IndicatorWidth,
            HeightRequest = IndicatorHeight,
            Color = IndicatorColor,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            AnchorX = 0,
            ScaleX = 0,
            InputTransparent = true
        };

        scroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = new Grid { headers, indicator }
        };
        Content = scroll;
    }

    private void CreateHeaders()
    {
        headers.Clear();
        if ((ItemsSource is not null) && (ItemTemplate is not null))
        {
            var index = 0;
            foreach (var item in ItemsSource)
            {
                if (ItemTemplate.CreateContent() is View header)
                {
                    header.BindingContext = item;
                    var position = index;
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += (_, _) => SelectedIndex = position;
                    header.GestureRecognizers.Add(tap);
                    // 見出しの大きさが決まったら下線を合わせる (列の大きさの変化は、見出しの配置より先に通知される)
                    header.SizeChanged += (_, _) => UpdateSelection(false);
                    headers.Add(header);
                }

                index++;
            }
        }

        UpdateSelection(false);
    }

    private void UpdateSelection(bool animated)
    {
        var index = SelectedIndex;
        for (var i = 0; i < headers.Count; i++)
        {
            VisualStateManager.GoToState((VisualElement)headers[i], i == index ? SelectedState : UnselectedState);
        }

        if (IsFollowing)
        {
            FollowPosition(PagePosition);
        }
        else if ((index >= 0) && (index < headers.Count) && (headers[index] is View header) && (header.Width > 0))
        {
            var width = Math.Max(header.Width - (IndicatorInset * 2), 0);
            MoveIndicator(header.X + IndicatorInset, width / IndicatorWidth, animated);
            if (animated)
            {
                _ = scroll.ScrollToAsync(header, ScrollToPosition.Center, true);
            }
        }
    }

    // 一度に 1 ページ以上動くのはタブのタップでの移動なので、選択の変更の動きに任せる
    private void OnPagePositionChanged(double oldPosition, double position)
    {
        if (!Double.IsNaN(position) && (Double.IsNaN(oldPosition) || (Math.Abs(position - oldPosition) < 0.99)))
        {
            FollowPosition(position);
        }
    }

    // 下線は、進む側の端を速く、残る側の端を遅く動かして伸び縮みさせる
    private void FollowPosition(double position)
    {
        var index = (int)Math.Floor(position);
        var fraction = position - index;
        if ((index >= 0) && (index < headers.Count) && (headers[index] is View from) && (from.Width > 0))
        {
            var to = (index + 1 < headers.Count) && (headers[index + 1] is View next) ? next : from;
            var left = Lerp(from.X, to.X, fraction * fraction) + IndicatorInset;
            var right = Lerp(from.X + from.Width, to.X + to.Width, 1 - ((1 - fraction) * (1 - fraction))) - IndicatorInset;
            indicator.AbortAnimation(AnimationName);
            indicator.TranslationX = left;
            indicator.ScaleX = Math.Max(right - left, 0) / IndicatorWidth;

            var center = Lerp(from.X + (from.Width / 2), to.X + (to.Width / 2), fraction);
            var x = Math.Clamp(center - (scroll.Width / 2), 0, Math.Max(scroll.ContentSize.Width - scroll.Width, 0));
            _ = scroll.ScrollToAsync(x, 0, false);
        }
    }

    private static double Lerp(double from, double to, double t) => from + ((to - from) * t);

    private void ChangeIndicatorColor(Color from, Color to)
    {
        indicator.AbortAnimation(ColorAnimationName);
        indicator.Animate(
            ColorAnimationName,
            v => indicator.Color = new Color(
                (float)Lerp(from.Red, to.Red, v),
                (float)Lerp(from.Green, to.Green, v),
                (float)Lerp(from.Blue, to.Blue, v),
                (float)Lerp(from.Alpha, to.Alpha, v)));
    }

    private void MoveIndicator(double x, double scale, bool animated)
    {
        indicator.AbortAnimation(AnimationName);
        if (animated)
        {
            var startX = indicator.TranslationX;
            var startScale = indicator.ScaleX;
            indicator.Animate(
                AnimationName,
                v =>
                {
                    indicator.TranslationX = startX + ((x - startX) * v);
                    indicator.ScaleX = startScale + ((scale - startScale) * v);
                },
                16,
                Duration,
                Easing.CubicOut);
        }
        else
        {
            indicator.TranslationX = x;
            indicator.ScaleX = scale;
        }
    }
}
