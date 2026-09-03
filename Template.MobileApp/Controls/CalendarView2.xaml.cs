namespace Template.MobileApp.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;

using Microsoft.Maui.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using Template.MobileApp.Models.Sample.Calendar;

// CalendarView の SkiaSharp 完全自前描画版
// カレンダーボディを SKCanvasView 1枚で描画し、タップ・スワイプをすべて自前で処理する
// CalendarView と同じ BindableProperty API を持つ
public partial class CalendarView2
{
    // ------------------------------------------------------------------ BindableProperties: Commands / View

    public static readonly BindableProperty ViewProperty =
        BindableProperty.Create(nameof(View), typeof(MonthView), typeof(CalendarView2),
            propertyChanged: OnViewChanged);

    public static readonly BindableProperty PrevMonthCommandProperty =
        BindableProperty.Create(nameof(PrevMonthCommand), typeof(ICommand), typeof(CalendarView2));

    public static readonly BindableProperty NextMonthCommandProperty =
        BindableProperty.Create(nameof(NextMonthCommand), typeof(ICommand), typeof(CalendarView2));

    public static readonly BindableProperty GoToTodayCommandProperty =
        BindableProperty.Create(nameof(GoToTodayCommand), typeof(ICommand), typeof(CalendarView2));

    public static readonly BindableProperty DayTappedCommandProperty =
        BindableProperty.Create(nameof(DayTappedCommand), typeof(ICommand), typeof(CalendarView2));

    public static readonly BindableProperty EventTappedCommandProperty =
        BindableProperty.Create(nameof(EventTappedCommand), typeof(ICommand), typeof(CalendarView2));

    // ------------------------------------------------------------------ BindableProperties: Selection

    public static readonly BindableProperty SelectionModeProperty =
        BindableProperty.Create(nameof(SelectionMode), typeof(CalendarSelectionMode), typeof(CalendarView2),
            CalendarSelectionMode.None, propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(nameof(SelectedDate), typeof(DateOnly?), typeof(CalendarView2),
            null, BindingMode.TwoWay, propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty SelectedDatesProperty =
        BindableProperty.Create(nameof(SelectedDates), typeof(ObservableCollection<DateOnly>), typeof(CalendarView2),
            null, propertyChanged: OnSelectedDatesChanged);

    public static readonly BindableProperty SelectedStartDateProperty =
        BindableProperty.Create(nameof(SelectedStartDate), typeof(DateOnly?), typeof(CalendarView2),
            null, BindingMode.TwoWay, propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty SelectedEndDateProperty =
        BindableProperty.Create(nameof(SelectedEndDate), typeof(DateOnly?), typeof(CalendarView2),
            null, BindingMode.TwoWay, propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty SelectedDayBackgroundProperty =
        BindableProperty.Create(nameof(SelectedDayBackground), typeof(Color), typeof(CalendarView2),
            Color.FromArgb("#1A73E8"), propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty SelectedDayTextColorProperty =
        BindableProperty.Create(nameof(SelectedDayTextColor), typeof(Color), typeof(CalendarView2),
            Colors.White, propertyChanged: OnInvalidateBody);

    public static readonly BindableProperty RangeBackgroundProperty =
        BindableProperty.Create(nameof(RangeBackground), typeof(Color), typeof(CalendarView2),
            Color.FromArgb("#BDD7F5"), propertyChanged: OnInvalidateBody);

    // ------------------------------------------------------------------ BindableProperties: Navigation limits

    public static readonly BindableProperty MinDateProperty =
        BindableProperty.Create(nameof(MinDate), typeof(DateOnly?), typeof(CalendarView2), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty MaxDateProperty =
        BindableProperty.Create(nameof(MaxDate), typeof(DateOnly?), typeof(CalendarView2), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty DisabledDayTextColorProperty =
        BindableProperty.Create(nameof(DisabledDayTextColor), typeof(Color), typeof(CalendarView2),
            Color.FromArgb("#C0C0C0"), propertyChanged: OnInvalidateAll);

    // ------------------------------------------------------------------ BindableProperties: Localization

    public static readonly BindableProperty CultureProperty =
        BindableProperty.Create(nameof(Culture), typeof(CultureInfo), typeof(CalendarView2), propertyChanged: OnCultureChanged);

    // ------------------------------------------------------------------ BindableProperties: Layout / Sizes

    public static readonly BindableProperty DateRowHeightProperty =
        BindableProperty.Create(nameof(DateRowHeight), typeof(double), typeof(CalendarView2), 26d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty SlotRowHeightProperty =
        BindableProperty.Create(nameof(SlotRowHeight), typeof(double), typeof(CalendarView2), 17d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty DateNumberSizeProperty =
        BindableProperty.Create(nameof(DateNumberSize), typeof(double), typeof(CalendarView2), 22d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty DateNumberMarginProperty =
        BindableProperty.Create(nameof(DateNumberMargin), typeof(Thickness), typeof(CalendarView2), new Thickness(4, 2, 0, 0), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty DateNumberFontSizeProperty =
        BindableProperty.Create(nameof(DateNumberFontSize), typeof(double), typeof(CalendarView2), 14d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty EventFontSizeProperty =
        BindableProperty.Create(nameof(EventFontSize), typeof(double), typeof(CalendarView2), 11d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty EventRowHeightProperty =
        BindableProperty.Create(nameof(EventRowHeight), typeof(double), typeof(CalendarView2), 15d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty StampMarginEdgeProperty =
        BindableProperty.Create(nameof(StampMarginEdge), typeof(double), typeof(CalendarView2), 2d, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty NavButtonWidthProperty =
        BindableProperty.Create(nameof(NavButtonWidth), typeof(double), typeof(CalendarView2), 44d, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty NavButtonHeightProperty =
        BindableProperty.Create(nameof(NavButtonHeight), typeof(double), typeof(CalendarView2), 44d, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty NavButtonFontSizeProperty =
        BindableProperty.Create(nameof(NavButtonFontSize), typeof(double), typeof(CalendarView2), 18d, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty FirstDayOfWeekProperty =
        BindableProperty.Create(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(CalendarView2),
            DayOfWeek.Monday, propertyChanged: OnFirstDayOfWeekChanged);

    public static readonly BindableProperty SwipeEnabledProperty =
        BindableProperty.Create(nameof(SwipeEnabled), typeof(bool), typeof(CalendarView2), true);

    public static readonly BindableProperty HeaderPaddingProperty =
        BindableProperty.Create(nameof(HeaderPadding), typeof(Thickness), typeof(CalendarView2), new Thickness(16, 12, 16, 8), propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty WeekdayHeaderFontSizeProperty =
        BindableProperty.Create(nameof(WeekdayHeaderFontSize), typeof(double), typeof(CalendarView2), 14d, propertyChanged: OnInvalidateWeekdayHeader);

    public static readonly BindableProperty WeekdayHeaderPaddingProperty =
        BindableProperty.Create(nameof(WeekdayHeaderPadding), typeof(Thickness), typeof(CalendarView2), new Thickness(0, 6, 0, 6), propertyChanged: OnInvalidateWeekdayHeader);

    public static readonly BindableProperty YearFontSizeProperty =
        BindableProperty.Create(nameof(YearFontSize), typeof(double), typeof(CalendarView2), 14d, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty MonthFontSizeProperty =
        BindableProperty.Create(nameof(MonthFontSize), typeof(double), typeof(CalendarView2), 28d, propertyChanged: OnInvalidateHeader);

    // ------------------------------------------------------------------ BindableProperties: Colors

    public static readonly BindableProperty GridLineColorProperty =
        BindableProperty.Create(nameof(GridLineColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#E0E0E0"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty WeekdayTextColorProperty =
        BindableProperty.Create(nameof(WeekdayTextColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#1F1F1F"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty SaturdayTextColorProperty =
        BindableProperty.Create(nameof(SaturdayTextColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#2196F3"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty SundayTextColorProperty =
        BindableProperty.Create(nameof(SundayTextColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#E53935"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty OutsideMonthTextColorProperty =
        BindableProperty.Create(nameof(OutsideMonthTextColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#BDBDBD"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty OutsideMonthBackgroundProperty =
        BindableProperty.Create(nameof(OutsideMonthBackground), typeof(Color), typeof(CalendarView2), Color.FromArgb("#F2F2F2"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty WeekendBackgroundProperty =
        BindableProperty.Create(nameof(WeekendBackground), typeof(Color), typeof(CalendarView2), Color.FromArgb("#FFF1F1"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty TodayBackgroundProperty =
        BindableProperty.Create(nameof(TodayBackground), typeof(Color), typeof(CalendarView2), Colors.Black, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty TodayTextColorProperty =
        BindableProperty.Create(nameof(TodayTextColor), typeof(Color), typeof(CalendarView2), Colors.White, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty NavButtonColorProperty =
        BindableProperty.Create(nameof(NavButtonColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#333333"), propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty YearTextColorProperty =
        BindableProperty.Create(nameof(YearTextColor), typeof(Color), typeof(CalendarView2), Colors.Black, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty MonthTextColorProperty =
        BindableProperty.Create(nameof(MonthTextColor), typeof(Color), typeof(CalendarView2), Colors.Black, propertyChanged: OnInvalidateHeader);

    public static readonly BindableProperty WeekdayHeaderColorProperty =
        BindableProperty.Create(nameof(WeekdayHeaderColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#333333"), propertyChanged: OnInvalidateWeekdayHeader);

    public static readonly BindableProperty SaturdayHeaderColorProperty =
        BindableProperty.Create(nameof(SaturdayHeaderColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#2196F3"), propertyChanged: OnInvalidateWeekdayHeader);

    public static readonly BindableProperty SundayHeaderColorProperty =
        BindableProperty.Create(nameof(SundayHeaderColor), typeof(Color), typeof(CalendarView2), Color.FromArgb("#E53935"), propertyChanged: OnInvalidateWeekdayHeader);

    // ------------------------------------------------------------------ BindableProperties: Templates

    public static readonly BindableProperty HeaderTemplateProperty =
        BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(CalendarView2),
            propertyChanged: OnHeaderTemplateChanged);

    public static readonly BindableProperty MonthIndicatorEnabledProperty =
        BindableProperty.Create(nameof(MonthIndicatorEnabled), typeof(bool), typeof(CalendarView2),
            false, propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty MonthIndicatorColorProperty =
        BindableProperty.Create(nameof(MonthIndicatorColor), typeof(Color), typeof(CalendarView2),
            Color.FromArgb("#10000000"), propertyChanged: OnInvalidateAll);

    public static readonly BindableProperty MonthIndicatorFontSizeProperty =
        BindableProperty.Create(nameof(MonthIndicatorFontSize), typeof(double), typeof(CalendarView2),
            160d, propertyChanged: OnInvalidateAll);

    // ------------------------------------------------------------------ CLR Properties

    public MonthView? View { get => (MonthView?)GetValue(ViewProperty); set => SetValue(ViewProperty, value); }
    public ICommand? PrevMonthCommand { get => (ICommand?)GetValue(PrevMonthCommandProperty); set => SetValue(PrevMonthCommandProperty, value); }
    public ICommand? NextMonthCommand { get => (ICommand?)GetValue(NextMonthCommandProperty); set => SetValue(NextMonthCommandProperty, value); }
    public ICommand? GoToTodayCommand { get => (ICommand?)GetValue(GoToTodayCommandProperty); set => SetValue(GoToTodayCommandProperty, value); }
    public ICommand? DayTappedCommand { get => (ICommand?)GetValue(DayTappedCommandProperty); set => SetValue(DayTappedCommandProperty, value); }
    public ICommand? EventTappedCommand { get => (ICommand?)GetValue(EventTappedCommandProperty); set => SetValue(EventTappedCommandProperty, value); }

    public CalendarSelectionMode SelectionMode { get => (CalendarSelectionMode)GetValue(SelectionModeProperty); set => SetValue(SelectionModeProperty, value); }
    public DateOnly? SelectedDate { get => (DateOnly?)GetValue(SelectedDateProperty); set => SetValue(SelectedDateProperty, value); }
    public ObservableCollection<DateOnly>? SelectedDates => (ObservableCollection<DateOnly>?)GetValue(SelectedDatesProperty);
    public DateOnly? SelectedStartDate { get => (DateOnly?)GetValue(SelectedStartDateProperty); set => SetValue(SelectedStartDateProperty, value); }
    public DateOnly? SelectedEndDate { get => (DateOnly?)GetValue(SelectedEndDateProperty); set => SetValue(SelectedEndDateProperty, value); }
    public Color SelectedDayBackground { get => (Color)GetValue(SelectedDayBackgroundProperty); set => SetValue(SelectedDayBackgroundProperty, value); }
    public Color SelectedDayTextColor { get => (Color)GetValue(SelectedDayTextColorProperty); set => SetValue(SelectedDayTextColorProperty, value); }
    public Color RangeBackground { get => (Color)GetValue(RangeBackgroundProperty); set => SetValue(RangeBackgroundProperty, value); }

    public DateOnly? MinDate { get => (DateOnly?)GetValue(MinDateProperty); set => SetValue(MinDateProperty, value); }
    public DateOnly? MaxDate { get => (DateOnly?)GetValue(MaxDateProperty); set => SetValue(MaxDateProperty, value); }
    public Color DisabledDayTextColor { get => (Color)GetValue(DisabledDayTextColorProperty); set => SetValue(DisabledDayTextColorProperty, value); }

    public CultureInfo? Culture { get => (CultureInfo?)GetValue(CultureProperty); set => SetValue(CultureProperty, value); }

    public double DateRowHeight { get => (double)GetValue(DateRowHeightProperty); set => SetValue(DateRowHeightProperty, value); }
    public double SlotRowHeight { get => (double)GetValue(SlotRowHeightProperty); set => SetValue(SlotRowHeightProperty, value); }
    public double DateNumberSize { get => (double)GetValue(DateNumberSizeProperty); set => SetValue(DateNumberSizeProperty, value); }
    public Thickness DateNumberMargin { get => (Thickness)GetValue(DateNumberMarginProperty); set => SetValue(DateNumberMarginProperty, value); }
    public double DateNumberFontSize { get => (double)GetValue(DateNumberFontSizeProperty); set => SetValue(DateNumberFontSizeProperty, value); }
    public double EventFontSize { get => (double)GetValue(EventFontSizeProperty); set => SetValue(EventFontSizeProperty, value); }
    public double EventRowHeight { get => (double)GetValue(EventRowHeightProperty); set => SetValue(EventRowHeightProperty, value); }
    public double StampMarginEdge { get => (double)GetValue(StampMarginEdgeProperty); set => SetValue(StampMarginEdgeProperty, value); }
    public double NavButtonWidth { get => (double)GetValue(NavButtonWidthProperty); set => SetValue(NavButtonWidthProperty, value); }
    public double NavButtonHeight { get => (double)GetValue(NavButtonHeightProperty); set => SetValue(NavButtonHeightProperty, value); }
    public double NavButtonFontSize { get => (double)GetValue(NavButtonFontSizeProperty); set => SetValue(NavButtonFontSizeProperty, value); }
    public DayOfWeek FirstDayOfWeek { get => (DayOfWeek)GetValue(FirstDayOfWeekProperty); set => SetValue(FirstDayOfWeekProperty, value); }
    public bool SwipeEnabled { get => (bool)GetValue(SwipeEnabledProperty); set => SetValue(SwipeEnabledProperty, value); }
    public Thickness HeaderPadding { get => (Thickness)GetValue(HeaderPaddingProperty); set => SetValue(HeaderPaddingProperty, value); }
    public double WeekdayHeaderFontSize { get => (double)GetValue(WeekdayHeaderFontSizeProperty); set => SetValue(WeekdayHeaderFontSizeProperty, value); }
    public Thickness WeekdayHeaderPadding { get => (Thickness)GetValue(WeekdayHeaderPaddingProperty); set => SetValue(WeekdayHeaderPaddingProperty, value); }
    public double YearFontSize { get => (double)GetValue(YearFontSizeProperty); set => SetValue(YearFontSizeProperty, value); }
    public double MonthFontSize { get => (double)GetValue(MonthFontSizeProperty); set => SetValue(MonthFontSizeProperty, value); }

    public Color GridLineColor { get => (Color)GetValue(GridLineColorProperty); set => SetValue(GridLineColorProperty, value); }
    public Color WeekdayTextColor { get => (Color)GetValue(WeekdayTextColorProperty); set => SetValue(WeekdayTextColorProperty, value); }
    public Color SaturdayTextColor { get => (Color)GetValue(SaturdayTextColorProperty); set => SetValue(SaturdayTextColorProperty, value); }
    public Color SundayTextColor { get => (Color)GetValue(SundayTextColorProperty); set => SetValue(SundayTextColorProperty, value); }
    public Color OutsideMonthTextColor { get => (Color)GetValue(OutsideMonthTextColorProperty); set => SetValue(OutsideMonthTextColorProperty, value); }
    public Color OutsideMonthBackground { get => (Color)GetValue(OutsideMonthBackgroundProperty); set => SetValue(OutsideMonthBackgroundProperty, value); }
    public Color WeekendBackground { get => (Color)GetValue(WeekendBackgroundProperty); set => SetValue(WeekendBackgroundProperty, value); }
    public Color TodayBackground { get => (Color)GetValue(TodayBackgroundProperty); set => SetValue(TodayBackgroundProperty, value); }
    public Color TodayTextColor { get => (Color)GetValue(TodayTextColorProperty); set => SetValue(TodayTextColorProperty, value); }
    public Color NavButtonColor { get => (Color)GetValue(NavButtonColorProperty); set => SetValue(NavButtonColorProperty, value); }
    public Color YearTextColor { get => (Color)GetValue(YearTextColorProperty); set => SetValue(YearTextColorProperty, value); }
    public Color MonthTextColor { get => (Color)GetValue(MonthTextColorProperty); set => SetValue(MonthTextColorProperty, value); }
    public Color WeekdayHeaderColor { get => (Color)GetValue(WeekdayHeaderColorProperty); set => SetValue(WeekdayHeaderColorProperty, value); }
    public Color SaturdayHeaderColor { get => (Color)GetValue(SaturdayHeaderColorProperty); set => SetValue(SaturdayHeaderColorProperty, value); }
    public Color SundayHeaderColor { get => (Color)GetValue(SundayHeaderColorProperty); set => SetValue(SundayHeaderColorProperty, value); }

    public DataTemplate? HeaderTemplate { get => (DataTemplate?)GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }
    public bool MonthIndicatorEnabled { get => (bool)GetValue(MonthIndicatorEnabledProperty); set => SetValue(MonthIndicatorEnabledProperty, value); }
    public Color MonthIndicatorColor { get => (Color)GetValue(MonthIndicatorColorProperty); set => SetValue(MonthIndicatorColorProperty, value); }
    public double MonthIndicatorFontSize { get => (double)GetValue(MonthIndicatorFontSizeProperty); set => SetValue(MonthIndicatorFontSizeProperty, value); }

    // ------------------------------------------------------------------ Internal constants

    private const int DaysPerWeek = 7;
    private const int MaxWeekRows = 6;
    private const double SwipeThreshold = 36d;
    private const double SwipeFlickThreshold = 20d;
    private const double SwipeHorizontalBias = 1.25d;
    private const string MonthSlideAnimationName = "CalendarView2MonthSlide";

    // 絵文字スタンプ用タイプフェイス(Skia はフォントフォールバックしないため明示解決する)
    private static readonly SKTypeface EmojiTypeface =
        SKFontManager.Default.MatchCharacter(0x1F431) ?? SKTypeface.Default;

    // 絵文字の異体字セレクタ(U+FE0F)。絵文字フォントでは独立グリフ(豆腐)になるため描画前に除去する
    private static readonly string VariationSelector = ((char)0xFE0F).ToString();

    // ------------------------------------------------------------------ Hit-test records (filled each paint)

    // Day cell hit areas: [weekRow][col]
    private readonly SKRect[][] dayCellRects = CreateDayCellRects();

    private static SKRect[][] CreateDayCellRects()
    {
        var rows = new SKRect[MaxWeekRows][];
        for (var i = 0; i < MaxWeekRows; i++)
        {
            rows[i] = new SKRect[DaysPerWeek];
        }
        return rows;
    }

    // Event hit areas for the current month
    private readonly record struct EventHitRect(SKRect Rect, ScheduleEvent Event);
    private readonly List<EventHitRect> eventHitRects = [];

    // Cached week row info for hit testing
    private int renderedWeekCount;

    // ------------------------------------------------------------------ Swipe state

    private double panStartX;
    private double panStartY;
    private double lastPanX;
    private bool panConsumed;

    // ------------------------------------------------------------------ Constructor

    public CalendarView2()
    {
        InitializeComponent();

        UpdateHeaderStyle();

        // Attach PanGestureRecognizer to the body canvas for swipe month navigation and tap
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        BodyCanvas.GestureRecognizers.Add(pan);

        // Tap on body canvas: resolved in OnPanUpdated Completed with no movement
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnBodyTapped;
        BodyCanvas.GestureRecognizers.Add(tap);
    }

    // ------------------------------------------------------------------ Property changed callbacks

    private static void OnViewChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.UpdateHeaderContent();
        self.InvalidateCanvases();
        self.AnimateMonthSlide(oldValue as MonthView, newValue as MonthView);
    }

    // 月切替時にボディを進行方向から軽くスライドさせる
    private void AnimateMonthSlide(MonthView? oldView, MonthView? newView)
    {
        if ((oldView is null) || (newView is null))
        {
            return;
        }

        var direction = ((newView.Year * 12) + newView.Month) >= ((oldView.Year * 12) + oldView.Month) ? 1 : -1;

        BodyCanvas.AbortAnimation(MonthSlideAnimationName);
        BodyCanvas.Animate(
            MonthSlideAnimationName,
            v =>
            {
                BodyCanvas.TranslationX = direction * 48d * (1 - v);
                BodyCanvas.Opacity = 0.4 + (0.6 * v);
            },
            16,
            220,
            Easing.CubicOut,
            (_, _) =>
            {
                BodyCanvas.TranslationX = 0;
                BodyCanvas.Opacity = 1;
            });
    }

    private static void OnInvalidateAll(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.InvalidateCanvases();
    }

    private static void OnInvalidateBody(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.BodyCanvas.InvalidateSurface();
    }

    private static void OnInvalidateHeader(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.UpdateHeaderStyle();
        self.UpdateHeaderContent();
    }

    private static void OnInvalidateWeekdayHeader(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.WeekdayHeaderCanvas.InvalidateSurface();
    }

    private static void OnFirstDayOfWeekChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.WeekdayHeaderCanvas.InvalidateSurface();
        self.BodyCanvas.InvalidateSurface();
    }

    private static void OnCultureChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.WeekdayHeaderCanvas.InvalidateSurface();
        self.BodyCanvas.InvalidateSurface();
        self.UpdateHeaderContent();
    }

    private static void OnSelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        if (oldValue is ObservableCollection<DateOnly> old)
        {
            old.CollectionChanged -= self.OnSelectedDatesCollectionChanged;
        }
        if (newValue is ObservableCollection<DateOnly> next)
        {
            next.CollectionChanged += self.OnSelectedDatesCollectionChanged;
        }
        self.BodyCanvas.InvalidateSurface();
    }

    private void OnSelectedDatesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BodyCanvas.InvalidateSurface();
    }

    private static void OnHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (CalendarView2)bindable;
        self.ApplyHeaderTemplate();
        self.UpdateHeaderContent();
    }

    // ------------------------------------------------------------------ Header

    private void UpdateHeaderStyle()
    {
        PrevButton.TextColor = NavButtonColor;
        PrevButton.FontSize = NavButtonFontSize;
        PrevButton.WidthRequest = NavButtonWidth;
        PrevButton.HeightRequest = NavButtonHeight;
        NextButton.TextColor = NavButtonColor;
        NextButton.FontSize = NavButtonFontSize;
        NextButton.WidthRequest = NavButtonWidth;
        NextButton.HeightRequest = NavButtonHeight;
        HeaderGrid.Padding = HeaderPadding;
        YearLabel.FontSize = YearFontSize;
        MonthLabel.FontSize = MonthFontSize;
    }

    private void ApplyHeaderTemplate()
    {
        if (HeaderTemplate is { } template)
        {
            HeaderContentHost.Content = (View)template.CreateContent();
        }
        else
        {
            HeaderContentHost.Content = HeaderGrid;
        }
    }

    private void UpdateHeaderContent()
    {
        if (View is not { } month)
        {
            return;
        }

        if (HeaderTemplate is not null)
        {
            HeaderContentHost.BindingContext = month;
        }
        else
        {
            YearLabel.Text = month.Year.ToString(CultureInfo.InvariantCulture);
            YearLabel.TextColor = YearTextColor;
            MonthLabel.Text = GetMonthDisplayName(month.Month);
            MonthLabel.TextColor = MonthTextColor;
        }
    }

    // ------------------------------------------------------------------ Canvas invalidation

    private void InvalidateCanvases()
    {
        WeekdayHeaderCanvas.InvalidateSurface();
        BodyCanvas.InvalidateSurface();
    }

    // ------------------------------------------------------------------ Weekday header paint

    private void OnWeekdayHeaderPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var density = (float)(info.Width / WeekdayHeaderCanvas.Width);
        var colWidth = (float)info.Width / DaysPerWeek;
        var padTop = (float)(WeekdayHeaderPadding.Top * density);
        var padBottom = (float)(WeekdayHeaderPadding.Bottom * density);

        // SKTypeface.Default は日本語グリフを持たないためアプリ内蔵フォントを使う
        using var font = new SKFont(SocialFonts.NotoSerifJP);
        font.Size = (float)(WeekdayHeaderFontSize * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;

        var start = (int)FirstDayOfWeek;
        for (var i = 0; i < DaysPerWeek; i++)
        {
            var dow = (DayOfWeek)((start + i) % DaysPerWeek);
            paint.Color = GetWeekdayHeaderSkColor(dow);
            var text = GetDayOfWeekShortName(dow);
            var x = (colWidth * i) + (colWidth / 2f);
            // vertically center text within padding area
            var metrics = font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var y = padTop + ((info.Height - padTop - padBottom - textHeight) / 2f) - metrics.Ascent;
            canvas.DrawText(text, x, y, SKTextAlign.Center, font, paint);
        }
        _ = padBottom; // suppress warning
    }

    // ------------------------------------------------------------------ Body paint

    private void OnBodyPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if (View is not { } month)
        {
            return;
        }

        var density = (float)(info.Width / BodyCanvas.Width);
        var weekCount = month.Weeks.Count;
        renderedWeekCount = weekCount;

        var colWidth = (float)info.Width / DaysPerWeek;

        var dateRowH = (float)(DateRowHeight * density);
        var slotRowH = (float)(SlotRowHeight * density);

        // Total height available per week row
        var rowH = (float)info.Height / weekCount;

        // Clear hit-test data
        eventHitRects.Clear();

        // 月番号ウォーターマーク(セル背景より先に描き、背景付きセルの下に沈める)
        if (MonthIndicatorEnabled)
        {
            using var indicatorFont = new SKFont(SocialFonts.NotoSerifJP);
            indicatorFont.Size = (float)(MonthIndicatorFontSize * density);
            indicatorFont.Embolden = true;
            using var indicatorPaint = new SKPaint();
            indicatorPaint.IsAntialias = true;
            indicatorPaint.Color = MonthIndicatorColor.ToSKColor();
            var indicatorMetrics = indicatorFont.Metrics;
            var indicatorY = ((info.Height - (indicatorMetrics.Descent - indicatorMetrics.Ascent)) / 2f) - indicatorMetrics.Ascent;
            canvas.DrawText(
                month.Month.ToString(CultureInfo.InvariantCulture),
                info.Width / 2f,
                indicatorY,
                SKTextAlign.Center,
                indicatorFont,
                indicatorPaint);
        }

        using var fillPaint = new SKPaint();
        fillPaint.IsAntialias = false;
        // SKTypeface.Default は日本語グリフを持たないためアプリ内蔵フォントを使う
        using var textFont = new SKFont(SocialFonts.NotoSerifJP);
        using var textPaint = new SKPaint();
        textPaint.IsAntialias = true;
        // 絵文字スタンプは専用タイプフェイスで描く
        using var stampFont = new SKFont(EmojiTypeface);
        using var eventTextFont = new SKFont(SocialFonts.NotoSerifJP);
        eventTextFont.Size = (float)(EventFontSize * density);
        using var eventTextPaint = new SKPaint();
        eventTextPaint.IsAntialias = true;
        using var dividerPaint = new SKPaint();
        dividerPaint.Color = GridLineColor.ToSKColor();
        dividerPaint.StrokeWidth = density * 0.5f;
        dividerPaint.IsAntialias = false;

        for (var weekIndex = 0; weekIndex < weekCount; weekIndex++)
        {
            var week = month.Weeks[weekIndex];
            var weekTop = rowH * weekIndex;
            var weekBottom = weekTop + rowH;

            // Top divider
            canvas.DrawLine(0, weekTop, info.Width, weekTop, dividerPaint);

            for (var col = 0; col < DaysPerWeek; col++)
            {
                var day = week.Days[col];
                var cellLeft = colWidth * col;
                var cellRight = cellLeft + colWidth;
                var cellRect = new SKRect(cellLeft, weekTop, cellRight, weekBottom);

                // Store for hit-testing
                dayCellRects[weekIndex][col] = cellRect;

                // Cell background
                var bgColor = GetCellBgSkColor(day);
                if (bgColor != SKColor.Empty)
                {
                    fillPaint.Color = bgColor;
                    canvas.DrawRect(cellRect, fillPaint);
                }

                // Range background (interior of range)
                var rangeBg = GetRangeCellSkBg(day.Date);
                if (rangeBg != SKColor.Empty)
                {
                    fillPaint.Color = rangeBg;
                    // Draw range bg only on the date row portion
                    canvas.DrawRect(new SKRect(cellLeft, weekTop, cellRight, weekTop + dateRowH), fillPaint);
                }

                // Vertical divider (right edge, skip last col)
                if (col < DaysPerWeek - 1)
                {
                    canvas.DrawLine(cellRight, weekTop, cellRight, weekBottom, dividerPaint);
                }

                // Date number bubble
                DrawDateNumber(canvas, day, cellLeft, weekTop, density, textFont, textPaint, fillPaint);

                // Stamp glyphs
                DrawStamps(canvas, day, cellLeft, cellRight, weekTop, weekBottom, density, stampFont, textPaint);
            }

            // Event slots
            DrawEvents(canvas, week, colWidth, weekTop, dateRowH, slotRowH, density, eventTextFont, eventTextPaint, fillPaint);
        }

        // Bottom border of last week
        var lastBottom = rowH * weekCount;
        canvas.DrawLine(0, lastBottom, info.Width, lastBottom, dividerPaint);
    }

    // ------------------------------------------------------------------ Draw helpers

    private void DrawDateNumber(
        SKCanvas canvas,
        DayView day,
        float cellLeft,
        float weekTop,
        float density,
        SKFont textFont,
        SKPaint textPaint,
        SKPaint fillPaint)
    {
        var disabled = IsDateDisabled(day.Date);
        var selected = IsDateSelected(day.Date);

        SKColor textColor;
        SKColor bubbleBg;

        if (disabled)
        {
            textColor = DisabledDayTextColor.ToSKColor();
            bubbleBg = SKColor.Empty;
        }
        else if (selected)
        {
            textColor = SelectedDayTextColor.ToSKColor();
            bubbleBg = SelectedDayBackground.ToSKColor();
        }
        else if (day.IsToday)
        {
            textColor = TodayTextColor.ToSKColor();
            bubbleBg = TodayBackground.ToSKColor();
        }
        else
        {
            textColor = GetDateTextSkColor(day);
            bubbleBg = SKColor.Empty;
        }

        var size = (float)(DateNumberSize * density);
        var marginLeft = (float)(DateNumberMargin.Left * density);
        var marginTop = (float)(DateNumberMargin.Top * density);

        var bubbleLeft = cellLeft + marginLeft;
        var bubbleTop = weekTop + marginTop;
        var bubbleRect = new SKRect(bubbleLeft, bubbleTop, bubbleLeft + size, bubbleTop + size);

        if (bubbleBg != SKColor.Empty)
        {
            fillPaint.Color = bubbleBg;
            var radius = size / 2f * 0.3f; // small rounded corner
            canvas.DrawRoundRect(bubbleRect, radius, radius, fillPaint);
        }

        textPaint.Color = textColor;
        textFont.Size = (float)(DateNumberFontSize * density);
        textFont.Embolden = true;
        var text = day.Date.Day.ToString(CultureInfo.InvariantCulture);
        var metrics = textFont.Metrics;
        var textY = bubbleTop + ((size - (metrics.Descent - metrics.Ascent)) / 2f) - metrics.Ascent;
        var textX = bubbleLeft + (size / 2f);
        canvas.DrawText(text, textX, textY, SKTextAlign.Center, textFont, textPaint);
        textFont.Embolden = false;
    }

    private void DrawStamps(
        SKCanvas canvas,
        DayView day,
        float cellLeft, float cellRight,
        float weekTop, float weekBottom,
        float density,
        SKFont textFont,
        SKPaint textPaint)
    {
        if (day.Stamps.Count == 0)
        {
            return;
        }

        var cellWidth = cellRight - cellLeft;
        var cellHeight = weekBottom - weekTop;
        var edge = (float)(StampMarginEdge * density);

        foreach (var stamp in day.Stamps)
        {
            var glyph = stamp.Glyph.Replace(VariationSelector, string.Empty, StringComparison.Ordinal);

            textFont.Size = (float)(stamp.FontSize * density);
            textPaint.Color = SKColors.Black.WithAlpha((byte)(stamp.Opacity * 255));

            var metrics = textFont.Metrics;
            var textH = metrics.Descent - metrics.Ascent;
            var textW = textFont.MeasureText(glyph);

            float x, y;
            switch (stamp.Position)
            {
                case StampPosition.TopLeft:
                    x = cellLeft + edge + (textW / 2f);
                    y = weekTop + edge - metrics.Ascent;
                    break;
                case StampPosition.TopCenter:
                    x = cellLeft + (cellWidth / 2f);
                    y = weekTop + edge - metrics.Ascent;
                    break;
                case StampPosition.TopRight:
                    x = cellRight - edge - (textW / 2f);
                    y = weekTop + edge - metrics.Ascent;
                    break;
                case StampPosition.BottomLeft:
                    x = cellLeft + edge + (textW / 2f);
                    y = weekBottom - edge - metrics.Descent;
                    break;
                case StampPosition.BottomCenter:
                    x = cellLeft + (cellWidth / 2f);
                    y = weekBottom - edge - metrics.Descent;
                    break;
                case StampPosition.BottomRight:
                    x = cellRight - edge - (textW / 2f);
                    y = weekBottom - edge - metrics.Descent;
                    break;
                default: // Center
                    x = cellLeft + (cellWidth / 2f);
                    y = weekTop + ((cellHeight - textH) / 2f) - metrics.Ascent;
                    break;
            }

            canvas.DrawText(glyph, x, y, SKTextAlign.Center, textFont, textPaint);
        }
    }

    private void DrawEvents(
        SKCanvas canvas,
        WeekView week,
        float colWidth,
        float weekTop,
        float dateRowH,
        float slotRowH,
        float density,
        SKFont eventTextFont,
        SKPaint eventTextPaint,
        SKPaint fillPaint)
    {
        var eventRowH = (float)(EventRowHeight * density);
        var padding = 4f * density;

        foreach (var placement in week.EventPlacements)
        {
            var evt = placement.Event;
            var left = colWidth * placement.StartColumn;
            var right = left + (colWidth * placement.ColumnSpan);
            var top = weekTop + dateRowH + (slotRowH * placement.Slot) + (1f * density);
            var bottom = top + eventRowH;

            var hitRect = new SKRect(left, top, right, bottom);
            eventHitRects.Add(new EventHitRect(hitRect, evt));

            if (evt.Style == ScheduleStyle.Filled)
            {
                fillPaint.Color = evt.BackgroundColor.ToSKColor();
                var rl = placement.ContinuesFromPreviousWeek ? 0 : 4f * density;
                var rr = placement.ContinuesToNextWeek ? 0 : 4f * density;
                var marginL = placement.ContinuesFromPreviousWeek ? 0 : 1f * density;
                var marginR = placement.ContinuesToNextWeek ? 0 : 1f * density;

                using var rrRect = new SKRoundRect();
                rrRect.SetRectRadii(
                    new SKRect(left + marginL, top, right - marginR, bottom),
                    [new SKPoint(rl, rl), new SKPoint(rr, rr), new SKPoint(rr, rr), new SKPoint(rl, rl)]);
                canvas.DrawRoundRect(rrRect, fillPaint);
            }

            // Event title text
            eventTextPaint.Color = evt.TextColor.ToSKColor();
            var textLeft = left + padding;
            var metrics = eventTextFont.Metrics;
            var textY = top + ((eventRowH - (metrics.Descent - metrics.Ascent)) / 2f) - metrics.Ascent;
            var maxWidth = right - left - (padding * 2f);
            var title = TruncateText(evt.Title, eventTextFont, maxWidth);
            canvas.DrawText(title, textLeft, textY, SKTextAlign.Left, eventTextFont, eventTextPaint);
        }
    }

    // ------------------------------------------------------------------ Utility

    private static string TruncateText(string text, SKFont font, float maxWidth)
    {
        if (font.MeasureText(text) <= maxWidth)
        {
            return text;
        }
        var ellipsis = "…";
        var ellipsisWidth = font.MeasureText(ellipsis);
        var available = maxWidth - ellipsisWidth;
        if (available <= 0)
        {
            return ellipsis;
        }
        for (var i = text.Length - 1; i >= 0; i--)
        {
            var sub = text[..i];
            if (font.MeasureText(sub) <= available)
            {
                return sub + ellipsis;
            }
        }
        return ellipsis;
    }

    // ------------------------------------------------------------------ Color helpers

    private SKColor GetCellBgSkColor(DayView day)
    {
        if (!day.IsCurrentMonth)
        {
            return OutsideMonthBackground.ToSKColor();
        }
        return day.Kind switch
        {
            DayKind.Saturday or DayKind.Sunday or DayKind.Holiday => WeekendBackground.ToSKColor(),
            _ => SKColor.Empty
        };
    }

    private SKColor GetRangeCellSkBg(DateOnly date)
    {
        if (SelectionMode != CalendarSelectionMode.Range)
        {
            return SKColor.Empty;
        }
        if ((SelectedStartDate is not { } start) || (SelectedEndDate is not { } end))
        {
            return SKColor.Empty;
        }
        if (start > end)
        {
            (start, end) = (end, start);
        }
        return (date > start) && (date < end) ? RangeBackground.ToSKColor() : SKColor.Empty;
    }

    private SKColor GetDateTextSkColor(DayView day)
    {
        if (!day.IsCurrentMonth)
        {
            return OutsideMonthTextColor.ToSKColor();
        }
        return day.Kind switch
        {
            DayKind.Sunday or DayKind.Holiday => SundayTextColor.ToSKColor(),
            DayKind.Saturday => SaturdayTextColor.ToSKColor(),
            _ => WeekdayTextColor.ToSKColor()
        };
    }

    private SKColor GetWeekdayHeaderSkColor(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Saturday => SaturdayHeaderColor.ToSKColor(),
        DayOfWeek.Sunday => SundayHeaderColor.ToSKColor(),
        _ => WeekdayHeaderColor.ToSKColor()
    };

    // ------------------------------------------------------------------ Selection / disabled

    private bool IsDateDisabled(DateOnly date)
    {
        if ((MinDate is { } min) && (date < min))
        {
            return true;
        }
        if ((MaxDate is { } max) && (date > max))
        {
            return true;
        }
        return false;
    }

    private bool IsDateSelected(DateOnly date) => SelectionMode switch
    {
        CalendarSelectionMode.Single => SelectedDate == date,
        CalendarSelectionMode.Multiple => SelectedDates?.Contains(date) == true,
        CalendarSelectionMode.Range => IsInRange(date),
        _ => false
    };

    private bool IsInRange(DateOnly date)
    {
        if ((SelectedStartDate is not { } start) || (SelectedEndDate is not { } end))
        {
            return false;
        }
        if (start > end)
        {
            (start, end) = (end, start);
        }
        return (date >= start) && (date <= end);
    }

    // ------------------------------------------------------------------ Localization

    private string GetDayOfWeekShortName(DayOfWeek dow)
    {
        var culture = Culture;
        if (culture is not null)
        {
            var abbreviated = culture.DateTimeFormat.GetAbbreviatedDayName(dow);
            return abbreviated.Length > 0 ? abbreviated[..1].ToUpper(culture) : abbreviated;
        }
        return dow switch
        {
            DayOfWeek.Monday => "月",
            DayOfWeek.Tuesday => "火",
            DayOfWeek.Wednesday => "水",
            DayOfWeek.Thursday => "木",
            DayOfWeek.Friday => "金",
            DayOfWeek.Saturday => "土",
            DayOfWeek.Sunday => "日",
            _ => string.Empty
        };
    }

    private string GetMonthDisplayName(int month)
    {
        var culture = Culture;
        if (culture is not null)
        {
            return culture.DateTimeFormat.GetMonthName(month).ToUpper(culture);
        }
        return $"{month}\u6708";
    }

    // ------------------------------------------------------------------ PanGesture (swipe + tap fallback)

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panStartX = e.TotalX;
                panStartY = e.TotalY;
                lastPanX = e.TotalX;
                panConsumed = false;
                break;

            case GestureStatus.Running:
                if (panConsumed)
                {
                    break;
                }
                var dx = e.TotalX - panStartX;
                var dy = e.TotalY - panStartY;
                var stepDx = e.TotalX - lastPanX;
                lastPanX = e.TotalX;
                var absDx = Math.Abs(dx);
                var absDy = Math.Abs(dy);
                if (absDx < 8d)
                {
                    break;
                }
                if (absDx < absDy * SwipeHorizontalBias)
                {
                    break;
                }
                if ((absDx >= SwipeThreshold) || (Math.Abs(stepDx) >= SwipeFlickThreshold))
                {
                    NavigateBySwipe(dx < 0 ? 1 : -1);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                panConsumed = false;
                break;
        }
    }

    private void NavigateBySwipe(int direction)
    {
        if (!SwipeEnabled || panConsumed)
        {
            return;
        }
        panConsumed = true;
        if (direction > 0)
        {
            NextMonthCommand?.Execute(null);
        }
        else
        {
            PrevMonthCommand?.Execute(null);
        }
    }

    // ------------------------------------------------------------------ Tap on body canvas

    private void OnBodyTapped(object? sender, TappedEventArgs e)
    {
        if (View is not { } month)
        {
            return;
        }

        var pos = e.GetPosition(BodyCanvas);
        if (pos is null)
        {
            return;
        }

        var density = (float)(BodyCanvas.Width > 0 ? BodyCanvas.CanvasSize.Width / BodyCanvas.Width : 1.0);
        var px = (float)(pos.Value.X * density);
        var py = (float)(pos.Value.Y * density);

        var skPos = new SKPoint(px, py);

        // Check event hit first
        foreach (var hit in eventHitRects)
        {
            if (hit.Rect.Contains(skPos))
            {
                OnEventTapped(hit.Event);
                return;
            }
        }

        // Check day cell hit
        var weekCount = Math.Min(renderedWeekCount, month.Weeks.Count);
        for (var weekIndex = 0; weekIndex < weekCount; weekIndex++)
        {
            for (var col = 0; col < DaysPerWeek; col++)
            {
                if (dayCellRects[weekIndex][col].Contains(skPos))
                {
                    var day = month.Weeks[weekIndex].Days[col];
                    if (!IsDateDisabled(day.Date))
                    {
                        OnDayTapped(day);
                    }
                    return;
                }
            }
        }
    }

    // ------------------------------------------------------------------ Tap handlers

    private void OnDayTapped(DayView day)
    {
        var date = day.Date;
        switch (SelectionMode)
        {
            case CalendarSelectionMode.Single:
                SelectedDate = SelectedDate == date ? null : date;
                break;

            case CalendarSelectionMode.Multiple:
                if (SelectedDates is not { } list)
                {
                    list = [];
                    SetValue(SelectedDatesProperty, list);
                }
                if (!list.Remove(date))
                {
                    list.Add(date);
                }
                break;

            case CalendarSelectionMode.Range:
                if ((SelectedStartDate is null) || ((SelectedStartDate is not null) && (SelectedEndDate is not null)))
                {
                    SelectedStartDate = date;
                    SelectedEndDate = null;
                }
                else
                {
                    if (date < SelectedStartDate)
                    {
                        (SelectedStartDate, SelectedEndDate) = (date, SelectedStartDate);
                    }
                    else
                    {
                        SelectedEndDate = date;
                    }
                }
                break;
        }

        DayTappedCommand?.Execute(day);
    }

    private void OnEventTapped(ScheduleEvent evt) => EventTappedCommand?.Execute(evt);
}
