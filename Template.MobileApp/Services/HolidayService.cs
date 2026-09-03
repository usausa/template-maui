namespace Template.MobileApp.Services;

public sealed class HolidayService
{
#pragma warning disable CA1822
    public IReadOnlyList<DateOnly> GetHolidays(DateOnly start, DateOnly end)
    {
        if (SampleDataBoundary.IsBeforeLimit(start))
        {
            return Array.Empty<DateOnly>();
        }

        var holidays = new List<DateOnly>();
        for (var year = start.Year; year <= end.Year; year++)
        {
            foreach (var d in GetYearHolidays(year))
            {
                if ((d >= start) && (d <= end))
                {
                    holidays.Add(d);
                }
            }
        }
        holidays.Sort();
        return holidays;
    }
#pragma warning restore CA1822

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
}
