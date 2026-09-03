namespace Template.MobileApp.Models.Sample.Calendar;

public sealed class TimetableEvent
{
    public required string Title { get; init; }

    public required TimeSpan Start { get; init; }

    public required TimeSpan End { get; init; }

    public required Color Color { get; init; }
}
