namespace Template.MobileApp.Controls;

using System.Collections.Specialized;
using System.Windows.Input;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public sealed class DayTimetableView : SKCanvasView
{
    private const float TimeColumnWidth = 56f;

    private const float PaddingTop = 16f;

    private const float PaddingBottom = 16f;

    private const float PaddingRight = 12f;

    private const float LaneSpacing = 4f;

    private const float TextTop = 4f;

    private const float LineHeight = 16f;

    private const float MinimumWidthForDuration = 110f;

    private const float FontSize = 12f;

    private const float TimeColumnSpacing = 8f;

    private const float TimeTextMargin = 8f;

    private const float TimeTextOffset = 4f;

    private const float GridLineWidth = 1f;

    private const float CurrentTimeWidth = 2f;

    private const float CurrentTimeRadius = 5f;

    private const float FreeSlotInset = 1f;

    private const float FreeSlotRadius = 6f;

    private const float FreeTextLeft = 8f;

    private const float FreeTextTop = 16f;

    private const float CardInset = 2f;

    private const float CardRadius = 8f;

    private const float CardBorderWidth = 1f;

    private const float CardShadowOffset = 2f;

    private const float CardShadowBlur = 3f;

    private const float AccentInset = 4f;

    private const float AccentWidth = 4f;

    private const float AccentRadius = 2f;

    private const float TextLeft = 14f;

    private const float TextRight = 8f;

    private const float BaselineOffset = 4f;

    private const float DurationSpacing = 8f;

    private const string Ellipsis = "…";

    private static readonly SKColor LineColor = new(0xE0, 0xE0, 0xE0);

    private static readonly SKColor TimeTextColor = new(0x9E, 0x9E, 0x9E);

    private static readonly SKColor CurrentTimeColor = new(0xE5, 0x39, 0x35);

    private static readonly SKColor CardBorderColor = new(0xEE, 0xEE, 0xEE);

    private static readonly SKColor TitleColor = new(0x42, 0x42, 0x42);

    private static readonly SKColor FreeFillColor = new(0x43, 0xA0, 0x47, 0x12);

    private static readonly SKColor FreeTextColor = new(0x66, 0xBB, 0x6A);

    private static readonly SKColor CardShadowColor = new(0x00, 0x00, 0x00, 0x30);

    private static readonly Dictionary<int, string> HourTexts = [];

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly List<(SKRect Rect, TimetableEvent Event)> eventRects = [];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty HourHeightProperty = BindableProperty.Create(
        nameof(HourHeight),
        typeof(float),
        typeof(DayTimetableView),
        60f,
        propertyChanged: OnLayoutChanged);

    public static readonly BindableProperty StartTimeProperty = BindableProperty.Create(
        nameof(StartTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromHours(8),
        propertyChanged: OnLayoutChanged);

    public static readonly BindableProperty EndTimeProperty = BindableProperty.Create(
        nameof(EndTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromHours(20),
        propertyChanged: OnLayoutChanged);

    public static readonly BindableProperty TimeSlotIntervalProperty = BindableProperty.Create(
        nameof(TimeSlotInterval),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromMinutes(30),
        propertyChanged: Invalidate);

    public static readonly BindableProperty EventsProperty = BindableProperty.Create(
        nameof(Events),
        typeof(IReadOnlyList<TimetableEvent>),
        typeof(DayTimetableView),
        propertyChanged: OnEventsChanged);

    public static readonly BindableProperty CurrentTimeProperty = BindableProperty.Create(
        nameof(CurrentTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.Zero,
        propertyChanged: Invalidate);

    public static readonly BindableProperty ShowCurrentTimeProperty = BindableProperty.Create(
        nameof(ShowCurrentTime),
        typeof(bool),
        typeof(DayTimetableView),
        false,
        propertyChanged: Invalidate);

    public static readonly BindableProperty FreeSlotHighlightVisibleProperty = BindableProperty.Create(
        nameof(FreeSlotHighlightVisible),
        typeof(bool),
        typeof(DayTimetableView),
        true,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MinimumFreeSlotForLabelProperty = BindableProperty.Create(
        nameof(MinimumFreeSlotForLabel),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromMinutes(45),
        propertyChanged: Invalidate);

    public static readonly BindableProperty EventTappedCommandProperty = BindableProperty.Create(
        nameof(EventTappedCommand),
        typeof(ICommand),
        typeof(DayTimetableView));

    public float HourHeight
    {
        get => (float)GetValue(HourHeightProperty);
        set => SetValue(HourHeightProperty, value);
    }

    public TimeSpan StartTime
    {
        get => (TimeSpan)GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public TimeSpan EndTime
    {
        get => (TimeSpan)GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }

    public TimeSpan TimeSlotInterval
    {
        get => (TimeSpan)GetValue(TimeSlotIntervalProperty);
        set => SetValue(TimeSlotIntervalProperty, value);
    }

    public IReadOnlyList<TimetableEvent>? Events
    {
        get => (IReadOnlyList<TimetableEvent>?)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public bool ShowCurrentTime
    {
        get => (bool)GetValue(ShowCurrentTimeProperty);
        set => SetValue(ShowCurrentTimeProperty, value);
    }

    public bool FreeSlotHighlightVisible
    {
        get => (bool)GetValue(FreeSlotHighlightVisibleProperty);
        set => SetValue(FreeSlotHighlightVisibleProperty, value);
    }

    public TimeSpan MinimumFreeSlotForLabel
    {
        get => (TimeSpan)GetValue(MinimumFreeSlotForLabelProperty);
        set => SetValue(MinimumFreeSlotForLabelProperty, value);
    }

    public ICommand? EventTappedCommand
    {
        get => (ICommand?)GetValue(EventTappedCommandProperty);
        set => SetValue(EventTappedCommandProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DayTimetableView()
    {
        BackgroundColor = Colors.White;
        UpdateHeight();

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    private static void OnLayoutChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DayTimetableView)bindable;
        control.UpdateHeight();
        control.InvalidateSurface();
    }

    private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DayTimetableView)bindable;
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= control.OnEventsCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += control.OnEventsCollectionChanged;
        }

        control.InvalidateSurface();
    }

    private void OnEventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateSurface();

    private void UpdateHeight()
    {
        HeightRequest = PaddingTop + ((float)Math.Max(0, (EndTime - StartTime).TotalHours) * HourHeight) + PaddingBottom;
    }

    private float TimeToY(TimeSpan time) => PaddingTop + ((float)(time - StartTime).TotalHours * HourHeight);

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((DayTimetableView)bindable).InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        eventRects.Clear();

        var scale = (float)(e.Info.Width / Width);
        if (!Single.IsFinite(scale) || (scale <= 0) || (EndTime <= StartTime))
        {
            return;
        }

        canvas.Scale(scale);
        Render(canvas, (float)Width);
    }

    private void Render(SKCanvas canvas, float width)
    {
        using var font = new SKFont(SocialFonts.NotoSerifJP, size: FontSize);
        using var boldFont = new SKFont(SocialFonts.NotoSerifJP, size: FontSize);
        boldFont.Embolden = true;

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        var areaLeft = TimeColumnWidth + TimeColumnSpacing;
        var areaRight = width - PaddingRight;

        RenderGrid(canvas, paint, font, areaRight);

        var events = Events ?? [];
        if (FreeSlotHighlightVisible)
        {
            RenderFreeSlots(canvas, paint, font, events, areaLeft, areaRight);
        }

        RenderEvents(canvas, paint, font, boldFont, events, areaLeft, areaRight);

        if (ShowCurrentTime && (CurrentTime >= StartTime) && (CurrentTime <= EndTime))
        {
            var y = TimeToY(CurrentTime);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = CurrentTimeWidth;
            paint.Color = CurrentTimeColor;
            canvas.DrawLine(TimeColumnWidth, y, areaRight, y, paint);

            paint.Style = SKPaintStyle.Fill;
            canvas.DrawCircle(TimeColumnWidth, y, CurrentTimeRadius, paint);
        }
    }

    private void RenderGrid(SKCanvas canvas, SKPaint paint, SKFont font, float areaRight)
    {
        var interval = TimeSlotInterval > TimeSpan.Zero ? TimeSlotInterval : TimeSpan.FromHours(1);
        for (var time = StartTime; time <= EndTime; time += interval)
        {
            var y = TimeToY(time);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = GridLineWidth;
            paint.Color = LineColor;
            canvas.DrawLine(TimeColumnWidth, y, areaRight, y, paint);

            if (time.Ticks % TimeSpan.TicksPerHour == 0)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = TimeTextColor;
                canvas.DrawText(GetHourText((int)time.TotalHours), TimeColumnWidth - TimeTextMargin, y + TimeTextOffset, SKTextAlign.Right, font, paint);
            }
        }
    }

    private void RenderFreeSlots(SKCanvas canvas, SKPaint paint, SKFont font, IReadOnlyList<TimetableEvent> events, float areaLeft, float areaRight)
    {
        foreach (var (freeStart, freeEnd) in TimetableCalculator.GetFreeSlots(events, StartTime, EndTime))
        {
            var y1 = TimeToY(freeStart);
            var y2 = TimeToY(freeEnd);
            paint.Style = SKPaintStyle.Fill;
            paint.Color = FreeFillColor;
            canvas.DrawRoundRect(new SKRect(areaLeft, y1 + FreeSlotInset, areaRight, y2 - FreeSlotInset), FreeSlotRadius, FreeSlotRadius, paint);

            if (freeEnd - freeStart >= MinimumFreeSlotForLabel)
            {
                paint.Color = FreeTextColor;
                canvas.DrawText($"空き {TimetableCalculator.FormatDuration(freeEnd - freeStart)}", areaLeft + FreeTextLeft, y1 + FreeTextTop, SKTextAlign.Left, font, paint);
            }
        }
    }

    private void RenderEvents(SKCanvas canvas, SKPaint paint, SKFont font, SKFont boldFont, IReadOnlyList<TimetableEvent> events, float areaLeft, float areaRight)
    {
        var sorted = events
            .Select(x => (Event: x, Start: x.Start < StartTime ? StartTime : x.Start, End: x.End > EndTime ? EndTime : x.End))
            .Where(static x => x.Start < x.End)
            .OrderBy(static x => x.Start)
            .ToList();
        if (sorted.Count == 0)
        {
            return;
        }

        var lanes = new List<TimeSpan>();
        var laneIndexes = new int[sorted.Count];
        for (var i = 0; i < sorted.Count; i++)
        {
            var lane = lanes.FindIndex(x => x <= sorted[i].Start);
            if (lane < 0)
            {
                lane = lanes.Count;
                lanes.Add(TimeSpan.Zero);
            }
            lanes[lane] = sorted[i].End;
            laneIndexes[i] = lane;
        }

        var laneWidth = (areaRight - areaLeft - ((lanes.Count - 1) * LaneSpacing)) / lanes.Count;

        using var cardPaint = new SKPaint();
        cardPaint.IsAntialias = true;
        cardPaint.Style = SKPaintStyle.Fill;
        cardPaint.Color = SKColors.White;
        cardPaint.ImageFilter = SKImageFilter.CreateDropShadow(0f, CardShadowOffset, CardShadowBlur, CardShadowBlur, CardShadowColor);

        for (var i = 0; i < sorted.Count; i++)
        {
            var (ev, start, end) = sorted[i];
            var x1 = areaLeft + (laneIndexes[i] * (laneWidth + LaneSpacing));
            var rect = new SKRect(x1, TimeToY(start) + CardInset, x1 + laneWidth, TimeToY(end) - CardInset);
            eventRects.Add((rect, ev));

            canvas.DrawRoundRect(rect, CardRadius, CardRadius, cardPaint);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = CardBorderWidth;
            paint.Color = CardBorderColor;
            canvas.DrawRoundRect(rect, CardRadius, CardRadius, paint);
            paint.Style = SKPaintStyle.Fill;

            var color = ev.Color.ToSKColor();
            paint.Color = color;
            canvas.DrawRoundRect(new SKRect(rect.Left + AccentInset, rect.Top + AccentInset, rect.Left + AccentInset + AccentWidth, Math.Max(rect.Top + AccentInset, rect.Bottom - AccentInset)), AccentRadius, AccentRadius, paint);

            canvas.Save();
            canvas.ClipRect(rect);
            var textLeft = rect.Left + TextLeft;
            var textRight = rect.Right - TextRight;
            var titleY = rect.Top + TextTop + LineHeight - BaselineOffset;

            var titleRight = textRight;
            if (rect.Width >= MinimumWidthForDuration)
            {
                var duration = TimetableCalculator.FormatDuration(end - start);
                paint.Color = color;
                canvas.DrawText(duration, textRight, titleY, SKTextAlign.Right, boldFont, paint);
                titleRight -= boldFont.MeasureText(duration) + DurationSpacing;
            }

            paint.Color = TitleColor;
            canvas.DrawText(TruncateText(ev.Title, boldFont, titleRight - textLeft), textLeft, titleY, SKTextAlign.Left, boldFont, paint);

            if (rect.Height >= TextTop + (LineHeight * 2) + TextTop)
            {
                var time = $"{ev.Start:hh\\:mm} - {ev.End:hh\\:mm}";
                var line = String.IsNullOrEmpty(ev.Place) ? time : $"{time}  {ev.Place}";
                paint.Color = TimeTextColor;
                canvas.DrawText(TruncateText(line, font, textRight - textLeft), textLeft, titleY + LineHeight, SKTextAlign.Left, font, paint);
            }

            canvas.Restore();
        }
    }

    private static string GetHourText(int hour)
    {
        if (!HourTexts.TryGetValue(hour, out var text))
        {
            text = $"{hour}:00";
            HourTexts[hour] = text;
        }

        return text;
    }

    private static string TruncateText(string text, SKFont font, float maxWidth)
    {
        if ((maxWidth <= 0) || (font.MeasureText(text) <= maxWidth))
        {
            return text;
        }

        var available = maxWidth - font.MeasureText(Ellipsis);
        for (var length = text.Length - 1; length > 0; length--)
        {
            if (font.MeasureText(text[..length]) <= available)
            {
                return text[..length] + Ellipsis;
            }
        }
        return Ellipsis;
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if ((EventTappedCommand is not { } command) || (e.GetPosition(this) is not { } position))
        {
            return;
        }

        var point = new SKPoint((float)position.X, (float)position.Y);
        for (var i = eventRects.Count - 1; i >= 0; i--)
        {
            if (eventRects[i].Rect.Contains(point))
            {
                command.Execute(eventRects[i].Event);
                return;
            }
        }
    }
}
