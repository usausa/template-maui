namespace Template.MobileApp.Behaviors;

public static partial class LabelOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty AutoSizeProperty = BindableProperty.CreateAttached(
        "AutoSize",
        typeof(bool),
        typeof(LabelOption),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetAutoSize(BindableObject bindable) => (bool)bindable.GetValue(AutoSizeProperty);

    public static void SetAutoSize(BindableObject bindable, bool value) => bindable.SetValue(AutoSizeProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty MaxSizeProperty = BindableProperty.CreateAttached(
        "MaxSize",
        typeof(double),
        typeof(LabelOption),
        144d);
    // ReSharper restore InconsistentNaming

    public static double GetMaxSize(BindableObject bindable) => (double)bindable.GetValue(MaxSizeProperty);

    public static void SetMaxSize(BindableObject bindable, double value) => bindable.SetValue(MaxSizeProperty, value);

    // ------------------------------------------------------------------ CountUp (数値カウントアップ表示)

    private const string CountUpAnimationName = "LabelOptionCountUpAnimation";

    public static readonly BindableProperty CountUpDurationProperty = BindableProperty.CreateAttached(
        "CountUpDuration",
        typeof(int),
        typeof(LabelOption),
        800);

    public static int GetCountUpDuration(BindableObject bindable) => (int)bindable.GetValue(CountUpDurationProperty);

    public static void SetCountUpDuration(BindableObject bindable, int value) => bindable.SetValue(CountUpDurationProperty, value);

    public static readonly BindableProperty CountUpValueProperty = BindableProperty.CreateAttached(
        "CountUpValue",
        typeof(double),
        typeof(LabelOption),
        0d,
        propertyChanged: OnCountUpValueChanged);

    public static double GetCountUpValue(BindableObject bindable) => (double)bindable.GetValue(CountUpValueProperty);

    public static void SetCountUpValue(BindableObject bindable, double value) => bindable.SetValue(CountUpValueProperty, value);

    public static readonly BindableProperty CountUpFormatProperty = BindableProperty.CreateAttached(
        "CountUpFormat",
        typeof(string),
        typeof(LabelOption),
        "{0:N0}");

    public static string GetCountUpFormat(BindableObject bindable) => (string)bindable.GetValue(CountUpFormatProperty);

    public static void SetCountUpFormat(BindableObject bindable, string value) => bindable.SetValue(CountUpFormatProperty, value);

    // 現在表示中の値(アニメーション中の中間値を保持し、次のアニメーションの開始点にする)
    private static readonly BindableProperty CountUpDisplayValueProperty = BindableProperty.CreateAttached(
        "CountUpDisplayValue",
        typeof(double),
        typeof(LabelOption),
        0d);

    private static void OnCountUpValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Label label)
        {
            return;
        }

        if (label.IsLoaded)
        {
            StartCountUp(label, (double)newValue);
        }
        else
        {
            // 表示前に値が確定した場合は Loaded 時に 0 から数え上げる
            label.Loaded -= OnCountUpLabelLoaded;
            label.Loaded += OnCountUpLabelLoaded;
        }
    }

    private static void OnCountUpLabelLoaded(object? sender, EventArgs e)
    {
        if (sender is Label label)
        {
            label.Loaded -= OnCountUpLabelLoaded;
            StartCountUp(label, GetCountUpValue(label));
        }
    }

    private static void StartCountUp(Label label, double to)
    {
        var from = (double)label.GetValue(CountUpDisplayValueProperty);
        var format = GetCountUpFormat(label);

        label.AbortAnimation(CountUpAnimationName);
        label.Animate(
            CountUpAnimationName,
            v =>
            {
                var current = from + ((to - from) * v);
                label.SetValue(CountUpDisplayValueProperty, current);
                label.Text = String.Format(CultureInfo.CurrentCulture, format, current);
            },
            16,
            (uint)GetCountUpDuration(label),
            Easing.CubicOut,
            (_, _) =>
            {
                label.SetValue(CountUpDisplayValueProperty, to);
                label.Text = String.Format(CultureInfo.CurrentCulture, format, to);
            });
    }
}
