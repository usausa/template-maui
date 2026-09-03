namespace Template.MobileApp.Models.Sample.Calendar;

public sealed class DayView
{
    public required DateOnly Date { get; init; }

    public required bool IsCurrentMonth { get; init; }

    public required bool IsToday { get; init; }

    public required DayKind Kind { get; init; }

    public required IReadOnlyList<Stamp> Stamps { get; init; }
}

public sealed class WeekView
{
    public required IReadOnlyList<DayView> Days { get; init; }

    public required IReadOnlyList<EventPlacement> EventPlacements { get; init; }

    public required int SlotCount { get; init; }
}

public sealed class MonthView
{
    public required int Year { get; init; }

    public required int Month { get; init; }

    public required DateOnly Today { get; init; }

    public required IReadOnlyList<WeekView> Weeks { get; init; }
}
