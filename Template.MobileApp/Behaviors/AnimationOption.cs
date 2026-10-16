namespace Template.MobileApp.Behaviors;

public enum EnterAnimationType
{
    None,
    FadeUp,
    Pop,
    Grow
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
    private const string RippleAnimationName = "AnimationOptionRipple";
    private const string SpinAnimationName = "AnimationOptionSpin";
    private const string BlinkAnimationName = "AnimationOptionBlink";
    private const string HeartbeatAnimationName = "AnimationOptionHeartbeat";
    private const string JumpAnimationName = "AnimationOptionJump";
    private const string FloatAnimationName = "AnimationOptionFloat";
    private const string EnterAnimationName = "AnimationOptionEnter";

    //--------------------------------------------------------------------------------
    // Pulse
    //--------------------------------------------------------------------------------

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

        element.Animate(
            PulseAnimationName,
            v => element.Scale = 1.0 + (0.05 * Math.Sin(v * Math.PI)),
            16,
            3000,
            Easing.Linear,
            repeat: () => GetPulse(element));
    }

    //--------------------------------------------------------------------------------
    // Bounce
    //--------------------------------------------------------------------------------

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
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        var expected = GetBounceValue(bindable);
        if ((expected is null) || Equals(newValue, expected))
        {
            StartBounce(element);
        }
    }

    private static void StartBounce(VisualElement element)
    {
        element.AbortAnimation(BounceAnimationName);

        element.Animate(
            BounceAnimationName,
            v => element.Scale = 1.0 + (0.15 * Math.Sin(v * Math.PI)),
            16,
            300,
            Easing.CubicOut,
            (_, _) => element.Scale = 1.0);
    }

    //--------------------------------------------------------------------------------
    // FadeIn
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Wage
    //--------------------------------------------------------------------------------

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

        element.Animate(
            WaveAnimationName,
            v => element.TranslationY = -5.0 * Math.Sin(v * Math.PI),
            16,
            700,
            Easing.Linear,
            repeat: () => GetWave(element));
    }

    //--------------------------------------------------------------------------------
    // Ripple
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty RippleProperty = BindableProperty.CreateAttached(
        "Ripple",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnRippleChanged);

    public static bool GetRipple(BindableObject bindable) => (bool)bindable.GetValue(RippleProperty);

    public static void SetRipple(BindableObject bindable, bool value) => bindable.SetValue(RippleProperty, value);

    public static readonly BindableProperty RippleDelayProperty = BindableProperty.CreateAttached(
        "RippleDelay",
        typeof(int),
        typeof(AnimationOption),
        0);

    public static int GetRippleDelay(BindableObject bindable) => (int)bindable.GetValue(RippleDelayProperty);

    public static void SetRippleDelay(BindableObject bindable, int value) => bindable.SetValue(RippleDelayProperty, value);

    private static void OnRippleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnRippleLoaded;
            element.Unloaded += OnRippleUnloaded;
            if (element.IsLoaded)
            {
                StartRippleAfterDelay(element);
            }
        }
        else
        {
            element.Loaded -= OnRippleLoaded;
            element.Unloaded -= OnRippleUnloaded;
            StopRipple(element);
        }
    }

    private static void OnRippleLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartRippleAfterDelay(element);
        }
    }

    private static void OnRippleUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StopRipple(element);
        }
    }

    private static void StartRippleAfterDelay(VisualElement element)
    {
        var delay = GetRippleDelay(element);
        if (delay > 0)
        {
            element.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delay), () =>
            {
                if (element.IsLoaded && GetRipple(element))
                {
                    StartRipple(element);
                }
            });
        }
        else
        {
            StartRipple(element);
        }
    }

    private static void StartRipple(VisualElement element)
    {
        element.AbortAnimation(RippleAnimationName);

        element.Animate(
            RippleAnimationName,
            v =>
            {
                element.Scale = 1 + (0.5 * v);
                element.Opacity = 0.8 * (1 - v);
            },
            16,
            1800,
            Easing.CubicOut,
            repeat: () => GetRipple(element));
    }

    private static void StopRipple(VisualElement element)
    {
        element.AbortAnimation(RippleAnimationName);
        element.Scale = 1;
        element.Opacity = 0;
    }

    //--------------------------------------------------------------------------------
    // Spin
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SpinProperty = BindableProperty.CreateAttached(
        "Spin",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnSpinChanged);

    public static bool GetSpin(BindableObject bindable) => (bool)bindable.GetValue(SpinProperty);

    public static void SetSpin(BindableObject bindable, bool value) => bindable.SetValue(SpinProperty, value);

    private static void OnSpinChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnSpinLoaded;
            element.Unloaded += OnSpinUnloaded;
            if (element.IsLoaded)
            {
                StartSpin(element);
            }
        }
        else
        {
            element.Loaded -= OnSpinLoaded;
            element.Unloaded -= OnSpinUnloaded;
            element.AbortAnimation(SpinAnimationName);
            element.Rotation = 0;
        }
    }

    private static void OnSpinLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartSpin(element);
        }
    }

    private static void OnSpinUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(SpinAnimationName);
            element.Rotation = 0;
        }
    }

    private static void StartSpin(VisualElement element)
    {
        element.AbortAnimation(SpinAnimationName);

        element.Animate(
            SpinAnimationName,
            v => element.Rotation = 360 * v,
            16,
            1200,
            Easing.Linear,
            repeat: () => GetSpin(element));
    }

    //--------------------------------------------------------------------------------
    // Blink
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty BlinkProperty = BindableProperty.CreateAttached(
        "Blink",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnBlinkChanged);

    public static bool GetBlink(BindableObject bindable) => (bool)bindable.GetValue(BlinkProperty);

    public static void SetBlink(BindableObject bindable, bool value) => bindable.SetValue(BlinkProperty, value);

    private static void OnBlinkChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnBlinkLoaded;
            element.Unloaded += OnBlinkUnloaded;
            if (element.IsLoaded)
            {
                StartBlink(element);
            }
        }
        else
        {
            element.Loaded -= OnBlinkLoaded;
            element.Unloaded -= OnBlinkUnloaded;
            element.AbortAnimation(BlinkAnimationName);
            element.Opacity = 1;
        }
    }

    private static void OnBlinkLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartBlink(element);
        }
    }

    private static void OnBlinkUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(BlinkAnimationName);
            element.Opacity = 1;
        }
    }

    private static void StartBlink(VisualElement element)
    {
        element.AbortAnimation(BlinkAnimationName);

        element.Animate(
            BlinkAnimationName,
            v => element.Opacity = 0.65 + (0.35 * Math.Cos(v * 2 * Math.PI)),
            16,
            1000,
            Easing.Linear,
            repeat: () => GetBlink(element));
    }

    //--------------------------------------------------------------------------------
    // Heartbeat
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty HeartbeatProperty = BindableProperty.CreateAttached(
        "Heartbeat",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnHeartbeatChanged);

    public static bool GetHeartbeat(BindableObject bindable) => (bool)bindable.GetValue(HeartbeatProperty);

    public static void SetHeartbeat(BindableObject bindable, bool value) => bindable.SetValue(HeartbeatProperty, value);

    public static readonly BindableProperty HeartbeatRateProperty = BindableProperty.CreateAttached(
        "HeartbeatRate",
        typeof(int),
        typeof(AnimationOption),
        72,
        propertyChanged: OnHeartbeatRateChanged);

    public static int GetHeartbeatRate(BindableObject bindable) => (int)bindable.GetValue(HeartbeatRateProperty);

    public static void SetHeartbeatRate(BindableObject bindable, int value) => bindable.SetValue(HeartbeatRateProperty, value);

    public static readonly BindableProperty HeartbeatTriggerProperty = BindableProperty.CreateAttached(
        "HeartbeatTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnHeartbeatTriggerChanged);

    public static object? GetHeartbeatTrigger(BindableObject bindable) => bindable.GetValue(HeartbeatTriggerProperty);

    public static void SetHeartbeatTrigger(BindableObject bindable, object? value) => bindable.SetValue(HeartbeatTriggerProperty, value);

    public static readonly BindableProperty HeartbeatValueProperty = BindableProperty.CreateAttached(
        "HeartbeatValue",
        typeof(object),
        typeof(AnimationOption),
        null);

    public static object? GetHeartbeatValue(BindableObject bindable) => bindable.GetValue(HeartbeatValueProperty);

    public static void SetHeartbeatValue(BindableObject bindable, object? value) => bindable.SetValue(HeartbeatValueProperty, value);

    private static void OnHeartbeatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnHeartbeatLoaded;
            element.Unloaded += OnHeartbeatUnloaded;
            if (element.IsLoaded)
            {
                StartHeartbeat(element);
            }
        }
        else
        {
            element.Loaded -= OnHeartbeatLoaded;
            element.Unloaded -= OnHeartbeatUnloaded;
            element.AbortAnimation(HeartbeatAnimationName);
            element.Scale = 1.0;
        }
    }

    private static void OnHeartbeatRateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if ((bindable is VisualElement element) && GetHeartbeat(element) && element.IsLoaded)
        {
            StartHeartbeat(element);
        }
    }

    private static void OnHeartbeatLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartHeartbeat(element);
        }
    }

    private static void OnHeartbeatUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(HeartbeatAnimationName);
            element.Scale = 1.0;
        }
    }

    private static void StartHeartbeat(VisualElement element)
    {
        element.AbortAnimation(HeartbeatAnimationName);

        element.Animate(
            HeartbeatAnimationName,
            v => element.Scale = v < 0.5 ? BeatScale(v / 0.5) : 1.0,
            16,
            (uint)(60000 / Math.Max(GetHeartbeatRate(element), 1)),
            Easing.Linear,
            repeat: () => GetHeartbeat(element));
    }

    private static void OnHeartbeatTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        var target = GetHeartbeatValue(element);
        if ((target is not null) && !Equals(target, newValue))
        {
            return;
        }

        element.AbortAnimation(HeartbeatAnimationName);
        element.Animate(
            HeartbeatAnimationName,
            v => element.Scale = BeatScale(v),
            16,
            450,
            Easing.Linear,
            (_, _) => element.Scale = 1.0);
    }

    private static double BeatScale(double v) =>
        v < 0.5 ? 1.0 + (0.18 * Math.Sin(v / 0.5 * Math.PI)) : 1.0 + (0.1 * Math.Sin((v - 0.5) / 0.5 * Math.PI));

    //--------------------------------------------------------------------------------
    // Jump
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty JumpTriggerProperty = BindableProperty.CreateAttached(
        "JumpTrigger",
        typeof(object),
        typeof(AnimationOption),
        null,
        propertyChanged: OnJumpTriggerChanged);

    public static object? GetJumpTrigger(BindableObject bindable) => bindable.GetValue(JumpTriggerProperty);

    public static void SetJumpTrigger(BindableObject bindable, object? value) => bindable.SetValue(JumpTriggerProperty, value);

    public static readonly BindableProperty JumpValueProperty = BindableProperty.CreateAttached(
        "JumpValue",
        typeof(object),
        typeof(AnimationOption),
        null);

    public static object? GetJumpValue(BindableObject bindable) => bindable.GetValue(JumpValueProperty);

    public static void SetJumpValue(BindableObject bindable, object? value) => bindable.SetValue(JumpValueProperty, value);

    private static void OnJumpTriggerChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if ((bindable is not VisualElement element) || (oldValue is null) || (newValue is null))
        {
            return;
        }

        var target = GetJumpValue(element);
        if ((target is not null) && !Equals(target, newValue))
        {
            return;
        }

        element.AbortAnimation(JumpAnimationName);
        var baseTranslationY = element.TranslationY;
        var baseAnchorY = element.AnchorY;
        element.AnchorY = 1;
        element.Animate(
            JumpAnimationName,
            v =>
            {
                if (v < 0.7)
                {
                    var p = v / 0.7;
                    element.TranslationY = baseTranslationY - (56 * p * (1 - p));
                }
                else
                {
                    var s = Math.Sin((v - 0.7) / 0.3 * Math.PI);
                    element.TranslationY = baseTranslationY;
                    element.ScaleY = 1 - (0.18 * s);
                    element.ScaleX = 1 + (0.1 * s);
                }
            },
            16,
            520,
            Easing.Linear,
            (_, _) =>
            {
                element.TranslationY = baseTranslationY;
                element.ScaleX = 1;
                element.ScaleY = 1;
                element.AnchorY = baseAnchorY;
            });
    }

    //--------------------------------------------------------------------------------
    // Float
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty FloatProperty = BindableProperty.CreateAttached(
        "Float",
        typeof(bool),
        typeof(AnimationOption),
        false,
        propertyChanged: OnFloatChanged);

    public static bool GetFloat(BindableObject bindable) => (bool)bindable.GetValue(FloatProperty);

    public static void SetFloat(BindableObject bindable, bool value) => bindable.SetValue(FloatProperty, value);

    private static void OnFloatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element)
        {
            return;
        }

        if ((bool)newValue)
        {
            element.Loaded += OnFloatLoaded;
            element.Unloaded += OnFloatUnloaded;
            if (element.IsLoaded)
            {
                StartFloat(element);
            }
        }
        else
        {
            element.Loaded -= OnFloatLoaded;
            element.Unloaded -= OnFloatUnloaded;
            element.AbortAnimation(FloatAnimationName);
            element.TranslationY = 0;
        }
    }

    private static void OnFloatLoaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            StartFloat(element);
        }
    }

    private static void OnFloatUnloaded(object? sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            element.AbortAnimation(FloatAnimationName);
            element.TranslationY = 0;
        }
    }

    private static void StartFloat(VisualElement element)
    {
        element.AbortAnimation(FloatAnimationName);

        element.Animate(
            FloatAnimationName,
            v => element.TranslationY = -3 * (1 - Math.Cos(v * 2 * Math.PI)),
            16,
            3000,
            Easing.Linear,
            repeat: () => GetFloat(element));
    }

    //--------------------------------------------------------------------------------
    // Flash
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Highlight
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // ProgressTo
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Hold
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Enter
    //--------------------------------------------------------------------------------

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

    private static readonly BindableProperty EnterBaseTranslationYProperty = BindableProperty.CreateAttached(
        "EnterBaseTranslationY",
        typeof(double?),
        typeof(AnimationOption),
        null);

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
        else if (GetEnterAnimation(element) == EnterAnimationType.Grow)
        {
            GetEnterBaseTranslationY(element);
            element.AnchorY = 0;
            element.ScaleY = 0;
        }
        else
        {
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
        else if (GetEnterAnimation(element) == EnterAnimationType.Grow)
        {
            element.Animate(
                EnterAnimationName,
                v => element.ScaleY = v,
                16,
                400,
                Easing.CubicOut,
                (_, _) => ResetEnter(element));
        }
        else
        {
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
        element.TranslationY = GetEnterBaseTranslationY(element);
        element.Scale = 1;
        element.ScaleY = 1;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static Color LerpColor(Color from, Color to, double t) =>
        new((float)(from.Red + ((to.Red - from.Red) * t)), (float)(from.Green + ((to.Green - from.Green) * t)), (float)(from.Blue + ((to.Blue - from.Blue) * t)), (float)(from.Alpha + ((to.Alpha - from.Alpha) * t)));
}
