namespace Template.MobileApp.Behaviors;

public static partial class CarouselOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    public static readonly BindableProperty PageSnapProperty = BindableProperty.CreateAttached(
        "PageSnap",
        typeof(bool),
        typeof(CarouselOption),
        false);

    public static bool GetPageSnap(BindableObject bindable) => (bool)bindable.GetValue(PageSnapProperty);

    public static void SetPageSnap(BindableObject bindable, bool value) => bindable.SetValue(PageSnapProperty, value);

    // PageSnap のときのスクロールの位置 (ページの単位。2.5 は 2 ページ目と 3 ページ目の真ん中)
    public static readonly BindableProperty PagePositionProperty = BindableProperty.CreateAttached(
        "PagePosition",
        typeof(double),
        typeof(CarouselOption),
        0d,
        BindingMode.OneWayToSource);

    public static double GetPagePosition(BindableObject bindable) => (double)bindable.GetValue(PagePositionProperty);

    public static void SetPagePosition(BindableObject bindable, double value) => bindable.SetValue(PagePositionProperty, value);

    public static readonly BindableProperty CacheSizeProperty = BindableProperty.CreateAttached(
        "CacheSize",
        typeof(int),
        typeof(CarouselOption),
        -1);

    public static int GetCacheSize(BindableObject bindable) => (int)bindable.GetValue(CacheSizeProperty);

    public static void SetCacheSize(BindableObject bindable, int value) => bindable.SetValue(CacheSizeProperty, value);
}
