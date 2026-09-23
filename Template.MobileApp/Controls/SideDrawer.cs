namespace Template.MobileApp.Controls;

// 左端から出るドロワー (自作)。ページの最前面に置く。IsOpen で表示、左端のスワイプで開く、
// パネルと背景のドラッグ (スワイプ) と背景のタップで閉じる。閉じているときは左端の帯以外はタッチを通す
// ドラッグの終了時は、離したときの速さ (止まっていれば無し) か最後に動かした方向で開閉を決め、方向が明確でなければ位置 (半分) で決める。
// ドラッグの続きの移動は、離したときの速さから減速して止まる (残りの距離に応じた時間)
public sealed partial class SideDrawer : Grid
{
    private const string AnimationName = "DrawerMove";

    private const uint Duration = 150;

    // ドラッグの続きとして動かす時間の範囲
    private const uint MinFlingDuration = 80;

    private const uint MaxFlingDuration = 400;

    // SinOut の初速は平均の π/2 倍 (時間 = 傾き × 距離 / 速さ で初速を離したときの速さに合わせる)
    private const double FlingSlope = Math.PI / 2;

    private const double BackdropOpacity = 0.4;

    // 開閉を確定する移動量 (dp) と速さ (dp/ms)。短い操作でも方向が明確なら従う
    private const double SwipeDistance = 16;

    private const double SwipeVelocity = 0.2;

    // 方向とみなす 1 回の移動量 (dp)。離す前にこの時間 (ms) 動いていなければ速さは無いものとする
    private const double DirectionDistance = 1;

    private const long PauseInterval = 100;

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen),
        typeof(bool),
        typeof(SideDrawer),
        false,
        BindingMode.TwoWay,
        propertyChanged: static (bindable, _, newValue) => ((SideDrawer)bindable).OnIsOpenChanged((bool)newValue));

    public static readonly BindableProperty DrawerContentProperty = BindableProperty.Create(
        nameof(DrawerContent),
        typeof(View),
        typeof(SideDrawer),
        propertyChanged: static (bindable, _, newValue) => ((SideDrawer)bindable).SetDrawerContent((View?)newValue));

    public static readonly BindableProperty DrawerWidthProperty = BindableProperty.Create(
        nameof(DrawerWidth),
        typeof(double),
        typeof(SideDrawer),
        280d,
        propertyChanged: static (bindable, _, _) => ((SideDrawer)bindable).UpdatePanelSize());

    // 左端のスワイプで開くか (無効にしても IsOpen とパネルのドラッグは使える)
    public static readonly BindableProperty EdgeSwipeEnabledProperty = BindableProperty.Create(
        nameof(EdgeSwipeEnabled),
        typeof(bool),
        typeof(SideDrawer),
        true,
        propertyChanged: static (bindable, _, newValue) => ((SideDrawer)bindable).edge.IsVisible = (bool)newValue);

    public static readonly BindableProperty EdgeWidthProperty = BindableProperty.Create(
        nameof(EdgeWidth),
        typeof(double),
        typeof(SideDrawer),
        24d,
        propertyChanged: static (bindable, _, newValue) => ((SideDrawer)bindable).edge.WidthRequest = (double)newValue);

    public static readonly BindableProperty DrawerBackgroundColorProperty = BindableProperty.Create(
        nameof(DrawerBackgroundColor),
        typeof(Color),
        typeof(SideDrawer),
        Colors.White,
        propertyChanged: static (bindable, _, newValue) => ((SideDrawer)bindable).panel.BackgroundColor = (Color)newValue);

    private readonly Grid edge;

    private readonly BoxView backdrop;

    private readonly Grid panel;

    private double panStart;

    private double panTotal;

    private double panVelocity;

    // 最後に動かした方向 (右 = 1 / 左 = -1)
    private int panDirection;

    private long panTimestamp;

    // ドラッグから開閉するときの時間 (IsOpen の変更で始まるアニメーションに渡す)
    private uint? flingDuration;

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public View? DrawerContent
    {
        get => (View?)GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    public double DrawerWidth
    {
        get => (double)GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    public bool EdgeSwipeEnabled
    {
        get => (bool)GetValue(EdgeSwipeEnabledProperty);
        set => SetValue(EdgeSwipeEnabledProperty, value);
    }

    public double EdgeWidth
    {
        get => (double)GetValue(EdgeWidthProperty);
        set => SetValue(EdgeWidthProperty, value);
    }

    public Color DrawerBackgroundColor
    {
        get => (Color)GetValue(DrawerBackgroundColorProperty);
        set => SetValue(DrawerBackgroundColorProperty, value);
    }

    public SideDrawer()
    {
        // コンテナ自体はタッチを通し、子 (左端の帯・背景・パネル) だけが受ける
        InputTransparent = true;
        CascadeInputTransparent = false;

        // ジェスチャはレイアウト (Grid) に付ける。完全な透明はタッチが届かないため、ごく薄い色を付ける
        edge = new Grid
        {
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = EdgeWidth,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.01)
        };
        var edgePan = new PanGestureRecognizer();
        edgePan.PanUpdated += OnPanUpdated;
        edge.GestureRecognizers.Add(edgePan);
        edge.HandlerChanged += (_, _) => UpdateGestureExclusion();
        edge.SizeChanged += (_, _) => UpdateGestureExclusion();

        backdrop = new BoxView { Color = Colors.Black, Opacity = 0, IsVisible = false };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => IsOpen = false;
        backdrop.GestureRecognizers.Add(tap);
        // 背景のドラッグもパネルのドラッグと同じ扱い (開いているときは画面のどこからでもスワイプで閉じる)
        var backdropPan = new PanGestureRecognizer();
        backdropPan.PanUpdated += OnPanUpdated;
        backdrop.GestureRecognizers.Add(backdropPan);

        panel = new Grid
        {
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = DrawerWidth,
            BackgroundColor = DrawerBackgroundColor,
            TranslationX = -DrawerWidth,
            IsVisible = false,
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.25f, Radius = 12, Offset = new Point(2, 0) }
        };
        var panelPan = new PanGestureRecognizer();
        panelPan.PanUpdated += OnPanUpdated;
        panel.GestureRecognizers.Add(panelPan);

        Children.Add(edge);
        Children.Add(backdrop);
        Children.Add(panel);
    }

    // 左端はシステムの戻るジェスチャと重なるため、プラットフォーム側で帯の一部をシステムジェスチャから除外する
    partial void UpdateGestureExclusion();

    private void SetDrawerContent(View? view)
    {
        panel.Children.Clear();
        if (view is not null)
        {
            panel.Children.Add(view);
        }
    }

    private void UpdatePanelSize()
    {
        panel.WidthRequest = DrawerWidth;
        if (!IsOpen)
        {
            panel.TranslationX = -DrawerWidth;
        }
    }

    private void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            backdrop.IsVisible = true;
            panel.IsVisible = true;
        }

        // ボタンや背景のタップからは通常の時間、ドラッグの続きは離したときの速さから減速する
        var duration = flingDuration ?? Duration;
        var easing = flingDuration is null ? null : Easing.SinOut;
        flingDuration = null;
        MoveTo(open ? 0 : -DrawerWidth, open, duration, easing);
    }

    private void MoveTo(double x, bool open, uint duration = Duration, Easing? easing = null)
    {
        panel.AbortAnimation(AnimationName);
        var start = panel.TranslationX;
        var startOpacity = backdrop.Opacity;
        var endOpacity = open ? BackdropOpacity : 0;
        panel.Animate(
            AnimationName,
            v =>
            {
                panel.TranslationX = start + ((x - start) * v);
                backdrop.Opacity = startOpacity + ((endOpacity - startOpacity) * v);
            },
            16,
            duration,
            easing ?? (open ? Easing.CubicOut : Easing.CubicIn),
            (_, _) =>
            {
                backdrop.IsVisible = IsOpen;
                panel.IsVisible = IsOpen;
            });
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panel.AbortAnimation(AnimationName);
                panStart = panel.TranslationX;
                panTotal = 0;
                panVelocity = 0;
                panDirection = 0;
                panTimestamp = Environment.TickCount64;
                backdrop.IsVisible = true;
                panel.IsVisible = true;
                break;
            case GestureStatus.Running:
                var delta = e.TotalX - panTotal;
                var now = Environment.TickCount64;
                var elapsed = now - panTimestamp;
                if ((delta != 0) && (elapsed > 0))
                {
                    // 直前の区間の速さ (揺れを抑えるため前回と平均する。逆方向に転じたら置き換える)
                    var velocity = delta / elapsed;
                    panVelocity = (panVelocity == 0) || (Math.Sign(velocity) != Math.Sign(panVelocity)) ? velocity : (panVelocity + velocity) / 2;
                    panTimestamp = now;
                }

                if (Math.Abs(delta) >= DirectionDistance)
                {
                    panDirection = Math.Sign(delta);
                }

                panTotal = e.TotalX;
                var x = Math.Clamp(panStart + e.TotalX, -DrawerWidth, 0);
                panel.TranslationX = x;
                backdrop.Opacity = BackdropOpacity * (1 + (x / DrawerWidth));
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                Settle();
                break;
        }
    }

    // 右方向 (速いか、最後に右へ動かして合計が距離を超えた) は開く、左方向は閉じる。方向が明確でなければ半分の位置で決める。
    // IsOpen が変わらない場合もあるため位置は明示的に戻す
    private void Settle()
    {
        var x = panel.TranslationX;
        var velocity = Environment.TickCount64 - panTimestamp > PauseInterval ? 0 : panVelocity;
        bool open;
        if ((velocity >= SwipeVelocity) || ((velocity >= 0) && (panDirection > 0) && (panTotal >= SwipeDistance)))
        {
            open = true;
        }
        else if ((velocity <= -SwipeVelocity) || ((velocity <= 0) && (panDirection < 0) && (panTotal <= -SwipeDistance)))
        {
            open = false;
        }
        else
        {
            open = x > -DrawerWidth / 2;
        }

        var duration = FlingDuration(open ? -x : x + DrawerWidth, open ? velocity : -velocity);
        if (open == IsOpen)
        {
            MoveTo(open ? 0 : -DrawerWidth, open, duration, Easing.SinOut);
        }
        else
        {
            flingDuration = duration;
            IsOpen = open;
        }
    }

    // 離したときの速さ (dp/ms) で残りの距離を進む時間。逆方向や停止からは通常の時間
    private static uint FlingDuration(double distance, double velocity) =>
        velocity > 0 ? (uint)Math.Clamp(FlingSlope * distance / velocity, MinFlingDuration, MaxFlingDuration) : Duration;
}
