namespace Template.MobileApp.Controls;

using System.Windows.Input;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.Sample.Calendar;

// 1 日分のタイムテーブルを描画するコントロール (時間範囲 / 罫線の間隔 / 空き時間帯 / 現在時刻ライン / イベントのタップ)
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

    private static readonly SKColor LineColor = new(0xE0, 0xE0, 0xE0);
    private static readonly SKColor TimeTextColor = new(0x9E, 0x9E, 0x9E);
    private static readonly SKColor CurrentTimeColor = new(0xE5, 0x39, 0x35);
    private static readonly SKColor CardBorderColor = new(0xEE, 0xEE, 0xEE);
    private static readonly SKColor TitleColor = new(0x42, 0x42, 0x42);
    private static readonly SKColor FreeFillColor = new(0x43, 0xA0, 0x47, 0x12);
    private static readonly SKColor FreeTextColor = new(0x66, 0xBB, 0x6A);

    // タップ判定用 (描画のたびに作り直す。後のレーンが上に描かれるので逆順に探す)
    private readonly List<(SKRect Rect, TimetableEvent Event)> eventRects = [];

    public static readonly BindableProperty HourHeightProperty = BindableProperty.Create(
        nameof(HourHeight),
        typeof(float),
        typeof(DayTimetableView),
        60f,
        propertyChanged: OnLayoutChanged);

    public float HourHeight
    {
        get => (float)GetValue(HourHeightProperty);
        set => SetValue(HourHeightProperty, value);
    }

    public static readonly BindableProperty StartTimeProperty = BindableProperty.Create(
        nameof(StartTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromHours(8),
        propertyChanged: OnLayoutChanged);

    public TimeSpan StartTime
    {
        get => (TimeSpan)GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public static readonly BindableProperty EndTimeProperty = BindableProperty.Create(
        nameof(EndTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromHours(20),
        propertyChanged: OnLayoutChanged);

    public TimeSpan EndTime
    {
        get => (TimeSpan)GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }

    // 罫線の間隔 (時刻の文字は正時だけ)
    public static readonly BindableProperty TimeSlotIntervalProperty = BindableProperty.Create(
        nameof(TimeSlotInterval),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromMinutes(30),
        propertyChanged: Invalidate);

    public TimeSpan TimeSlotInterval
    {
        get => (TimeSpan)GetValue(TimeSlotIntervalProperty);
        set => SetValue(TimeSlotIntervalProperty, value);
    }

    public static readonly BindableProperty EventsProperty = BindableProperty.Create(
        nameof(Events),
        typeof(IReadOnlyList<TimetableEvent>),
        typeof(DayTimetableView),
        propertyChanged: Invalidate);

    public IReadOnlyList<TimetableEvent>? Events
    {
        get => (IReadOnlyList<TimetableEvent>?)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public static readonly BindableProperty CurrentTimeProperty = BindableProperty.Create(
        nameof(CurrentTime),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.Zero,
        propertyChanged: Invalidate);

    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public static readonly BindableProperty ShowCurrentTimeProperty = BindableProperty.Create(
        nameof(ShowCurrentTime),
        typeof(bool),
        typeof(DayTimetableView),
        false,
        propertyChanged: Invalidate);

    public bool ShowCurrentTime
    {
        get => (bool)GetValue(ShowCurrentTimeProperty);
        set => SetValue(ShowCurrentTimeProperty, value);
    }

    public static readonly BindableProperty FreeSlotHighlightVisibleProperty = BindableProperty.Create(
        nameof(FreeSlotHighlightVisible),
        typeof(bool),
        typeof(DayTimetableView),
        true,
        propertyChanged: Invalidate);

    public bool FreeSlotHighlightVisible
    {
        get => (bool)GetValue(FreeSlotHighlightVisibleProperty);
        set => SetValue(FreeSlotHighlightVisibleProperty, value);
    }

    // 空き時間帯にラベルを出す下限
    public static readonly BindableProperty MinimumFreeSlotForLabelProperty = BindableProperty.Create(
        nameof(MinimumFreeSlotForLabel),
        typeof(TimeSpan),
        typeof(DayTimetableView),
        TimeSpan.FromMinutes(45),
        propertyChanged: Invalidate);

    public TimeSpan MinimumFreeSlotForLabel
    {
        get => (TimeSpan)GetValue(MinimumFreeSlotForLabelProperty);
        set => SetValue(MinimumFreeSlotForLabelProperty, value);
    }

    public static readonly BindableProperty EventTappedCommandProperty = BindableProperty.Create(
        nameof(EventTappedCommand),
        typeof(ICommand),
        typeof(DayTimetableView));

    public ICommand? EventTappedCommand
    {
        get => (ICommand?)GetValue(EventTappedCommandProperty);
        set => SetValue(EventTappedCommandProperty, value);
    }

    public DayTimetableView()
    {
        BackgroundColor = Colors.White;
        UpdateHeight();

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
    }

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((DayTimetableView)bindable).InvalidateSurface();
    }

    private static void OnLayoutChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DayTimetableView)bindable;
        control.UpdateHeight();
        control.InvalidateSurface();
    }

    private void UpdateHeight()
    {
        HeightRequest = PaddingTop + ((float)Math.Max(0, (EndTime - StartTime).TotalHours) * HourHeight) + PaddingBottom;
    }

    private float TimeToY(TimeSpan time) => PaddingTop + ((float)(time - StartTime).TotalHours * HourHeight);

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
        // SKTypeface.Default は日本語グリフを持たないためアプリ内蔵フォントを使う
        using var font = new SKFont(SocialFonts.NotoSerifJP, size: 12f);
        using var boldFont = new SKFont(SocialFonts.NotoSerifJP, size: 12f);
        boldFont.Embolden = true;

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        var areaLeft = TimeColumnWidth + 8f;
        var areaRight = width - PaddingRight;

        RenderGrid(canvas, paint, font, areaRight);

        IEnumerable<TimetableEvent> events = Events ?? [];
        if (FreeSlotHighlightVisible)
        {
            RenderFreeSlots(canvas, paint, font, events, areaLeft, areaRight);
        }

        RenderEvents(canvas, paint, font, boldFont, events, areaLeft, areaRight);

        if (ShowCurrentTime && (CurrentTime >= StartTime) && (CurrentTime <= EndTime))
        {
            var y = TimeToY(CurrentTime);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2f;
            paint.Color = CurrentTimeColor;
            canvas.DrawLine(TimeColumnWidth, y, areaRight, y, paint);

            paint.Style = SKPaintStyle.Fill;
            canvas.DrawCircle(TimeColumnWidth, y, 5f, paint);
        }
    }

    private void RenderGrid(SKCanvas canvas, SKPaint paint, SKFont font, float areaRight)
    {
        var interval = TimeSlotInterval > TimeSpan.Zero ? TimeSlotInterval : TimeSpan.FromHours(1);
        for (var time = StartTime; time <= EndTime; time += interval)
        {
            var y = TimeToY(time);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1f;
            paint.Color = LineColor;
            canvas.DrawLine(TimeColumnWidth, y, areaRight, y, paint);

            if (time.Ticks % TimeSpan.TicksPerHour == 0)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = TimeTextColor;
                canvas.DrawText($"{(int)time.TotalHours}:00", TimeColumnWidth - 8f, y + 4f, SKTextAlign.Right, font, paint);
            }
        }
    }

    private void RenderFreeSlots(SKCanvas canvas, SKPaint paint, SKFont font, IEnumerable<TimetableEvent> events, float areaLeft, float areaRight)
    {
        foreach (var (freeStart, freeEnd) in TimetableCalculator.GetFreeSlots(events, StartTime, EndTime))
        {
            var y1 = TimeToY(freeStart);
            var y2 = TimeToY(freeEnd);
            paint.Style = SKPaintStyle.Fill;
            paint.Color = FreeFillColor;
            canvas.DrawRoundRect(new SKRect(areaLeft, y1 + 1f, areaRight, y2 - 1f), 6f, 6f, paint);

            if (freeEnd - freeStart >= MinimumFreeSlotForLabel)
            {
                paint.Color = FreeTextColor;
                canvas.DrawText($"空き {TimetableCalculator.FormatDuration(freeEnd - freeStart)}", areaLeft + 8f, y1 + 16f, SKTextAlign.Left, font, paint);
            }
        }
    }

    private void RenderEvents(SKCanvas canvas, SKPaint paint, SKFont font, SKFont boldFont, IEnumerable<TimetableEvent> events, float areaLeft, float areaRight)
    {
        // 範囲へクランプし、開始順のグリーディ法でレーンを割り付ける
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

        // カードの持ち上げ影 (白地 + ドロップシャドウ)
        using var cardPaint = new SKPaint();
        cardPaint.IsAntialias = true;
        cardPaint.Style = SKPaintStyle.Fill;
        cardPaint.Color = SKColors.White;
        cardPaint.ImageFilter = SKImageFilter.CreateDropShadow(0f, 2f, 3f, 3f, new SKColor(0x00, 0x00, 0x00, 0x30));

        for (var i = 0; i < sorted.Count; i++)
        {
            var (ev, start, end) = sorted[i];
            var x1 = areaLeft + (laneIndexes[i] * (laneWidth + LaneSpacing));
            var rect = new SKRect(x1, TimeToY(start) + 2f, x1 + laneWidth, TimeToY(end) - 2f);
            eventRects.Add((rect, ev));

            // カード地
            canvas.DrawRoundRect(rect, 8f, 8f, cardPaint);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1f;
            paint.Color = CardBorderColor;
            canvas.DrawRoundRect(rect, 8f, 8f, paint);
            paint.Style = SKPaintStyle.Fill;

            // 左端のアクセントバー
            var color = ev.Color.ToSKColor();
            paint.Color = color;
            canvas.DrawRoundRect(new SKRect(rect.Left + 4f, rect.Top + 4f, rect.Left + 8f, Math.Max(rect.Top + 4f, rect.Bottom - 4f)), 2f, 2f, paint);

            // 文字はカードの中に収める
            canvas.Save();
            canvas.ClipRect(rect);
            var textLeft = rect.Left + 14f;
            var textRight = rect.Right - 8f;
            var titleY = rect.Top + TextTop + LineHeight - 4f;

            // 所要時間ラベル (レーンが狭いときは省く)
            var titleRight = textRight;
            if (rect.Width >= MinimumWidthForDuration)
            {
                var duration = TimetableCalculator.FormatDuration(end - start);
                paint.Color = color;
                canvas.DrawText(duration, textRight, titleY, SKTextAlign.Right, boldFont, paint);
                titleRight -= boldFont.MeasureText(duration) + 8f;
            }

            paint.Color = TitleColor;
            canvas.DrawText(TruncateText(ev.Title, boldFont, titleRight - textLeft), textLeft, titleY, SKTextAlign.Left, boldFont, paint);

            // 2 行入らない高さのカードでは時刻行を省く
            if (rect.Height >= TextTop + (LineHeight * 2) + TextTop)
            {
                paint.Color = TimeTextColor;
                canvas.DrawText($"{ev.Start:hh\\:mm} - {ev.End:hh\\:mm}", textLeft, titleY + LineHeight, SKTextAlign.Left, font, paint);
            }

            canvas.Restore();
        }
    }

    private static string TruncateText(string text, SKFont font, float maxWidth)
    {
        if ((maxWidth <= 0) || (font.MeasureText(text) <= maxWidth))
        {
            return text;
        }

        const string ellipsis = "…";
        var available = maxWidth - font.MeasureText(ellipsis);
        for (var length = text.Length - 1; length > 0; length--)
        {
            if (font.MeasureText(text[..length]) <= available)
            {
                return text[..length] + ellipsis;
            }
        }
        return ellipsis;
    }
}
