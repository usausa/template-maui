namespace Template.MobileApp.Models.Sample.Calendar;

public sealed class TimetableDay
{
    public required DateOnly Date { get; init; }

    public string DowText => Date.ToString("ddd");

    public int Day => Date.Day;

    public bool IsToday => Date == DateOnly.FromDateTime(DateTime.Today);
}
