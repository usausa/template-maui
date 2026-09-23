namespace Template.MobileApp.Controls;

using Microsoft.Maui.Controls.Shapes;

// 下から出るシート (自作)。ページの最前面に置く。IsOpen で半開の状態から表示し、
// シートのドラッグで 半開 ⇔ 全開 ⇔ 閉じる、背景のタップで閉じる。
// シートの高さは内容に合わせる (上限は ExpandedRatio)。内容が半開の高さに収まるなら全開の状態は無い
// ドラッグの終了時は、離したときの速さ (止まっていれば無し) か最後に動かした方向で次の状態へ、方向が明確でなければ離した位置に近い状態へ。
// ドラッグの続きの移動は、離したときの速さから減速して止まる (残りの距離に応じた時間)
public sealed class BottomSheetView : Grid
{
    private const string AnimationName = "SheetMove";

    private const uint Duration = 250;

    // ドラッグの続きとして動かす時間の範囲
    private const uint MinFlingDuration = 80;

    private const uint MaxFlingDuration = 400;

    // SinOut の初速は平均の π/2 倍 (時間 = 傾き × 距離 / 速さ で初速を離したときの速さに合わせる)
    private const double FlingSlope = Math.PI / 2;

    private const double BackdropOpacity = 0.4;

    // 状態の移動を確定する移動量 (dp) と速さ (dp/ms)。短い操作でも方向が明確なら従う
    private const double SwipeDistance = 16;

    private const double SwipeVelocity = 0.2;

    // 方向とみなす 1 回の移動量 (dp)。離す前にこの時間 (ms) 動いていなければ速さは無いものとする
    private const double DirectionDistance = 1;

    private const long PauseInterval = 100;

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen),
        typeof(bool),
        typeof(BottomSheetView),
        false,
        BindingMode.TwoWay,
        propertyChanged: static (bindable, _, newValue) => ((BottomSheetView)bindable).OnIsOpenChanged((bool)newValue));

    public static readonly BindableProperty SheetContentProperty = BindableProperty.Create(
        nameof(SheetContent),
        typeof(View),
        typeof(BottomSheetView),
        propertyChanged: static (bindable, _, newValue) => ((BottomSheetView)bindable).contentHost.Content = (View?)newValue);

    // 半開のときにシートが占める高さの割合
    public static readonly BindableProperty HalfExpandedRatioProperty = BindableProperty.Create(
        nameof(HalfExpandedRatio),
        typeof(double),
        typeof(BottomSheetView),
        0.5d);

    // 全開のときにシートが占める高さの割合
    public static readonly BindableProperty ExpandedRatioProperty = BindableProperty.Create(
        nameof(ExpandedRatio),
        typeof(double),
        typeof(BottomSheetView),
        0.92d,
        propertyChanged: static (bindable, _, _) => ((BottomSheetView)bindable).UpdateSheetSize());

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(double),
        typeof(BottomSheetView),
        16d,
        propertyChanged: static (bindable, _, newValue) => ((BottomSheetView)bindable).sheet.StrokeShape = CreateShape((double)newValue));

    public static readonly BindableProperty SheetBackgroundColorProperty = BindableProperty.Create(
        nameof(SheetBackgroundColor),
        typeof(Color),
        typeof(BottomSheetView),
        Colors.White,
        propertyChanged: static (bindable, _, newValue) => ((BottomSheetView)bindable).sheet.BackgroundColor = (Color)newValue);

    private readonly BoxView backdrop;

    private readonly Border sheet;

    private readonly Grid body;

    private readonly ContentView contentHost;

    private double panStart;

    private double panTotal;

    private double panVelocity;

    // 最後に動かした方向 (下 = 1 / 上 = -1)
    private int panDirection;

    private long panTimestamp;

    private bool expanded;

    // 開いているときに始まったドラッグだけを扱う (閉じる途中に触れても動かさない)
    private bool panning;

    // ドラッグから閉じるときの時間 (IsOpen の変更で始まる閉じるアニメーションに渡す)
    private uint? closeDuration;

    // 表示前 (高さ未確定) に開かれたときは、レイアウト後に開く
    private bool pendingOpen;

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public View? SheetContent
    {
        get => (View?)GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    public double HalfExpandedRatio
    {
        get => (double)GetValue(HalfExpandedRatioProperty);
        set => SetValue(HalfExpandedRatioProperty, value);
    }

    public double ExpandedRatio
    {
        get => (double)GetValue(ExpandedRatioProperty);
        set => SetValue(ExpandedRatioProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Color SheetBackgroundColor
    {
        get => (Color)GetValue(SheetBackgroundColorProperty);
        set => SetValue(SheetBackgroundColorProperty, value);
    }

    private double SheetHeight { get; set; }

    private double HalfY => Math.Max(0, SheetHeight - (Height * HalfExpandedRatio));

    public BottomSheetView()
    {
        IsVisible = false;

        backdrop = new BoxView { Color = Colors.Black, Opacity = 0 };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => IsOpen = false;
        backdrop.GestureRecognizers.Add(tap);

        var grabber = new BoxView
        {
            WidthRequest = 40,
            HeightRequest = 4,
            CornerRadius = 2,
            Color = Color.FromArgb("#BDBDBD"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 6)
        };
        contentHost = new ContentView();
        // ジェスチャは Border ではなくレイアウト (Grid) に付ける
        body = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
            BackgroundColor = Colors.Transparent
        };
        body.Add(grabber);
        body.Add(contentHost, 0, 1);
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        body.GestureRecognizers.Add(pan);

        sheet = new Border
        {
            VerticalOptions = LayoutOptions.End,
            BackgroundColor = SheetBackgroundColor,
            StrokeThickness = 0,
            StrokeShape = CreateShape(CornerRadius),
            Padding = 0,
            Content = body
        };

        Children.Add(backdrop);
        Children.Add(sheet);

        SizeChanged += (_, _) => UpdateSheetSize();
    }

    private static RoundRectangle CreateShape(double radius) => new() { CornerRadius = new CornerRadius(radius, radius, 0, 0) };

    // 内容の高さに合わせる (上限は ExpandedRatio)
    private void MeasureSheet()
    {
        SheetHeight = Math.Min(Height * ExpandedRatio, body.Measure(Width, double.PositiveInfinity).Height);
        sheet.HeightRequest = SheetHeight;
    }

    private void UpdateSheetSize()
    {
        if ((Width <= 0) || (Height <= 0))
        {
            return;
        }

        MeasureSheet();
        if (pendingOpen)
        {
            pendingOpen = false;
            sheet.TranslationY = SheetHeight;
            MoveTo(HalfY);
        }
        else if (!sheet.AnimationIsRunning(AnimationName))
        {
            sheet.TranslationY = IsOpen ? (expanded ? 0 : HalfY) : SheetHeight;
        }
    }

    private void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            expanded = false;
            IsVisible = true;
            if ((Width > 0) && (Height > 0))
            {
                MeasureSheet();
                sheet.TranslationY = SheetHeight;
                MoveTo(HalfY);
            }
            else
            {
                pendingOpen = true;
            }
        }
        else
        {
            pendingOpen = false;
            sheet.AbortAnimation(AnimationName);
            var start = sheet.TranslationY;
            var end = SheetHeight;
            var startOpacity = backdrop.Opacity;
            // ボタンや背景のタップからは加速しながら、ドラッグの続きは離したときの速さから減速しながら閉じる
            var fling = closeDuration is not null;
            var duration = closeDuration ?? Duration;
            closeDuration = null;
            sheet.Animate(
                AnimationName,
                v =>
                {
                    sheet.TranslationY = start + ((end - start) * v);
                    backdrop.Opacity = startOpacity * (1 - v);
                },
                16,
                duration,
                fling ? Easing.SinOut : Easing.CubicIn,
                (_, _) => IsVisible = IsOpen);
        }
    }

    private void MoveTo(double y, uint duration = Duration, Easing? easing = null)
    {
        expanded = y < HalfY;
        sheet.AbortAnimation(AnimationName);
        var start = sheet.TranslationY;
        var startOpacity = backdrop.Opacity;
        var endOpacity = BackdropOpacity * (1 - (y / SheetHeight));
        sheet.Animate(
            AnimationName,
            v =>
            {
                sheet.TranslationY = start + ((y - start) * v);
                backdrop.Opacity = startOpacity + ((endOpacity - startOpacity) * v);
            },
            16,
            duration,
            easing ?? Easing.CubicOut);
    }

    // 離したときの速さ (dp/ms) で残りの距離を進む時間。逆方向や停止からは通常の時間
    private static uint FlingDuration(double distance, double velocity) =>
        velocity > 0 ? (uint)Math.Clamp(FlingSlope * distance / velocity, MinFlingDuration, MaxFlingDuration) : Duration;

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panning = IsOpen;
                if (!panning)
                {
                    break;
                }

                sheet.AbortAnimation(AnimationName);
                panStart = sheet.TranslationY;
                panTotal = 0;
                panVelocity = 0;
                panDirection = 0;
                panTimestamp = Environment.TickCount64;
                break;
            case GestureStatus.Running:
                if (!panning)
                {
                    break;
                }

                var delta = e.TotalY - panTotal;
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

                panTotal = e.TotalY;
                var y = Math.Clamp(panStart + e.TotalY, 0, SheetHeight);
                sheet.TranslationY = y;
                backdrop.Opacity = BackdropOpacity * (1 - (y / SheetHeight));
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (panning)
                {
                    panning = false;
                    Settle();
                }

                break;
        }
    }

    // 下方向 (速いか、最後に下へ動かして合計が距離を超えた) は半開より上なら半開へ、それ以外は閉じる。
    // 上方向は半開より下なら半開へ (閉じかけて戻した場合)、それ以外は全開へ。方向が明確でなければ離した位置から最も近い状態へ
    private void Settle()
    {
        var y = sheet.TranslationY;
        var velocity = Environment.TickCount64 - panTimestamp > PauseInterval ? 0 : panVelocity;
        if ((velocity >= SwipeVelocity) || ((velocity >= 0) && (panDirection > 0) && (panTotal >= SwipeDistance)))
        {
            if (y < HalfY - SwipeDistance)
            {
                MoveTo(HalfY, FlingDuration(HalfY - y, velocity), Easing.SinOut);
            }
            else
            {
                closeDuration = FlingDuration(SheetHeight - y, velocity);
                IsOpen = false;
            }

            return;
        }

        if ((velocity <= -SwipeVelocity) || ((velocity <= 0) && (panDirection < 0) && (panTotal <= -SwipeDistance)))
        {
            var target = y > HalfY + SwipeDistance ? HalfY : 0;
            MoveTo(target, FlingDuration(y - target, -velocity), Easing.SinOut);
            return;
        }

        if (y > (HalfY + SheetHeight) / 2)
        {
            IsOpen = false;
            return;
        }

        MoveTo(y < HalfY / 2 ? 0 : HalfY);
    }
}
