namespace Template.MobileApp.Services;

using ClamCalendar;

// カレンダー画面 (UICalendar / UISchedule) のイベント・スタンプ・祝日の供給元 (サンプルデータ生成器。VM からの new 直生成を避け DI 注入の見本とする)
public interface ICalendarService
{
    IReadOnlyList<CalendarEvent> GetEvents(DateOnly startDate, DateOnly endDate);

    IReadOnlyList<CalendarStamp> GetStamps(DateOnly startDate, DateOnly endDate);

    IReadOnlyList<DateOnly> GetHolidays(DateOnly startDate, DateOnly endDate);

    // 時刻のある予定 (Schedule のタイムテーブル)
    IReadOnlyList<TimetableEvent> GetSchedule(DateOnly day);
}

public sealed class CalendarService : ICalendarService
{
    // サンプルデータ生成の下限 (過去 2 ヶ月の月初より前は生成しない)
    private const int LimitMonths = 2;

    private static readonly Color DarkRed = Color.FromArgb("#8B1538");
    private static readonly Color HotPink = Color.FromArgb("#D81B60");
    private static readonly Color VividMagenta = Color.FromArgb("#C2185B");
    private static readonly Color Cyan = Color.FromArgb("#00ACC1");
    private static readonly Color Green = Color.FromArgb("#43A047");
    private static readonly Color GreenText = Color.FromArgb("#2E7D32");
    private static readonly Color PinkText = Color.FromArgb("#E91E63");
    private static readonly Color Yellow = Color.FromArgb("#FBC02D");
    private static readonly Color Orange = Color.FromArgb("#FB8C00");
    private static readonly Color Blue = Color.FromArgb("#1E88E5");
    private static readonly Color CyanText = Color.FromArgb("#00ACC1");
    private static readonly Color YellowText = Color.FromArgb("#F9A825");
    private static readonly Color Purple = Color.FromArgb("#8E24AA");
    private static readonly Color Teal = Color.FromArgb("#00897B");
    private static readonly Color Indigo = Color.FromArgb("#3949AB");
    private static readonly Color Red = Color.FromArgb("#E53935");
    private static readonly Color BlueGray = Color.FromArgb("#546E7A");
    private static readonly Color Amber = Color.FromArgb("#FFB300");

    private static readonly string[] MemberAvatars =
    [
        "avatar_person01.jpg",
        "avatar_person02.jpg",
        "avatar_person03.jpg",
        "avatar_person04.jpg",
        "avatar_person05.jpg"
    ];

    // 平日の予定。先頭 (朝会) は毎日入る
    private static readonly (TimeSpan Start, TimeSpan End, string Title, string Place, int Members, string Memo, Color Color)[] WorkScheduleTemplates =
    [
        (new(9, 0, 0), new(9, 30, 0), "朝会", "オンライン", 5, "今日の作業と、困っていることを共有する。", Blue),
        (new(10, 0, 0), new(11, 30, 0), "設計レビュー", "会議室 A", 4, "画面とモデルの分け方を確認する。資料は前日までに共有。", Indigo),
        (new(11, 0, 0), new(11, 30, 0), "1on1", "会議室 C", 2, "来期の目標と、勉強したい分野の相談。", Teal),
        (new(12, 0, 0), new(13, 0, 0), "ランチミーティング", "社員食堂", 3, "新しく入ったメンバーの歓迎会を兼ねる。", Orange),
        (new(13, 30, 0), new(15, 0, 0), "お客さまとの打ち合わせ", "渋谷オフィス", 4, "見積もりと日程を確認する。デモの端末を持っていく。", Red),
        (new(14, 0, 0), new(15, 0, 0), "採用面接", "会議室 B", 3, "エンジニアの 2 次面接。評価のシートを事前に読む。", HotPink),
        (new(15, 30, 0), new(16, 30, 0), "スプリント計画", "会議室 A", 6, "次の 2 週間で進める作業を決める。", Green),
        (new(16, 0, 0), new(17, 0, 0), "社内勉強会", "セミナールーム", 8, "MAUI の新しい機能の紹介と、質問の時間。", Amber),
        (new(17, 30, 0), new(18, 0, 0), "振り返り", "オンライン", 5, "今週の良かったことと、次に試すこと。", BlueGray)
    ];

    private static readonly (TimeSpan Start, TimeSpan End, string Title, string Place, int Members, string Memo, Color Color)[] HolidayScheduleTemplates =
    [
        (new(10, 0, 0), new(11, 30, 0), "ジム", "駅前のジム", 1, "体幹のトレーニングとストレッチ。", Cyan),
        (new(11, 30, 0), new(13, 0, 0), "ブランチ", "近所のカフェ", 2, "季節のパンケーキを食べる。", Orange),
        (new(14, 0, 0), new(16, 0, 0), "買い物", "ショッピングモール", 2, "冬物の服と日用品をまとめて買う。", Amber),
        (new(16, 30, 0), new(18, 30, 0), "映画", "シネマコンプレックス", 3, "話題のアニメ映画。席は予約済み。", Purple),
        (new(19, 0, 0), new(20, 0, 0), "夕食", "イタリアン", 4, "友だちの誕生日のお祝い。ケーキを頼んである。", Red)
    ];

    private static readonly (DayOfWeek Dow, string Title, CalendarEventStyle Style, Color Color)[] WeeklyTemplates =
    [
        (DayOfWeek.Monday, "週間報告", CalendarEventStyle.Text, GreenText),
        (DayOfWeek.Monday, "英会話", CalendarEventStyle.Text, PinkText),
        (DayOfWeek.Wednesday, "サークル", CalendarEventStyle.Text, YellowText),
        (DayOfWeek.Saturday, "水泳教室", CalendarEventStyle.Text, CyanText)
    ];

    private static readonly (int DayOffset, string Title, int Span, CalendarEventStyle Style, Color Bg, Color? Fg)[] MonthlyTemplates =
    [
        (3,  "燃えるゴ", 1, CalendarEventStyle.Filled, DarkRed, null),
        (10, "燃えるゴ", 1, CalendarEventStyle.Filled, DarkRed, null),
        (17, "燃えるゴ", 1, CalendarEventStyle.Filled, DarkRed, null),
        (24, "燃えるゴ", 1, CalendarEventStyle.Filled, DarkRed, null),
        (5,  "○ジム",   1, CalendarEventStyle.Filled, Cyan, null),
        (19, "○ジム",   1, CalendarEventStyle.Filled, Cyan, null)
    ];

    private static readonly (int DayOffset, string Glyph, CalendarStampPosition Position, float FontSize, float Opacity)[] StampTemplates =
    [
        (3,  "\U0001F6A9", CalendarStampPosition.TopRight,  22, 1.0f),
        (8,  "\U0001F426", CalendarStampPosition.TopRight,  22, 1.0f),
        (13, "✈️",         CalendarStampPosition.Center,    26, 1.0f),
        (16, "\U0001F436", CalendarStampPosition.Center,    32, 0.9f),
        (20, "\U0001F45B", CalendarStampPosition.TopCenter, 22, 1.0f),
        (22, "\U0001F43C", CalendarStampPosition.TopLeft,   22, 1.0f),
        (26, "\U0001F38F", CalendarStampPosition.TopCenter, 22, 1.0f),
        (29, "\U0001F408", CalendarStampPosition.TopRight,  24, 1.0f)
    ];

    private static readonly (int DayOffset, string Title, int Span, CalendarEventStyle Style, Color Bg, Color? Fg)[] OccasionalTemplates =
    [
        (2,  "ぶどう狩",   1, CalendarEventStyle.Filled, VividMagenta, null),
        (6,  "会社研修",   2, CalendarEventStyle.Filled, Green,        null),
        (9,  "温泉旅行",   1, CalendarEventStyle.Filled, Orange,       null),
        (12, "大阪出張",   2, CalendarEventStyle.Text,   Blue,         Blue),
        (14, "友達泊まり", 3, CalendarEventStyle.Filled, HotPink,      null),
        (21, "買い物",     1, CalendarEventStyle.Filled, Yellow,       Colors.Black),
        (26, "海外出張",   4, CalendarEventStyle.Filled, Blue,         null)
    ];

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    public IReadOnlyList<CalendarEvent> GetEvents(DateOnly startDate, DateOnly endDate)
    {
        if (IsBeforeLimit(startDate))
        {
            return Array.Empty<CalendarEvent>();
        }

        var events = new List<CalendarEvent>();
        var months = EnumerateMonths(startDate, endDate);
        var idx = 0;

        foreach (var (year, month) in months)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);

            foreach (var (dow, title, style, color) in WeeklyTemplates)
            {
                for (var day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);
                    if (date.DayOfWeek == dow)
                    {
                        var ev = CreateEvent($"w{idx++:D4}", title, date, date, style, Colors.Transparent, color);
                        if ((ev.StartDate <= endDate) && (ev.EndDate >= startDate))
                        {
                            events.Add(ev);
                        }
                    }
                }
            }

            foreach (var (offset, title, span, style, bg, fg) in MonthlyTemplates)
            {
                var day = Math.Min(offset, daysInMonth);
                var evStart = new DateOnly(year, month, day);
                var evEnd = evStart.AddDays(span - 1);
                if (evEnd.Month != month)
                {
                    evEnd = new DateOnly(year, month, daysInMonth);
                }

                if ((evStart <= endDate) && (evEnd >= startDate))
                {
                    events.Add(CreateEvent($"m{idx++:D4}", title, evStart, evEnd, style, bg, fg ?? Colors.White));
                }
            }

            var occCount = OccasionalTemplates.Length;
            var pickCount = 3 + (month % 3);
            for (var i = 0; i < pickCount; i++)
            {
                var t = OccasionalTemplates[(month + (i * 3)) % occCount];
                var day = Math.Min(t.DayOffset, daysInMonth);
                var evStart = new DateOnly(year, month, day);
                var evEnd = evStart.AddDays(t.Span - 1);
                if (evEnd.Month != month)
                {
                    evEnd = new DateOnly(year, month, daysInMonth);
                }

                if ((evStart <= endDate) && (evEnd >= startDate))
                {
                    events.Add(CreateEvent($"o{idx++:D4}", t.Title, evStart, evEnd, t.Style, t.Bg, t.Fg ?? Colors.White));
                }
            }
        }

        return events.OrderBy(e => e.StartDate).ToList();
    }

    public IReadOnlyList<CalendarStamp> GetStamps(DateOnly startDate, DateOnly endDate)
    {
        if (IsBeforeLimit(startDate))
        {
            return Array.Empty<CalendarStamp>();
        }

        var stamps = new List<CalendarStamp>();
        var months = EnumerateMonths(startDate, endDate);
        var idx = 0;

        foreach (var (year, month) in months)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var pickCount = 4 + (month % 4);
            for (var i = 0; i < pickCount; i++)
            {
                var t = StampTemplates[(month + (i * 2)) % StampTemplates.Length];
                var day = Math.Min(t.DayOffset, daysInMonth);
                var date = new DateOnly(year, month, day);
                if ((date >= startDate) && (date <= endDate))
                {
                    stamps.Add(new CalendarStamp
                    {
                        Key = $"s{idx++:D4}",
                        Date = date,
                        Glyph = t.Glyph,
                        Position = t.Position,
                        FontSize = t.FontSize,
                        Opacity = t.Opacity
                    });
                }
            }
        }

        return stamps;
    }

    private static bool IsBeforeLimit(DateOnly start)
    {
        var limit = DateOnly.FromDateTime(DateTime.Today).AddMonths(-LimitMonths);
        return start < new DateOnly(limit.Year, limit.Month, 1);
    }

    private static IEnumerable<(int Year, int Month)> EnumerateMonths(DateOnly startDate, DateOnly endDate)
    {
        var current = new DateOnly(startDate.Year, startDate.Month, 1);
        var last = new DateOnly(endDate.Year, endDate.Month, 1);
        while (current <= last)
        {
            yield return (current.Year, current.Month);
            current = current.AddMonths(1);
        }
    }

    private static CalendarEvent CreateEvent(
        string key,
        string title,
        DateOnly startDate,
        DateOnly endDate,
        CalendarEventStyle style,
        Color bg,
        Color fg) =>
        new()
        {
            Key = key,
            Title = title,
            StartDate = startDate,
            EndDate = endDate,
            Style = style,
            BackgroundColor = bg,
            TextColor = fg
        };

    //--------------------------------------------------------------------------------
    // Holiday
    //--------------------------------------------------------------------------------

    public IReadOnlyList<DateOnly> GetHolidays(DateOnly startDate, DateOnly endDate)
    {
        if (IsBeforeLimit(startDate))
        {
            return Array.Empty<DateOnly>();
        }

        var holidays = new List<DateOnly>();
        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
            foreach (var d in GetYearHolidays(year))
            {
                if ((d >= startDate) && (d <= endDate))
                {
                    holidays.Add(d);
                }
            }
        }
        holidays.Sort();
        return holidays;
    }

    private static HashSet<DateOnly> GetYearHolidays(int year)
    {
        var set = new HashSet<DateOnly>
        {
            new(year, 1, 1),
            NthWeekday(year, 1, DayOfWeek.Monday, 2),
            new(year, 2, 11),
            new(year, 3, SpringEquinox(year)),
            new(year, 4, 29),
            new(year, 5, 3),
            new(year, 5, 4),
            new(year, 5, 5),
            NthWeekday(year, 7, DayOfWeek.Monday, 3),
            NthWeekday(year, 9, DayOfWeek.Monday, 3),
            new(year, 9, AutumnEquinox(year)),
            NthWeekday(year, 10, DayOfWeek.Monday, 2),
            new(year, 11, 3),
            new(year, 11, 23)
        };
        if (year >= 2020)
        {
            set.Add(new(year, 2, 23));
        }

        if (year >= 2016)
        {
            set.Add(new(year, 8, 11));
        }

        if (year <= 2018)
        {
            set.Add(new(year, 12, 23));
        }

        // 振替休日
        var substitutes = new List<DateOnly>();
        foreach (var h in set)
        {
            if (h.DayOfWeek == DayOfWeek.Sunday)
            {
                var substitute = h.AddDays(1);
                if (!set.Contains(substitute))
                {
                    substitutes.Add(substitute);
                }
            }
        }
        foreach (var s in substitutes)
        {
            set.Add(s);
        }

        // 国民の休日(挟まれ日)
        var sandwiched = new List<DateOnly>();
        foreach (var h in set)
        {
            var candidate = h.AddDays(1);
            if (!set.Contains(candidate) &&
                (candidate.DayOfWeek != DayOfWeek.Sunday) &&
                (candidate.DayOfWeek != DayOfWeek.Saturday) &&
                set.Contains(candidate.AddDays(1)))
            {
                sandwiched.Add(candidate);
            }
        }
        foreach (var s in sandwiched)
        {
            set.Add(s);
        }

        return set;
    }

    private static DateOnly NthWeekday(int year, int month, DayOfWeek dow, int n)
    {
        var first = new DateOnly(year, month, 1);
        var offset = ((int)dow - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(offset + ((n - 1) * 7));
    }

    private static int SpringEquinox(int year)
    {
        var x = year - 1980;
        return (int)(20.69115 + (0.242194 * x) - Math.Floor(x / 4.0));
    }

    private static int AutumnEquinox(int year)
    {
        var x = year - 1980;
        return (int)(23.09 + (0.242194 * x) - Math.Floor(x / 4.0));
    }

    //--------------------------------------------------------------------------------
    // Schedule
    //--------------------------------------------------------------------------------

    // 日付で決まる予定 (平日は朝会と、ほかの約半分。土日と祝日は 3 件)
    public IReadOnlyList<TimetableEvent> GetSchedule(DateOnly day)
    {
        var seed = day.DayNumber;
        var holiday = (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) || GetYearHolidays(day.Year).Contains(day);
        var templates = holiday
            ? HolidayScheduleTemplates.Where((_, i) => ((i + seed) % HolidayScheduleTemplates.Length) < 3)
            : WorkScheduleTemplates.Where((_, i) => (i == 0) || (((i + seed) % 2) == 0));
        return templates
            .Select((x, i) => new TimetableEvent
            {
                Title = x.Title,
                Start = x.Start,
                End = x.End,
                Color = x.Color,
                Place = x.Place,
                Members = Enumerable.Range(0, x.Members).Select(m => MemberAvatars[(seed + i + m) % MemberAvatars.Length]).ToArray(),
                Memo = x.Memo
            })
            .ToList();
    }
}
