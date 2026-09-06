namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.Sample.Calendar;

// 1 日分のタイムテーブル(8:00-20:00)を描画するコントロール
public sealed class DayTimetableView : SKCanvasView
{
    private const int StartHour = 8;
    private const int EndHour = 20;
    private const float TimeColumnWidth = 56f;
    private const float PaddingTop = 16f;
    private const float PaddingBottom = 16f;
    private const float PaddingRight = 12f;
    private const float LaneSpacing = 4f;

    private static readonly SKColor LineColor = new(0xE0, 0xE0, 0xE0);
    private static readonly SKColor TimeTextColor = new(0x9E, 0x9E, 0x9E);
    private static readonly SKColor CurrentTimeColor = new(0xE5, 0x39, 0x35);
    private static readonly SKColor CardBorderColor = new(0xEE, 0xEE, 0xEE);
    private static readonly SKColor FreeFillColor = new(0x43, 0xA0, 0x47, 0x12);
    private static readonly SKColor FreeTextColor = new(0x66, 0xBB, 0x6A);

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

    public DayTimetableView()
    {
        BackgroundColor = Colors.White;
        UpdateHeight();
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
        HeightRequest = PaddingTop + ((EndHour - StartHour) * HourHeight) + PaddingBottom;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        var scale = (float)(e.Info.Width / Width);
        if (!Single.IsFinite(scale) || (scale <= 0))
        {
            return;
        }

        canvas.Scale(scale);
        var width = (float)Width;
        Render(canvas, width);
    }

    private void Render(SKCanvas canvas, float width)
    {
        var hourHeight = HourHeight;

        // SKTypeface.Default は日本語グリフを持たないためアプリ内蔵フォントを使う
        using var font = new SKFont(SocialFonts.NotoSerifJP, size: 12f);
        using var boldFont = new SKFont(SocialFonts.NotoSerifJP, size: 12f);
        boldFont.Embolden = true;

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        // 時刻罫線
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1f;
        for (var hour = StartHour; hour <= EndHour; hour++)
        {
            var y = PaddingTop + ((hour - StartHour) * hourHeight);
            paint.Color = LineColor;
            canvas.DrawLine(TimeColumnWidth, y, width - PaddingRight, y, paint);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = TimeTextColor;
            canvas.DrawText($"{hour}:00", TimeColumnWidth - 8f, y + 4f, SKTextAlign.Right, font, paint);
            paint.Style = SKPaintStyle.Stroke;
        }

        var areaLeft = TimeColumnWidth + 8f;
        var areaRight = width - PaddingRight;

        // 空き時間帯のハイライト (イベントの隙間を薄い緑で塗る)
        var events = Events;
        foreach (var (freeStart, freeEnd) in TimetableCalculator.GetFreeSlots(events ?? [], TimeSpan.FromHours(StartHour), TimeSpan.FromHours(EndHour)))
        {
            var y1 = PaddingTop + ((float)(freeStart.TotalHours - StartHour) * hourHeight);
            var y2 = PaddingTop + ((float)(freeEnd.TotalHours - StartHour) * hourHeight);
            paint.Style = SKPaintStyle.Fill;
            paint.Color = FreeFillColor;
            canvas.DrawRoundRect(new SKRect(areaLeft, y1 + 1f, areaRight, y2 - 1f), 6f, 6f, paint);

            // 45 分以上の隙間にはラベルを添える
            if (freeEnd - freeStart >= TimeSpan.FromMinutes(45))
            {
                paint.Color = FreeTextColor;
                canvas.DrawText($"空き {TimetableCalculator.FormatDuration(freeEnd - freeStart)}", areaLeft + 8f, y1 + 16f, SKTextAlign.Left, font, paint);
            }
        }

        // イベント(開始順のグリーディ法でレーン割付)
        if (events is { Count: > 0 })
        {
            var sorted = events.OrderBy(static x => x.Start).ToList();
            var lanes = new List<TimeSpan>();
            var assignments = new List<(TimetableEvent Event, int Lane)>();
            foreach (var ev in sorted)
            {
                var lane = -1;
                for (var i = 0; i < lanes.Count; i++)
                {
                    if (lanes[i] <= ev.Start)
                    {
                        lane = i;
                        break;
                    }
                }
                if (lane < 0)
                {
                    lane = lanes.Count;
                    lanes.Add(TimeSpan.Zero);
                }
                lanes[lane] = ev.End;
                assignments.Add((ev, lane));
            }

            var laneCount = Math.Max(1, lanes.Count);
            var areaWidth = areaRight - areaLeft;
            var laneWidth = (areaWidth - ((laneCount - 1) * LaneSpacing)) / laneCount;

            // カードの持ち上げ影 (白地 + ドロップシャドウ)
            using var cardPaint = new SKPaint();
            cardPaint.IsAntialias = true;
            cardPaint.Style = SKPaintStyle.Fill;
            cardPaint.Color = SKColors.White;
            cardPaint.ImageFilter = SKImageFilter.CreateDropShadow(0f, 2f, 3f, 3f, new SKColor(0x00, 0x00, 0x00, 0x30));

            paint.Style = SKPaintStyle.Fill;
            foreach (var (ev, lane) in assignments)
            {
                var y1 = PaddingTop + ((float)(ev.Start.TotalHours - StartHour) * hourHeight);
                var y2 = PaddingTop + ((float)(ev.End.TotalHours - StartHour) * hourHeight);
                var x1 = areaLeft + (lane * (laneWidth + LaneSpacing));
                var rect = new SKRect(x1, y1 + 2f, x1 + laneWidth, y2 - 2f);

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
                canvas.DrawRoundRect(new SKRect(rect.Left + 4f, rect.Top + 4f, rect.Left + 8f, rect.Bottom - 4f), 2f, 2f, paint);

                // 所要時間ラベル (レーンが狭いときは省く)
                if (rect.Width >= 110f)
                {
                    paint.Color = color;
                    canvas.DrawText(TimetableCalculator.FormatDuration(ev.End - ev.Start), rect.Right - 8f, rect.Top + 18f, SKTextAlign.Right, boldFont, paint);
                }

                paint.Color = new SKColor(0x42, 0x42, 0x42);
                canvas.DrawText(ev.Title, rect.Left + 14f, rect.Top + 18f, SKTextAlign.Left, boldFont, paint);
                paint.Color = TimeTextColor;
                canvas.DrawText($"{ev.Start:hh\\:mm} - {ev.End:hh\\:mm}", rect.Left + 14f, rect.Top + 34f, SKTextAlign.Left, font, paint);
            }
        }

        // 現在時刻ライン
        if (ShowCurrentTime)
        {
            var hours = CurrentTime.TotalHours;
            if (hours is >= StartHour and <= EndHour)
            {
                var y = PaddingTop + ((float)(hours - StartHour) * hourHeight);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 2f;
                paint.Color = CurrentTimeColor;
                canvas.DrawLine(TimeColumnWidth, y, width - PaddingRight, y, paint);

                paint.Style = SKPaintStyle.Fill;
                canvas.DrawCircle(TimeColumnWidth, y, 5f, paint);
            }
        }
    }
}
