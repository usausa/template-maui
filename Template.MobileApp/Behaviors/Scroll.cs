namespace Template.MobileApp.Behaviors;

using System.Collections;
using System.Runtime.CompilerServices;

public static partial class Scroll
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    //--------------------------------------------------------------------------------
    // DisableOverScroll
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty DisableOverScrollProperty = BindableProperty.CreateAttached(
        "DisableOverScroll",
        typeof(bool),
        typeof(Scroll),
        true);

    public static bool GetDisableOverScroll(BindableObject bindable) => (bool)bindable.GetValue(DisableOverScrollProperty);

    public static void SetDisableOverScroll(BindableObject bindable, bool value) => bindable.SetValue(DisableOverScrollProperty, value);

    //--------------------------------------------------------------------------------
    // HideOnScrollTarget
    //--------------------------------------------------------------------------------

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

        if ((e.VerticalDelta > 8) && (target.Scale > 0.5))
        {
            _ = target.ScaleToAsync(0, 200, Easing.CubicIn);
        }
        else if ((e.VerticalDelta < -8) && (target.Scale < 0.5))
        {
            _ = target.ScaleToAsync(1, 200, Easing.CubicOut);
        }
    }

    //--------------------------------------------------------------------------------
    // ParallaxTarget
    //--------------------------------------------------------------------------------

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

        target?.TranslationY = Math.Max(0, e.ScrollY) * 0.5;
    }

    //--------------------------------------------------------------------------------
    // RatioCommand
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // IndexCommand
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty IndexCommandProperty = BindableProperty.CreateAttached(
        "IndexCommand",
        typeof(ICommand),
        typeof(Scroll),
        null,
        propertyChanged: OnIndexCommandChanged);

    public static ICommand? GetIndexCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(IndexCommandProperty);

    public static void SetIndexCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(IndexCommandProperty, value);

    private static void OnIndexCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ScrollView scrollView)
        {
            return;
        }

        if (oldValue is not null)
        {
            scrollView.Scrolled -= OnIndexScrolled;
        }
        if (newValue is not null)
        {
            scrollView.Scrolled += OnIndexScrolled;
        }
    }

    private static void OnIndexScrolled(object? sender, ScrolledEventArgs e)
    {
        if ((sender is not ScrollView { Content: Layout { Count: > 0 } layout } scrollView) ||
            (GetIndexCommand(scrollView) is not { } command))
        {
            return;
        }

        var index = layout.Count - 1;
        for (var i = 0; i < layout.Count; i++)
        {
            if ((layout[i] is VisualElement child) && ((child.Y + child.Height) > e.ScrollY))
            {
                index = i;
                break;
            }
        }

        if (command.CanExecute(index))
        {
            command.Execute(index);
        }
    }

    //--------------------------------------------------------------------------------
    // ShowOnAwayFromLastTarget
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // CollapseTarget
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty CollapseTargetProperty = BindableProperty.CreateAttached(
        "CollapseTarget",
        typeof(VisualElement),
        typeof(Scroll),
        null,
        propertyChanged: OnCollapseTargetChanged);

    public static VisualElement? GetCollapseTarget(BindableObject bindable) => (VisualElement?)bindable.GetValue(CollapseTargetProperty);

    public static void SetCollapseTarget(BindableObject bindable, VisualElement? value) => bindable.SetValue(CollapseTargetProperty, value);

    public static readonly BindableProperty CollapsedTargetProperty = BindableProperty.CreateAttached(
        "CollapsedTarget",
        typeof(VisualElement),
        typeof(Scroll),
        null);

    public static VisualElement? GetCollapsedTarget(BindableObject bindable) => (VisualElement?)bindable.GetValue(CollapsedTargetProperty);

    public static void SetCollapsedTarget(BindableObject bindable, VisualElement? value) => bindable.SetValue(CollapsedTargetProperty, value);

    private static void OnCollapseTargetChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ScrollView scrollView)
        {
            return;
        }

        if (oldValue is not null)
        {
            scrollView.Scrolled -= OnCollapseScrolled;
        }
        if (newValue is not null)
        {
            scrollView.Scrolled += OnCollapseScrolled;
        }
    }

    private static void OnCollapseScrolled(object? sender, ScrolledEventArgs e)
    {
        if ((sender is not ScrollView scrollView) || (GetCollapseTarget(scrollView) is not { Height: > 0 } target))
        {
            return;
        }

        var scrollY = Math.Max(0, e.ScrollY);
        var progress = Math.Clamp(scrollY / target.Height, 0, 1);
        target.TranslationY = scrollY * 0.4;
        target.Scale = 1 - (0.3 * progress);
        target.Opacity = 1 - progress;

        var collapsed = GetCollapsedTarget(scrollView);
        if (collapsed is not null)
        {
            var visible = Math.Clamp((progress - 0.5) * 2, 0, 1);
            collapsed.Opacity = visible;
            collapsed.TranslationY = (visible - 1) * 12;
        }
    }

    //--------------------------------------------------------------------------------
    // DragScrollSpeed
    //--------------------------------------------------------------------------------

    private static readonly ConditionalWeakTable<View, DragScrollWatcher> DragScrollWatchers = [];

    public static readonly BindableProperty DragScrollSpeedProperty = BindableProperty.CreateAttached(
        "DragScrollSpeed",
        typeof(double),
        typeof(Scroll),
        0d,
        propertyChanged: OnDragScrollSpeedChanged);

    public static double GetDragScrollSpeed(BindableObject bindable) => (double)bindable.GetValue(DragScrollSpeedProperty);

    public static void SetDragScrollSpeed(BindableObject bindable, double value) => bindable.SetValue(DragScrollSpeedProperty, value);

    private static void OnDragScrollSpeedChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        if (DragScrollWatchers.TryGetValue(view, out var watcher))
        {
            watcher.Detach();
            DragScrollWatchers.Remove(view);
        }

        if (newValue is double speed && (speed != 0))
        {
            DragScrollWatchers.Add(view, new DragScrollWatcher(view));
        }
    }

    private sealed class DragScrollWatcher
    {
        private const long MaximumElapsed = 100;

        private readonly View owner;

        private DropGestureRecognizer? recognizer;

        private long moved;

        public DragScrollWatcher(View owner)
        {
            this.owner = owner;
            owner.Loaded += HandleLoaded;
            owner.Unloaded += HandleUnloaded;

            if (owner.IsLoaded)
            {
                Attach();
            }
        }

        public void Detach()
        {
            owner.Loaded -= HandleLoaded;
            owner.Unloaded -= HandleUnloaded;
            Release();
        }

        private void HandleLoaded(object? sender, EventArgs e) => Attach();

        private void HandleUnloaded(object? sender, EventArgs e) => Release();

        private void Attach()
        {
            if (recognizer is not null)
            {
                return;
            }

            recognizer = owner.GestureRecognizers.FirstOrDefault(static x => x is DropGestureRecognizer) as DropGestureRecognizer;
            if (recognizer is null)
            {
                recognizer = new DropGestureRecognizer { AllowDrop = true };
                owner.GestureRecognizers.Add(recognizer);
            }

            recognizer.DragOver += HandleDragOver;
            recognizer.DragLeave += HandleDragLeave;
        }

        private void Release()
        {
            if (recognizer is not null)
            {
                recognizer.DragOver -= HandleDragOver;
                recognizer.DragLeave -= HandleDragLeave;
                recognizer = null;
            }
        }

        private void HandleDragLeave(object? sender, DragEventArgs e) => moved = 0;

        private void HandleDragOver(object? sender, DragEventArgs e)
        {
            var now = Environment.TickCount64;
            var last = moved;
            moved = now;

            if (last == 0)
            {
                return;
            }

            if ((owner.Parent is not Layout layout) ||
                (layout.FirstOrDefault(static x => x is ScrollView) is not ScrollView target))
            {
                return;
            }

            var elapsed = Math.Min(now - last, MaximumElapsed);
            var delta = GetDragScrollSpeed(owner) * elapsed / 1000d;

            var horizontal = target.Orientation == ScrollOrientation.Horizontal;
            var x = horizontal ? Math.Max(0, target.ScrollX + delta) : target.ScrollX;
            var y = horizontal ? target.ScrollY : Math.Max(0, target.ScrollY + delta);
            _ = target.ScrollToAsync(x, y, false);
        }
    }

    //--------------------------------------------------------------------------------
    // SyncTarget
    //--------------------------------------------------------------------------------

    private const double SyncTolerance = 0.5;

    public static readonly BindableProperty SyncTargetProperty = BindableProperty.CreateAttached(
        "SyncTarget",
        typeof(ScrollView),
        typeof(Scroll),
        null,
        propertyChanged: OnSyncTargetChanged);

    public static ScrollView? GetSyncTarget(BindableObject bindable) => (ScrollView?)bindable.GetValue(SyncTargetProperty);

    public static void SetSyncTarget(BindableObject bindable, ScrollView? value) => bindable.SetValue(SyncTargetProperty, value);

    public static readonly BindableProperty SyncOrientationProperty = BindableProperty.CreateAttached(
        "SyncOrientation",
        typeof(ScrollOrientation),
        typeof(Scroll),
        ScrollOrientation.Both);

    public static ScrollOrientation GetSyncOrientation(BindableObject bindable) => (ScrollOrientation)bindable.GetValue(SyncOrientationProperty);

    public static void SetSyncOrientation(BindableObject bindable, ScrollOrientation value) => bindable.SetValue(SyncOrientationProperty, value);

    private static void OnSyncTargetChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not ScrollView scrollView)
        {
            return;
        }

        if (oldValue is not null)
        {
            scrollView.Scrolled -= OnSyncScrolled;
        }
        if (newValue is not null)
        {
            scrollView.Scrolled += OnSyncScrolled;
        }
    }

    private static void OnSyncScrolled(object? sender, ScrolledEventArgs e)
    {
        if ((sender is not ScrollView scrollView) || (GetSyncTarget(scrollView) is not { } target))
        {
            return;
        }

        var orientation = GetSyncOrientation(scrollView);
        var x = orientation is ScrollOrientation.Horizontal or ScrollOrientation.Both ? e.ScrollX : target.ScrollX;
        var y = orientation is ScrollOrientation.Vertical or ScrollOrientation.Both ? e.ScrollY : target.ScrollY;

        // The target raises Scrolled in turn, so stop when it is already in place
        if ((Math.Abs(target.ScrollX - x) < SyncTolerance) && (Math.Abs(target.ScrollY - y) < SyncTolerance))
        {
            return;
        }

        _ = target.ScrollToAsync(x, y, false);
    }
}
