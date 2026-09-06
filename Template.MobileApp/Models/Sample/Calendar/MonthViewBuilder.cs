namespace Template.MobileApp.Models.Sample.Calendar;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
public sealed class MonthViewBuilder(DayOfWeek weekStartDayOfWeek = DayOfWeek.Monday)
{
    private const int DaysPerWeek = 7;
    private const int WeeksPerMonth = 6;

    public (DateOnly Start, DateOnly End) GetDisplayRange(int year, int month)
    {
        var monthFirst = new DateOnly(year, month, 1);
        var firstDayToShow = AlignToWeekStart(monthFirst);
        var lastDayToShow = firstDayToShow.AddDays((WeeksPerMonth * DaysPerWeek) - 1);
        return (firstDayToShow, lastDayToShow);
    }

    public MonthView Build(
        int year,
        int month,
        DateOnly today,
        IReadOnlyList<ScheduleEvent> events,
        IReadOnlyList<Stamp> stamps,
        IReadOnlyList<DateOnly> holidays)
    {
        var (firstDayToShow, _) = GetDisplayRange(year, month);
        var holidaySet = holidays.Count == 0 ? null : new HashSet<DateOnly>(holidays);
        var stampLookup = CreateStampLookup(stamps);
        var weeklyCandidates = CreateWeeklyEventCandidates(firstDayToShow, events);

        var weeks = new List<WeekView>(capacity: WeeksPerMonth);
        for (var w = 0; w < WeeksPerMonth; w++)
        {
            var weekStart = firstDayToShow.AddDays(w * DaysPerWeek);

            var days = new List<DayView>(capacity: DaysPerWeek);
            for (var d = 0; d < DaysPerWeek; d++)
            {
                var date = weekStart.AddDays(d);
                days.Add(new DayView
                {
                    Date = date,
                    IsCurrentMonth = (date.Year == year) && (date.Month == month),
                    IsToday = date == today,
                    Kind = DetermineKind(date, holidaySet),
                    Stamps = stampLookup.TryGetValue(date, out var s) ? s : []
                });
            }

            var placements = AssignPlacements(weeklyCandidates[w]);
            var slotCount = GetSlotCount(placements);

            weeks.Add(new WeekView
            {
                Days = days,
                EventPlacements = placements,
                SlotCount = slotCount
            });
        }

        return new MonthView
        {
            Year = year,
            Month = month,
            Today = today,
            Weeks = weeks
        };
    }

    private DateOnly AlignToWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)weekStartDayOfWeek + 7) % 7;
        return date.AddDays(-diff);
    }

    private static Dictionary<DateOnly, List<Stamp>> CreateStampLookup(IReadOnlyList<Stamp> stamps)
    {
        var lookup = new Dictionary<DateOnly, List<Stamp>>();
        foreach (var stamp in stamps)
        {
            if (!lookup.TryGetValue(stamp.Date, out var list))
            {
                list = [];
                lookup[stamp.Date] = list;
            }
            list.Add(stamp);
        }
        return lookup;
    }

    private static DayKind DetermineKind(DateOnly date, HashSet<DateOnly>? holidays)
    {
        if ((holidays is not null) && holidays.Contains(date))
        {
            return DayKind.Holiday;
        }

        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => DayKind.Sunday,
            DayOfWeek.Saturday => DayKind.Saturday,
            _ => DayKind.Weekday
        };
    }

    private static List<EventCandidate>[] CreateWeeklyEventCandidates(DateOnly firstDayToShow, IReadOnlyList<ScheduleEvent> events)
    {
        var weeklyCandidates = new List<EventCandidate>[WeeksPerMonth];
        for (var i = 0; i < WeeksPerMonth; i++)
        {
            weeklyCandidates[i] = [];
        }

        foreach (var evEvent in events)
        {
            var firstWeek = Math.Max(0, (evEvent.StartDate.DayNumber - firstDayToShow.DayNumber) / DaysPerWeek);
            var lastWeek = Math.Min(WeeksPerMonth - 1, (evEvent.EndDate.DayNumber - firstDayToShow.DayNumber) / DaysPerWeek);
            for (var weekIndex = firstWeek; weekIndex <= lastWeek; weekIndex++)
            {
                var weekStart = firstDayToShow.AddDays(weekIndex * DaysPerWeek);
                var weekEnd = weekStart.AddDays(DaysPerWeek - 1);
                var clippedStart = evEvent.StartDate < weekStart ? weekStart : evEvent.StartDate;
                var clippedEnd = evEvent.EndDate > weekEnd ? weekEnd : evEvent.EndDate;
                weeklyCandidates[weekIndex].Add(new EventCandidate(
                    evEvent,
                    clippedStart.DayNumber - weekStart.DayNumber,
                    clippedEnd.DayNumber - weekStart.DayNumber,
                    evEvent.StartDate < weekStart,
                    evEvent.EndDate > weekEnd));
            }
        }

        foreach (var candidate in weeklyCandidates)
        {
            candidate.Sort(static (left, right) =>
            {
                var startComparison = left.StartCol.CompareTo(right.StartCol);
                if (startComparison != 0)
                {
                    return startComparison;
                }

                return (right.EndCol - right.StartCol).CompareTo(left.EndCol - left.StartCol);
            });
        }

        return weeklyCandidates;
    }

    private static List<EventPlacement> AssignPlacements(List<EventCandidate> candidates)
    {
        Span<int> slotOccupancy = stackalloc int[16];
        var slotCount = 0;
        var placements = new List<EventPlacement>(capacity: candidates.Count);

        foreach (var candidate in candidates)
        {
            var mask = ((1 << ((candidate.EndCol - candidate.StartCol) + 1)) - 1) << candidate.StartCol;
            var slot = FindAvailableSlot(slotOccupancy[..slotCount], mask);
            if (slot >= slotCount)
            {
                slotCount++;
            }

            slotOccupancy[slot] |= mask;

            placements.Add(new EventPlacement
            {
                Event = candidate.Event,
                StartColumn = candidate.StartCol,
                ColumnSpan = (candidate.EndCol - candidate.StartCol) + 1,
                Slot = slot,
                ContinuesFromPreviousWeek = candidate.ContinuesFromPrev,
                ContinuesToNextWeek = candidate.ContinuesToNext
            });
        }

        return placements;
    }

    private static int FindAvailableSlot(ReadOnlySpan<int> occupancy, int mask)
    {
        for (var slot = 0; slot < occupancy.Length; slot++)
        {
            if ((occupancy[slot] & mask) == 0)
            {
                return slot;
            }
        }
        return occupancy.Length;
    }

    private static int GetSlotCount(List<EventPlacement> placements)
    {
        var slotCount = 0;
        foreach (var placement in placements)
        {
            var count = placement.Slot + 1;
            if (count > slotCount)
            {
                slotCount = count;
            }
        }
        return slotCount;
    }

    private readonly record struct EventCandidate(
        ScheduleEvent Event,
        int StartCol,
        int EndCol,
        bool ContinuesFromPrev,
        bool ContinuesToNext);
}
// ReSharper restore ParameterTypeCanBeEnumerable.Local
