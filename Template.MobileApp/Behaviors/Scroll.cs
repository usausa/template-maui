namespace Template.MobileApp.Behaviors;

using System.Collections;

public static partial class Scroll
{
    // ------------------------------------------------------------------ DisableOverScroll

    public static readonly BindableProperty DisableOverScrollProperty = BindableProperty.CreateAttached(
        "DisableOverScroll",
        typeof(bool),
        typeof(Scroll),
        true);

    public static bool GetDisableOverScroll(BindableObject bindable) => (bool)bindable.GetValue(DisableOverScrollProperty);

    public static void SetDisableOverScroll(BindableObject bindable, bool value) => bindable.SetValue(DisableOverScrollProperty, value);

    public static partial void UseCustomMapper(BehaviorOptions options);

    // ------------------------------------------------------------------ HideOnScrollTarget

    public static readonly BindableProperty HideOnScrollTargetProperty = BindableProperty.CreateAttached(
        "HideOnScrollTarget",
        typeof(VisualElement),
        typeof(Scroll),
        null,
        propertyChanged: OnHideOnScrollTargetChanged);

    public static VisualElement? GetHideOnScrollTarget(BindableObject bindable) => (VisualElement?)bindable.GetValue(HideOnScrollTargetProperty);

    public static void SetHideOnScrollTarget(BindableObject bindable, VisualElement? value) => bindable.SetValue(HideOnScrollTargetProperty, value);

    private static void OnHideOnScrollTargetChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ItemsView itemsView)
        {
            return;
        }

        if (oldValue is not null)
        {
            itemsView.Scrolled -= OnScrolled;
        }
        if (newValue is not null)
        {
            itemsView.Scrolled += OnScrolled;
        }
    }

    private static void OnScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (sender is not ItemsView itemsView)
        {
            return;
        }

        var target = GetHideOnScrollTarget(itemsView);
        if (target is null)
        {
            return;
        }

        // 下方向スクロールで隠し、上方向で戻す
        if ((e.VerticalDelta > 8) && (target.Scale > 0.5))
        {
            _ = target.ScaleToAsync(0, 200, Easing.CubicIn);
        }
        else if ((e.VerticalDelta < -8) && (target.Scale < 0.5))
        {
            _ = target.ScaleToAsync(1, 200, Easing.CubicOut);
        }
    }

    // ------------------------------------------------------------------ ParallaxTarget

    public static readonly BindableProperty ParallaxTargetProperty = BindableProperty.CreateAttached(
        "ParallaxTarget",
        typeof(VisualElement),
        typeof(Scroll),
        null,
        propertyChanged: OnParallaxTargetChanged);

    public static VisualElement? GetParallaxTarget(BindableObject bindable) => (VisualElement?)bindable.GetValue(ParallaxTargetProperty);

    public static void SetParallaxTarget(BindableObject bindable, VisualElement? value) => bindable.SetValue(ParallaxTargetProperty, value);

    private static void OnParallaxTargetChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ScrollView scrollView)
        {
            return;
        }

        if (oldValue is not null)
        {
            scrollView.Scrolled -= OnParallaxScrolled;
        }
        if (newValue is not null)
        {
            scrollView.Scrolled += OnParallaxScrolled;
        }
    }

    private static void OnParallaxScrolled(object? sender, ScrolledEventArgs e)
    {
        if (sender is not ScrollView scrollView)
        {
            return;
        }

        var target = GetParallaxTarget(scrollView);

        // スクロールの半分の速度で追従させて奥行きを出す
        target?.TranslationY = Math.Max(0, e.ScrollY) * 0.5;
    }

    // ------------------------------------------------------------------ RatioCommand

    // スクロール位置を 0-1 の比率へ正規化してコマンドに渡す (スクロール連動アニメーション用)
    public static readonly BindableProperty RatioCommandProperty = BindableProperty.CreateAttached(
        "RatioCommand",
        typeof(ICommand),
        typeof(Scroll),
        null,
        propertyChanged: OnRatioCommandChanged);

    public static ICommand? GetRatioCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(RatioCommandProperty);

    public static void SetRatioCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(RatioCommandProperty, value);

    private static void OnRatioCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ScrollView scrollView)
        {
            return;
        }

        if (oldValue is not null)
        {
            scrollView.Scrolled -= OnRatioScrolled;
        }
        if (newValue is not null)
        {
            scrollView.Scrolled += OnRatioScrolled;
        }
    }

    private static void OnRatioScrolled(object? sender, ScrolledEventArgs e)
    {
        if (sender is not ScrollView scrollView)
        {
            return;
        }

        var command = GetRatioCommand(scrollView);
        if (command is null)
        {
            return;
        }

        // スクロール可能量の大きい方の軸を比率化する
        var extentX = scrollView.ContentSize.Width - scrollView.Width;
        var extentY = scrollView.ContentSize.Height - scrollView.Height;
        var ratio = extentX > extentY
            ? (extentX > 0 ? e.ScrollX / extentX : 0d)
            : (extentY > 0 ? e.ScrollY / extentY : 0d);
        ratio = Math.Clamp(ratio, 0d, 1d);

        if (command.CanExecute(ratio))
        {
            command.Execute(ratio);
        }
    }

    // ------------------------------------------------------------------ ShowOnAwayFromLastTarget

    public static readonly BindableProperty ShowOnAwayFromLastTargetProperty = BindableProperty.CreateAttached(
        "ShowOnAwayFromLastTarget",
        typeof(VisualElement),
        typeof(Scroll),
        null,
        propertyChanged: OnShowOnAwayFromLastTargetChanged);

    public static VisualElement? GetShowOnAwayFromLastTarget(BindableObject bindable) => (VisualElement?)bindable.GetValue(ShowOnAwayFromLastTargetProperty);

    public static void SetShowOnAwayFromLastTarget(BindableObject bindable, VisualElement? value) => bindable.SetValue(ShowOnAwayFromLastTargetProperty, value);

    private static void OnShowOnAwayFromLastTargetChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ItemsView itemsView)
        {
            return;
        }

        if (oldValue is not null)
        {
            itemsView.Scrolled -= OnScrolledAwayFromLast;
        }
        if (newValue is not null)
        {
            itemsView.Scrolled += OnScrolledAwayFromLast;
        }
    }

    private static void OnScrolledAwayFromLast(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (sender is not ItemsView itemsView)
        {
            return;
        }

        var target = GetShowOnAwayFromLastTarget(itemsView);
        if ((target is null) || (itemsView.ItemsSource is not ICollection collection))
        {
            return;
        }

        // 末尾の 1 つ手前より上を表示しているときに出す
        var away = e.LastVisibleItemIndex < collection.Count - 2;
        if (away && (target.Scale < 0.5))
        {
            _ = target.ScaleToAsync(1, 200, Easing.CubicOut);
        }
        else if (!away && (target.Scale > 0.5))
        {
            _ = target.ScaleToAsync(0, 200, Easing.CubicIn);
        }
    }
}
