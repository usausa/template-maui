namespace Template.MobileApp.Behaviors;

public enum EnterAnimationType
{
    None,
    FadeUp,
    Pop
}

public static class AnimationOption
{
    private const string HoldAnimationName = "AnimationOptionHold";
    private const string PulseAnimationName = "AnimationOptionPulse";
    private const string BounceAnimationName = "AnimationOptionBounce";
    private const string FadeInAnimationName = "AnimationOptionFadeIn";
    private const string HighlightAnimationName = "AnimationOptionHighlight";
    private const string FlashAnimationName = "AnimationOptionFlash";
    private const string WaveAnimationName = "AnimationOptionWave";
    private const string EnterAnimationName = "AnimationOptionEnter";

    // ------------------------------------------------------------------ Pulse

    public static readonly BindableProperty PulseProperty = BindableProperty.CreateAttached(
        "Pulse",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnPulseChanged);

    public static bool GetPulse(BindableObject bindable) => (bool)bindable.GetValue(PulseProperty);

    public static void SetPulse(BindableObject bindable, bool value) => bindable.SetValue(PulseProperty, value);

    private static void OnPulseChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnPulseLoaded;
            element.Unloaded += OnPulseUnloaded;
            if (element.IsLoaded)
            {
                StartPulse(element);
            }
        }
        else
        {
            element.Loaded -= OnPulseLoaded;
            element.Unloaded -= OnPulseUnloaded;
            element.AbortAnimation(PulseAnimationName);
            element.Scale = 1.0;
        }
    }

    private static void OnPulseLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartPulse(element);
        }
    }

    private static void OnPulseUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(PulseAnimationName);
        }
    }

    private static void StartPulse(VisualElement element)
    {
        element.AbortAnimation(PulseAnimationName);

        // 正弦カーブで 1.0 → 1.05 → 1.0 を繰り返す
        element.Animate(
            PulseAnimationName,
            v => element.Scale = 1.0 + (0.05 * Math.Sin(v * Math.PI)),
            16,
            3000,
            Easing.Linear,
            repeat: () => GetPulse(element));
    }

    // ------------------------------------------------------------------ Bounce

    public static readonly BindableProperty BounceTriggerProperty = BindableProperty.CreateAttached(
        "BounceTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnBounceTriggerChanged);

    public static object? GetBounceTrigger(BindableObject bindable) => bindable.GetValue(BounceTriggerProperty);

    public static void SetBounceTrigger(BindableObject bindable, object? value) => bindable.SetValue(BounceTriggerProperty, value);

    public static readonly BindableProperty BounceValueProperty = BindableProperty.CreateAttached(
        "BounceValue",
        typeof(object),
        typeof(AnimationOption),
        null);

    public static object? GetBounceValue(BindableObject bindable) => bindable.GetValue(BounceValueProperty);

    public static void SetBounceValue(BindableObject bindable, object? value) => bindable.SetValue(BounceValueProperty, value);

    private static void OnBounceTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // 初回バインドでは跳ねない
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        // BounceValue 指定時は一致した要素のみ跳ねる
        var expected = GetBounceValue(bindable);
        if ((expected is null) || Equals(newValue, expected))
        {
            StartBounce(element);
        }
    }

    private static void StartBounce(VisualElement element)
    {
        element.AbortAnimation(BounceAnimationName);

        // 正弦カーブで 1.0 → 1.15 → 1.0 と跳ねる
        element.Animate(
            BounceAnimationName,
            v => element.Scale = 1.0 + (0.15 * Math.Sin(v * Math.PI)),
            16,
            300,
            Easing.CubicOut,
            (_, _) => element.Scale = 1.0);
    }

    // ------------------------------------------------------------------ FadeIn

    public static readonly BindableProperty FadeInTriggerProperty = BindableProperty.CreateAttached(
        "FadeInTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnFadeInTriggerChanged);

    public static object? GetFadeInTrigger(BindableObject bindable) => bindable.GetValue(FadeInTriggerProperty);

    public static void SetFadeInTrigger(BindableObject bindable, object? value) => bindable.SetValue(FadeInTriggerProperty, value);

    private static void OnFadeInTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // 初回バインドではフェードしない
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        element.AbortAnimation(FadeInAnimationName);
        element.Opacity = 0;

        element.Animate(
            FadeInAnimationName,
            v => element.Opacity = v,
            16,
            250,
            Easing.CubicOut,
            (_, _) => element.Opacity = 1.0);
    }

    // ------------------------------------------------------------------ Wave

    public static readonly BindableProperty WaveProperty = BindableProperty.CreateAttached(
        "Wave",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnWaveChanged);

    public static bool GetWave(BindableObject bindable) => (bool)bindable.GetValue(WaveProperty);

    public static void SetWave(BindableObject bindable, bool value) => bindable.SetValue(WaveProperty, value);

    public static readonly BindableProperty WaveDelayProperty = BindableProperty.CreateAttached(
        "WaveDelay",
        typeof(int),
        typeof(AnimationOption),
        0);

    public static int GetWaveDelay(BindableObject bindable) => (int)bindable.GetValue(WaveDelayProperty);

    public static void SetWaveDelay(BindableObject bindable, int value) => bindable.SetValue(WaveDelayProperty, value);

    private static void OnWaveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnWaveLoaded;
            element.Unloaded += OnWaveUnloaded;
            if (element.IsLoaded)
            {
                StartWave(element);
            }
        }
        else
        {
            element.Loaded -= OnWaveLoaded;
            element.Unloaded -= OnWaveUnloaded;
            element.AbortAnimation(WaveAnimationName);
            element.TranslationY = 0;
        }
    }

    private static void OnWaveLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            var delay = GetWaveDelay(element);
            if (delay > 0)
            {
                // 要素毎に位相をずらして波を作る
                element.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delay), () =>
                {
                    if (element.IsLoaded && GetWave(element))
                    {
                        StartWave(element);
                    }
                });
            }
            else
            {
                StartWave(element);
            }
        }
    }

    private static void OnWaveUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(WaveAnimationName);
            element.TranslationY = 0;
        }
    }

    private static void StartWave(VisualElement element)
    {
        element.AbortAnimation(WaveAnimationName);

        // 正弦カーブで上下に揺れ続ける
        element.Animate(
            WaveAnimationName,
            v => element.TranslationY = -5.0 * Math.Sin(v * Math.PI),
            16,
            700,
            Easing.Linear,
            repeat: () => GetWave(element));
    }

    // ------------------------------------------------------------------ Flash

    public static readonly BindableProperty FlashTriggerProperty = BindableProperty.CreateAttached(
        "FlashTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnFlashTriggerChanged);

    public static object? GetFlashTrigger(BindableObject bindable) => bindable.GetValue(FlashTriggerProperty);

    public static void SetFlashTrigger(BindableObject bindable, object? value) => bindable.SetValue(FlashTriggerProperty, value);

    private static void OnFlashTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // 初回バインドでは光らせない
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        element.AbortAnimation(FlashAnimationName);
        element.Opacity = 1;

        element.Animate(
            FlashAnimationName,
            v => element.Opacity = 1 - v,
            16,
            300,
            Easing.CubicOut,
            (_, _) => element.Opacity = 0);
    }

    // ------------------------------------------------------------------ Highlight

    public static readonly BindableProperty HighlightTriggerProperty = BindableProperty.CreateAttached(
        "HighlightTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnHighlightTriggerChanged);

    public static object? GetHighlightTrigger(BindableObject bindable) => bindable.GetValue(HighlightTriggerProperty);

    public static void SetHighlightTrigger(BindableObject bindable, object? value) => bindable.SetValue(HighlightTriggerProperty, value);

    public static readonly BindableProperty HighlightColorProperty = BindableProperty.CreateAttached(
        "HighlightColor",
        typeof(Color),
        typeof(AnimationOption),
        null);

    public static Color? GetHighlightColor(BindableObject bindable) => (Color?)bindable.GetValue(HighlightColorProperty);

    public static void SetHighlightColor(BindableObject bindable, Color? value) => bindable.SetValue(HighlightColorProperty, value);

    private static void OnHighlightTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // 初回バインドではハイライトしない
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        var highlight = GetHighlightColor(bindable);
        if (highlight is null)
        {
            return;
        }

        var original = element.BackgroundColor ?? Colors.Transparent;

        element.AbortAnimation(HighlightAnimationName);
        element.Animate(
            HighlightAnimationName,
            v => element.BackgroundColor = LerpColor(highlight, original, v),
            16,
            600,
            Easing.CubicOut,
            (_, _) => element.BackgroundColor = original);
    }

    // ------------------------------------------------------------------ ProgressTo (ProgressBar の伸長アニメーション)

    public static readonly BindableProperty ProgressToProperty = BindableProperty.CreateAttached(
        "ProgressTo",
        typeof(double),
        typeof(AnimationOption),
        0d,
        propertyChanged: OnProgressToChanged);

    public static double GetProgressTo(BindableObject bindable) => (double)bindable.GetValue(ProgressToProperty);

    public static void SetProgressTo(BindableObject bindable, double value) => bindable.SetValue(ProgressToProperty, value);

    private static void OnProgressToChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ProgressBar bar)
        {
            return;
        }

        if (bar.IsLoaded)
        {
            _ = bar.ProgressTo((double)newValue, 800, Easing.CubicOut);
        }
        else
        {
            // 表示前に値が確定した場合は Loaded 時に 0 から伸ばす
            bar.Loaded -= OnProgressBarLoaded;
            bar.Loaded += OnProgressBarLoaded;
        }
    }

    private static void OnProgressBarLoaded(object? sender, EventArgs e)
    {
        if (sender is ProgressBar bar)
        {
            bar.Loaded -= OnProgressBarLoaded;
            _ = bar.ProgressTo(GetProgressTo(bar), 800, Easing.CubicOut);
        }
    }

    // ------------------------------------------------------------------ Hold (長押しで 0-1 を進める)

    public static readonly BindableProperty HoldCommandProperty = BindableProperty.CreateAttached(
        "HoldCommand",
        typeof(ICommand),
        typeof(AnimationOption),
        null,
        propertyChanged: OnHoldCommandChanged);

    public static ICommand? GetHoldCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(HoldCommandProperty);

    public static void SetHoldCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(HoldCommandProperty, value);

    public static readonly BindableProperty HoldDurationProperty = BindableProperty.CreateAttached(
        "HoldDuration",
        typeof(int),
        typeof(AnimationOption),
        2000);

    public static int GetHoldDuration(BindableObject bindable) => (int)bindable.GetValue(HoldDurationProperty);

    public static void SetHoldDuration(BindableObject bindable, int value) => bindable.SetValue(HoldDurationProperty, value);

    // 進行中の値 (0-1)。途中で離したときの巻き戻しに使う
    private static readonly BindableProperty HoldProgressProperty = BindableProperty.CreateAttached(
        "HoldProgress",
        typeof(double),
        typeof(AnimationOption),
        0d);

    private static void OnHoldCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not Button button)
        {
            return;
        }

        if (oldValue is not null)
        {
            button.Pressed -= OnHoldPressed;
            button.Released -= OnHoldReleased;
        }
        if (newValue is not null)
        {
            button.Pressed += OnHoldPressed;
            button.Released += OnHoldReleased;
        }
    }

    private static void OnHoldPressed(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        button.AbortAnimation(HoldAnimationName);

        // 完走済みなら先頭から、途中なら続きから進める
        var progress = (double)button.GetValue(HoldProgressProperty);
        if (progress >= 1d)
        {
            progress = 0d;
        }

        var length = (uint)Math.Max(1, (int)((1d - progress) * GetHoldDuration(button)));
        button.Animate(
            HoldAnimationName,
            v =>
            {
                button.SetValue(HoldProgressProperty, v);
                ExecuteHold(button, v);
            },
            progress,
            1d,
            16,
            length,
            Easing.Linear);
    }

    private static void OnHoldReleased(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        button.AbortAnimation(HoldAnimationName);

        // 完走前に離したら素早く巻き戻す
        var progress = (double)button.GetValue(HoldProgressProperty);
        if (progress is <= 0d or >= 1d)
        {
            return;
        }

        button.Animate(
            HoldAnimationName,
            v =>
            {
                button.SetValue(HoldProgressProperty, v);
                ExecuteHold(button, v);
            },
            progress,
            0d,
            16,
            (uint)Math.Max(1, (int)(progress * 250)),
            Easing.CubicOut);
    }

    private static void ExecuteHold(BindableObject bindable, double value)
    {
        var command = GetHoldCommand(bindable);
        if ((command is not null) && command.CanExecute(value))
        {
            command.Execute(value);
        }
    }

    // ------------------------------------------------------------------ Enter

    public static readonly BindableProperty EnterAnimationProperty = BindableProperty.CreateAttached(
        "EnterAnimation",
        typeof(EnterAnimationType),
        typeof(AnimationOption),
        EnterAnimationType.None,
        propertyChanged: OnEnterAnimationChanged);

    public static EnterAnimationType GetEnterAnimation(BindableObject bindable) => (EnterAnimationType)bindable.GetValue(EnterAnimationProperty);

    public static void SetEnterAnimation(BindableObject bindable, EnterAnimationType value) => bindable.SetValue(EnterAnimationProperty, value);

    public static readonly BindableProperty EnterDelayProperty = BindableProperty.CreateAttached(
        "EnterDelay",
        typeof(int),
        typeof(AnimationOption),
        0);

    public static int GetEnterDelay(BindableObject bindable) => (int)bindable.GetValue(EnterDelayProperty);

    public static void SetEnterDelay(BindableObject bindable, int value) => bindable.SetValue(EnterDelayProperty, value);

    public static readonly BindableProperty EnterTriggerProperty = BindableProperty.CreateAttached(
        "EnterTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnEnterTriggerChanged);

    public static object? GetEnterTrigger(BindableObject bindable) => bindable.GetValue(EnterTriggerProperty);

    public static void SetEnterTrigger(BindableObject bindable, object? value) => bindable.SetValue(EnterTriggerProperty, value);

    // 演出で TranslationY を動かすため、要素が元々持っていた値(Style 等の静的オフセット)を退避しておく
    private static readonly BindableProperty EnterBaseTranslationYProperty = BindableProperty.CreateAttached(
        "EnterBaseTranslationY",
        typeof(double?),
        typeof(AnimationOption),
        null);

    // 初回呼び出し時のみ現在値を基準として記憶する(2回目以降は演出中の値を拾わないようにする)
    private static double GetEnterBaseTranslationY(VisualElement element)
    {
        if (element.GetValue(EnterBaseTranslationYProperty) is double baseTranslationY)
        {
            return baseTranslationY;
        }

        baseTranslationY = element.TranslationY;
        element.SetValue(EnterBaseTranslationYProperty, baseTranslationY);
        return baseTranslationY;
    }

    private static void OnEnterAnimationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((EnterAnimationType)newValue != EnterAnimationType.None)
        {
            element.Loaded += OnEnterLoaded;
            element.Unloaded += OnEnterUnloaded;
            if (element.IsLoaded)
            {
                PrepareEnter(element);
                StartEnterDelayed(element);
            }
        }
        else
        {
            element.Loaded -= OnEnterLoaded;
            element.Unloaded -= OnEnterUnloaded;
            element.AbortAnimation(EnterAnimationName);
            ResetEnter(element);
        }
    }

    private static void OnEnterTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        // 初回バインドでは再生しない(表示時の再生は Loaded 側が担う)
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        if ((GetEnterAnimation(element) == EnterAnimationType.None) || !element.IsLoaded)
        {
            return;
        }

        PrepareEnter(element);
        StartEnterDelayed(element);
    }

    private static void OnEnterLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            // 遅延中に見えてしまわないよう先に初期状態へ
            PrepareEnter(element);
            StartEnterDelayed(element);
        }
    }

    private static void OnEnterUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(EnterAnimationName);
            ResetEnter(element);
        }
    }

    private static void PrepareEnter(VisualElement element)
    {
        element.AbortAnimation(EnterAnimationName);

        if (GetEnterAnimation(element) == EnterAnimationType.FadeUp)
        {
            element.Opacity = 0;
            element.TranslationY = GetEnterBaseTranslationY(element) + 16;
        }
        else
        {
            // Pop は TranslationY を使わないが、基準値の取得漏れを防ぐためここで確定させる
            GetEnterBaseTranslationY(element);
            element.Opacity = 0;
            element.Scale = 0;
        }
    }

    private static void StartEnterDelayed(VisualElement element)
    {
        var delay = GetEnterDelay(element);
        if (delay > 0)
        {
            // 要素毎に開始をずらして時間差表示を作る
            element.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delay), () =>
            {
                if (element.IsLoaded && (GetEnterAnimation(element) != EnterAnimationType.None))
                {
                    StartEnter(element);
                }
            });
        }
        else
        {
            StartEnter(element);
        }
    }

    private static void StartEnter(VisualElement element)
    {
        element.AbortAnimation(EnterAnimationName);

        if (GetEnterAnimation(element) == EnterAnimationType.FadeUp)
        {
            // 下から浮き上がりながらフェードイン
            var baseTranslationY = GetEnterBaseTranslationY(element);
            element.Animate(
                EnterAnimationName,
                v =>
                {
                    element.Opacity = v;
                    element.TranslationY = baseTranslationY + (16 * (1 - v));
                },
                16,
                250,
                Easing.CubicOut,
                (_, _) => ResetEnter(element));
        }
        else
        {
            // 0 → 1.15 → 1.0 と弾んで出現
            element.Animate(
                EnterAnimationName,
                v =>
                {
                    element.Opacity = Math.Min(1.0, v * 2.0);
                    element.Scale = v < 0.7 ? 1.15 * (v / 0.7) : 1.15 - (0.15 * ((v - 0.7) / 0.3));
                },
                16,
                300,
                Easing.CubicOut,
                (_, _) => ResetEnter(element));
        }
    }

    private static void ResetEnter(VisualElement element)
    {
        element.Opacity = 1;
        // 0 固定にすると Style 等で指定された静的な TranslationY を壊すため、退避した基準値へ戻す
        element.TranslationY = GetEnterBaseTranslationY(element);
        element.Scale = 1;
    }

    private static Color LerpColor(Color from, Color to, double t) =>
        new(
            (float)(from.Red + ((to.Red - from.Red) * t)),
            (float)(from.Green + ((to.Green - from.Green) * t)),
            (float)(from.Blue + ((to.Blue - from.Blue) * t)),
            (float)(from.Alpha + ((to.Alpha - from.Alpha) * t)));
}
